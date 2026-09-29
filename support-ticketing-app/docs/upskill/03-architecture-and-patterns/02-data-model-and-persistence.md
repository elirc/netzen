# 02 Data Model And Persistence

## Current Model

The `Ticket` aggregate owns core state at `backend/SupportDesk.Api/Domain/Ticket.cs:50-70`: identity, number, subject, customer data, status, priority, assignment, SLA timestamps, tags, comments, and version. Comments are child objects at `backend/SupportDesk.Api/Domain/TicketComment.cs:3-26`. The enum vocabulary lives at `backend/SupportDesk.Api/Domain/TicketEnums.cs:3-33`.

## Current Persistence

The store contract is `ITicketStore` at `backend/SupportDesk.Api/Application/ITicketStore.cs:5-12`. The active implementation is singleton in-memory storage registered at `backend/SupportDesk.Api/Program.cs:35` and implemented at `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:7-120`.

| Concern | Evidence | Judgment |
| --- | --- | --- |
| Identity | `Guid Id` at `backend/SupportDesk.Api/Domain/Ticket.cs:50`; ticket number from sequence at `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:11-13` | Good split between internal id and human ticket number. |
| Snapshot isolation | `Ticket.Snapshot()` at `backend/SupportDesk.Api/Domain/Ticket.cs:136-156`; store uses snapshots at `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:18-25` | Prevents callers from mutating stored object directly. |
| Optimistic concurrency | Version check at `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:39-42`; version increments at `backend/SupportDesk.Api/Domain/Ticket.cs:232-236` | Transferable to EF row versions. |
| Search | Full scan and filters at `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:55-107` | Fine for lab; would need DB indexes/query composition in production. |
| Seed data | `backend/SupportDesk.Api/Infrastructure/SeedData.cs:18-57` | Useful for local demos; not durable. |

## EF Core Extension Target

No EF Core package, `DbContext`, migrations folder, or database connection config exists. Evidence: `.csproj` has no package references at `backend/SupportDesk.Api/SupportDesk.Api.csproj:1-8`; active store registration is in-memory at `backend/SupportDesk.Api/Program.cs:35`.

If adding EF later:

1. Create `SupportDeskDbContext`.
2. Map `Ticket` and comments either as entities or persistence models.
3. Add indexes for status, priority, assignee, team, tags, and updated date based on search filters at `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:58-104`.
4. Use a row version or concurrency token corresponding to `Ticket.Version` at `backend/SupportDesk.Api/Domain/Ticket.cs:66`.
5. Keep permission filters near the query so `SearchTicketsHandler` does not page before auth as it currently does at `backend/SupportDesk.Api/Application/Queries/SearchTickets.cs:17-28`.

## Safe Schema Change Guidance

Illustrative fake code: not from this repo.

```csharp
// Illustrative fake code: not from this repo.
builder.Entity<TicketRow>()
    .Property(x => x.RowVersion)
    .IsRowVersion();
```

Migration discipline:

- Backward-compatible add first.
- Deploy code that writes both old and new shape.
- Backfill.
- Read from new shape.
- Remove old shape after confidence.

Drill: design a migration for normalized `Tags` using current tag behavior at `backend/SupportDesk.Api/Domain/Ticket.cs:219-238`.
