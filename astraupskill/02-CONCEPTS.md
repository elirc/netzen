# 2. Visibility before pagination

Search has two populations: records returned by the store and records the current user may read. The store applies text, status, priority, assignment, team, tag, and SLA filters. [`SearchTickets.cs`](../support-ticketing-app/backend/SupportDesk.Api/Application/Queries/SearchTickets.cs) then requests source pages of up to 100, filters each ticket with `Permissions.CanReadTicket`, maps visible tickets to `TicketSummaryDto`, and only then applies the caller's requested `Page` and `PageSize`. It returns `visible.Count` as the total. This order matters. If page one contains 100 records the customer cannot read, a matching record on source page two would disappear from the final page and the total would describe the wrong population.

The current implementation is intentionally suitable for a small in-memory store. It gathers every source page based on the store's `Total` and filters in application memory. A future database implementation should push authorization into the query and make the permission predicate part of the database-side population. It still needs a documented consistency model: the current loop is not an atomic snapshot across writes, so records can change between source pages. Do not claim cross-page snapshot isolation from this code.

Text visibility is a separate safeguard. [`InMemoryTicketStore.cs`](../support-ticketing-app/backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs) searches `Number`, `Subject`, `Description`, `CustomerName`, `CustomerEmail`, and only comments whose `Visibility == CommentVisibility.Public`. An internal comment body cannot make a customer's search return a ticket, even if the customer happens to know the marker. Detail mapping in [`Dto.cs`](../support-ticketing-app/backend/SupportDesk.Api/Application/Dto.cs) includes public comments for everyone who can read the ticket and internal comments only when `Permissions.CanReadInternalNotes` is true.

The role policy is explicit in [`Permissions.cs`](../support-ticketing-app/backend/SupportDesk.Api/Application/Permissions.cs). Admins and Managers can read broadly. An assignee, a member of the ticket's team, or the customer whose user ID equals `CustomerEmail` can read. Internal commenting requires staff role plus ticket access; public commenting also allows the customer on their own ticket. This is a demo policy, not a complete authorization system. Header identity, tenant boundaries, audit, and authentication remain outside the evidence.

The same policy is consulted again on writes. `CanTag`, `CanAssign`, and `CanChangeStatus` allow Admins and Managers, or an Agent who can read the ticket. `CanComment` receives the requested visibility, so a customer cannot switch a public reply to an internal note by changing one JSON enum. `TicketMapping.ToDetailDto` filters comments independently of the command check. This repetition is intentional: authorization is a property of each operation and representation, not a one-time decision made when the row was first loaded. A UI checkbox may guide an agent, but it cannot substitute for these server checks.

**Exercise 2.** Explain why a 121-record store can return a customer's one ticket when it is on source page two, yet return zero results for an internal-only marker. State the two distinct safeguards and where each is implemented.

## Pushing authorization into the query (what this code should become)

The fix you will read in chapter 03 is correct about the *rule* — filter by permission before you paginate — and deliberately wrong about the *mechanism*. `SearchTicketsHandler` walks source pages of 100, materialises them, applies `Permissions.CanReadTicket` in a `foreach`, and then pages the survivors. With 121 fixture tickets that is fine. With 121,000 rows it is a table scan in application memory on every keystroke, and it is the single thing in this project you must not copy into a job.

The database can do all of it. The shape to learn is: build one `IQueryable`, apply the ownership predicate **first**, count and page **on the same query**, and only then materialise.

```csharp
// EF Core version of the same rule. Nothing runs until CountAsync/ToListAsync.
IQueryable<Ticket> query = db.Tickets.AsNoTracking();

// 1. Authorization is a WHERE clause, not a loop. This mirrors Permissions.CanReadTicket
//    (Permissions.cs:7-11) clause for clause: Admin/Manager see everything; anyone else sees
//    tickets assigned to them, owned by one of their teams, or raised under their email.
//    (EF's case-insensitive comparison depends on the column collation; CanReadTicket uses
//    OrdinalIgnoreCase for the email clause.)
var teamIds = user.TeamIds.ToList();
if (user.Role is not (SupportRole.Admin or SupportRole.Manager))
{
    query = query.Where(t =>
        t.AssigneeId == user.UserId ||
        (t.TeamId != null && teamIds.Contains(t.TeamId)) ||
        t.CustomerEmail == user.UserId);
}

// 2. Then the user's own filters.
if (!string.IsNullOrWhiteSpace(criteria.Query))
{
    var term = criteria.Query.Trim();
    query = query.Where(t =>
        EF.Functions.Like(t.Subject, $"%{term}%") ||
        EF.Functions.Like(t.Description, $"%{term}%") ||
        // Only public comments participate in search - the same rule as the in-memory version,
        // expressed so the database never returns a row an internal note matched.
        t.Comments.Any(c => c.Visibility == CommentVisibility.Public && EF.Functions.Like(c.Body, $"%{term}%")));
}

// 3. Count and page the SAME filtered query. One SELECT COUNT, one paged SELECT.
var total = await query.CountAsync(cancellationToken);
var items = await query
    .OrderByDescending(t => t.UpdatedAt)
    .Skip((criteria.Page - 1) * criteria.PageSize)
    .Take(criteria.PageSize)
    // Illustrative slim row. The real TicketSummaryDto (Dto.cs:5-25) has 20 positional
    // members, including computed breach flags that need `now`; project those explicitly.
    .Select(t => new { t.Id, t.Number, t.Subject, t.Status, t.Priority, t.UpdatedAt })
    .ToListAsync(cancellationToken);
```

An earlier version of this snippet switched on role and gave `Manager` the `_ => false` arm (managers would have seen nothing) and restricted `Customer` to the email clause, which is not what `CanReadTicket` says. When you translate an in-memory policy into a query, diff the two predicates clause by clause; this is exactly the kind of drift a review must catch.

Three things to notice, because each is a bug class the in-memory loop can hide:

1. **`Total` comes from the filtered query.** The original bug was a total counted over a different population than the items. Deriving both from one `IQueryable` makes that impossible to get wrong by accident.
2. **`Skip`/`Take` come last.** In SQL, `OFFSET`/`FETCH` apply after `WHERE`. If you page first and filter after — which is exactly what the pre-change code did — rows an authorized user should see simply vanish once they fall past the first page boundary.
3. **The projection is inside the query.** `.Select(...)` before `ToListAsync` means the database returns only the projected columns, not whole ticket graphs with their comments. It is also a second authorization safeguard: a column you never select cannot leak.

The same reasoning applies to the other persistence concerns this in-memory project fakes:

| In-memory here | Relational equivalent |
|---|---|
| `_writeLock` around read-modify-write | A transaction plus a concurrency token: `[Timestamp] byte[] RowVersion` (or `Property(t => t.Version).IsConcurrencyToken()`), so `UPDATE ... WHERE Version = @expected` affects zero rows on a stale write and EF raises `DbUpdateConcurrencyException` → HTTP 409/412. |
| `expectedVersion` equality check throwing `ConcurrencyException` | The same `DbUpdateConcurrencyException`, or an explicit `ExecuteUpdateAsync` with `.Where(t => t.Version == expected)` and an affected-row count of 0 meaning "someone else got there first". |
| Snapshot-on-success (`Ticket.Snapshot()`) | `SaveChangesAsync` inside one transaction: either the ticket row, the comment row and the audit row all land, or none do. |
| `store.LoadSeedTickets` | `db.Tickets.AddRange(...)` + `SaveChangesAsync` in a seeder, or EF migrations with `HasData`. |
| Tag list replaced in memory | A `TicketTag` join table, replaced inside the same `SaveChangesAsync`; or a `jsonb`/`ON CONFLICT DO UPDATE` upsert if tags stay denormalised. |

None of that is implemented here. Read the loop as a diagram of the rule, not as a template.
