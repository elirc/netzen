# 6. Solutions and review

Exercise 0 has two separate answers. A customer's internal-marker search is stopped by the store's public-comments-only predicate before the result reaches permission-aware projection; the observable result is zero items and total zero. Two writers submitting version 3 meet the store's write lock; one working copy commits as version 4 and the other receives `ConcurrencyException` saying the current version is 4. Search safety is read-path policy; version safety is write-path coordination.

Exercise 1's customer-comment path carries `ticketId`, `body`, `visibility: Public`, and `expectedVersion`. `AddCommentHandler` calls `CanComment`, `Ticket.AddComment` validates the visibility and body, the working copy's version increments, and the store replaces its snapshot. The notifier sends only `tickets.changed`. Detail mapping includes public comments because the customer can read the ticket. Internal comments require `CanReadInternalNotes`.

Exercise 2 needs two safeguards: source-page aggregation before final pagination preserves the authorized population when the one visible ticket occurs after 100 source records, while the store predicate excludes internal comment bodies from text matches. The handler's `visible.Count` is therefore meaningful for the current user. A future database query should push authorization into the query; the current in-memory loop is not an atomic snapshot across writes.

Exercise 3 normalizes `{" Billing ", "billing", "VIP"}` to sorted `billing`, `vip`. `ReplaceTags` increments version once after successful replacement. If enumeration throws, the original `alpha`, `beta` list and version remain unchanged. `tickets.changed` has no ticket ID, customer email, comment, or other fields; it is an invalidation method sent with **zero** arguments. The `cancellationToken` passed to `SendAsync("tickets.changed", cancellationToken)` binds to the overload's `CancellationToken` parameter; it cancels the send and is not serialized to clients.

Exercise 4 must assert both `result.Total == 1` and `result.Items[0].Number == "T-000001"`; total alone could pass while page slicing returned the wrong item. Missing live checks include a real SignalR connection and group membership, an HTTP test server, browser rendering, database query behavior, and authentication or tenant enforcement.

Exercise 5e's callback calls `void refreshTickets()`, reads `selectedIdRef.current`, then increments `detailRequest` and captures its number (`App.tsx:72-78`). `getTicket(currentId).then` sets selected detail only when `active && request === detailRequest`; its catch uses the same guard. `connectTicketHub` handles reconnect by invoking `JoinInbox` and calling `onChanged` after the invocation resolves. Drafts that survive are `commentBody`, `visibility`, `tagDraft`, `assigneeDraft`, and `teamDraft`.

Exercise 6's database replacement must define one authorized result population before Count, Skip and Take. Express the existing read policy in the query — all four clauses of `CanReadTicket`, with Admin *and* Manager unrestricted — keep internal comment bodies out of customer text matches, and use a deterministic order with an ID tie-breaker. Test the same 121-ticket case against a disposable database and add page-boundary, total-count, role, team and internal-marker cases. Document whether pages are a snapshot or may shift during writes; the current implementation does not promise a cross-page snapshot. Public-comment-only text search currently applies to staff too, so restoring internal-note search for staff requires an explicit actor-aware query rather than removing the predicate globally.

Exercise 7's timeline starts with an authorized agent submitting an internal note at version 3. The store locks, validates version, mutates a copy, then commits version 4. The notifier emits only tickets.changed. Each browser refetches its authorized list and selected detail. The hub callback calls setSelected for a current detail response while leaving commentBody and the other draft variables unchanged. A second writer holding version 3 fails at the store version comparison before its mutation; no successful notification follows that rejected command. A notification arriving during downtime is not durably replayed: reconnect rejoins and refetches current state instead.

Exercise 8's evidence note should name `root-dotnet-test-01` with 3 xUnit tests and TRX (2026-09-12), the 4/4 fix-pass re-run (2026-09-17) after `Create_RejectsUndefinedPriority` was added, and `root-web-build-02` with a passing Vite build. It should say that no live hub, browser, test server, database, or real authentication was exercised. The backend proves focused domain and search behavior; the frontend build proves compilation and bundling. Neither proves a durable outbox, atomic cross-page search snapshot, or complete actor policy.

Review the implementation against the actual fields: `ExpectedVersion`, `CommentVisibility`, `TicketStatus`, `TicketPriority`, `TicketSummaryDto`, `TicketDetailDto`, `filters`, `selectedId`, and the five draft states. Verify every mutation uses the selected version, every permission check receives the current ticket, and errors do not silently update an abandoned request. Check that `upsertSummary` is used for local mutation results while hub events always refetch. Treat `X-User-*` headers as demo identity only. Do not claim complete authorization, durable notifications, or production persistence from this sample. (The creation enum guard *is* now present and tested: `Ticket.cs:101`, `Create_RejectsUndefinedPriority`.)

## Verified review findings (source-read 2026-10-06; no dotnet or browser run)

**F1 — the user-driven detail path has no request guard, and the hub guard does not know about it.** `openTicket` (`App.tsx:124-137`) awaits `getTicket(id)` and then calls `setSelectedId(id)` and `setSelected(detail)` unconditionally. The hub callback's guard (`App.tsx:79`) only compares against its own `detailRequest` counter. Two consequences: (a) click A then quickly click B; if A's response arrives last, `selected` shows A; (b) a hub refetch of A that is in flight when the user opens B still satisfies `request === detailRequest` and overwrites `selected` with A while `selectedId` is B. Every mutation sends `selected.id` and `selected.version` (`App.tsx:144, 157, 169, 183`), while `openTicket` has just reset `tagDraft`/`assigneeDraft`/`teamDraft` from B — so in case (b) saving tags or assignment applies B's drafts to ticket A, and the version check passes because A's version is current. Fix direction: one shared detail-request counter, bumped by both paths, and compare the response id with `selectedIdRef.current`.

**F2 — the list counter advances twice per filter change.** The `[filters]` effect's cleanup (`App.tsx:62`) *and* `refreshTickets` (line 108) both increment `listRequest`. It is harmless (the guard only needs monotonicity) but it is why exercise 5a's numbers jump by two.

**F3 — staff cannot search internal notes.** `InMemoryTicketStore.cs:67` restricts comment text matches to `CommentVisibility.Public` for every caller. That closes the customer leak; it also removes a feature for agents. An actor-aware predicate is the real fix (see exercise 6).

**F4 — unexpected exceptions leak their message to the client.** `ApiExceptionHandler.cs:19-27` maps known exceptions to 404/403/409/400 and everything else to 500, but line 39 sets `Detail = exception.Message` for *all* of them, including the 500 arm. The client then shows `problem.detail` verbatim (`api.ts:37-38`). For the 409 that is useful ("Ticket version is 4; refresh before applying changes based on version 3.", `InMemoryTicketStore.cs:41`); for an unexpected exception it can expose internals. Fix direction: a generic detail for the `_ =>` arm, with the real message logged server-side.

## The method behind exercise 3 and answer 5c

This is `ReplaceTags`, `support-ticketing-app/backend/SupportDesk.Api/Domain/Ticket.cs:229`. `ToArray()` is the whole answer to 5c: the new list is fully materialised before `_tags.Clear()` runs, so a throw during enumeration cannot leave a half-updated aggregate.

```csharp
    public void ReplaceTags(IEnumerable<string> tags, DateTimeOffset now)
    {
        if (tags is null) throw new DomainException("Tags are required.");
        var normalized = tags.Select(tag => NormalizeTag(tag ?? string.Empty))
            .Where(tag => tag.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase).Order().ToArray();
        _tags.Clear();
        _tags.AddRange(normalized);
        Touch(now);
    }
```

## Exercise 5 walkthrough (moved from chapter 05)

This is the implementation narration that used to sit inside the practice chapter. Read it after attempting the task list there. Note that the third paragraph describes the *delivered* hub effect with `[selectedId, filters]` dependencies; that effect has since been changed to an empty dependency list with refs, for the reasons in chapter 07 — the rest of the narration still holds.

First, make every notifier method call the group with the data-free event. `TicketCreatedAsync`, `TicketUpdatedAsync`, and `TicketCommentedAsync` all call `SendAsync("tickets.changed", cancellationToken)`. Do not add a ticket ID or comment body. A broad `inbox` group can contain users with different access, and a comment body could be internal. The client must refetch through `listTickets` and `getTicket`, which apply current headers, permissions, public-comment search, and filters.

Second, preserve ordering in `refreshTickets`. `App` already keeps `const listRequest = useRef(0)`. Increment it at the beginning, capture the number, set `loading`, and clear `error`. In `try`, commit `page.items` only when the captured request equals `listRequest.current`; apply the same condition in `catch` and `finally`. The `[filters]` effect invokes refresh and its cleanup increments the counter, so an old query cannot replace a newer filter result. This is a list sequence guard, not a distributed transaction.

Third, inspect the hub effect. `connectTicketHub` registers `tickets.changed`, reconnect callbacks, and `onreconnected`; the latter invokes `JoinInbox` and then calls the supplied refresh callback. In `App`, the effect keeps `active`, `detailRequest`, `selectedId`, and `filters` in its closure. A changed event refreshes the list and starts a detail request for the selected ID. Only an active effect and current detail sequence may call `setSelected` or `setError`. Cleanup sets `active = false` and stops the connection. The dependency list includes `[selectedId, filters]`, so current selection and filters are used by the closure.

Finally, keep drafts independent. `commentBody`, `visibility`, `tagDraft`, `assigneeDraft`, and `teamDraft` have separate state variables. A live detail refetch calls `setSelected(detail)` but does not overwrite those drafts. This lets an agent type an internal note or edit tags while a notification refreshes server data. A successful mutation updates the selected detail and uses `upsertSummary` for the list, preserving the expected-version workflow.

Check the request adapter while making the repair. `listTickets` always starts with `page=1&pageSize=50`, then adds only nonempty query, status, priority, tag, and breached values. It does not expose `assigneeId` or `teamId` filters from this screen. Every request adds the same demo headers, so a refetch is evaluated as the current fixture user. This makes refetching safer than accepting a hub payload, but it does not make those headers a security boundary. Preserve the exact JSON field names `expectedVersion`, `visibility`, `assigneeId`, and `teamId` when writing mutation examples.

### Answers to 5a-5f and the lab

These were corrected on 2026-10-06 after re-reading the source (no dotnet or browser run): the earlier 5a numbers ignored the effect cleanup, the 5c "actual" value ignored `Order()` buffering, and the lab answer put `T-000001` on the wrong source page.

**5a.** Each filter change increments the counter **twice**: the old effect's cleanup (`App.tsx:62`) and then `refreshTickets` (line 108). From `n`: change to A → `n+2`, A's request captured `n+2`, `tickets` unchanged, `loading` true. Change to B → `n+4`, B captured `n+4`, unchanged, loading true. Response A arrives: `n+2 !== n+4`, nothing committed — not even `setLoading(false)` (line 120 is guarded too). Response B arrives: `n+4 === n+4`, `tickets` becomes B's items and loading clears. (A hub event in between would add one more increment and supersede B as well; the hub's own refresh then commits.)

**5b.** With `[selectedId, filters]` you get one "hub stop" + one "hub connect" pair per keystroke (five pairs for five characters), plus one per selection change; with `[]` you get a single "hub connect" for the whole session. In `npm run dev`, `<StrictMode>` (`main.tsx:7`) adds one extra connect/stop/connect at start-up in both runs. Each pair is a WebSocket teardown, a new handshake, a `JoinInbox` round trip and a window in which an event reaches nobody; multiplied by 500 connected agents typing, that is a self-inflicted load spike on the hub and a stream of missed invalidations.

**5c.** `Assert.Throws` passes and the **version** assertion passes — `Touch(now)` comes after `AddRange`, so the throw skips it. The **tag contents** assertion fails: expected `["alpha", "beta"]`, actual **empty**. Not `["changed"]`: the snapshot's lazy chain ends in `.Order()`, and an ordering operator must buffer its entire source before yielding anything, so the iterator throws during that buffering — after `_tags.Clear()` has run but before a single element reaches `AddRange`. The aggregate is not "half-updated", it is emptied with the version unchanged, which is arguably worse: a silent data loss with no version bump for a concurrent editor to notice. (If you drop `.Order()` from the reverted chain, the actual becomes `["changed"]`.) The materialising version builds the array *first* and only then swaps it in. This answer is reasoned from LINQ semantics, not executed here; run the exercise to confirm.

**5d.** Only `Create_RejectsUndefinedPriority` fails. A priority-99 ticket falls through both SLA `switch` expressions to their `_ =>` arm and receives the *Low* due dates (`now.AddBusinessHours(8)` and `now.AddDays(5)`) while reporting priority 99 — a row that no `UpdatePriority` call would ever have been allowed to produce.

**5e.** See the Exercise 5e paragraph near the top of this chapter.

**5f.** See finding F1 above: `selected` can end on A while `selectedId` is B, and the next tag or assignment save applies B's drafts to A.

**Lab.** The store orders by `UpdatedAt` descending (`InMemoryTicketStore.cs:101`). `T-000001` is created and commented at `now`; the other 120 tickets are created at `now.AddSeconds(2..121)`, so they all sort ahead of it and `T-000001` is row 121 — on source page **2**. With the one-source-page shape, page 1 contains no ticket the customer may read, so `visible` is empty: `Total` 0, `Items.Count` 0. The first failing assertion is `Assert.Equal(1, result.Total)` (expected 1, actual 0). The marker half of the test would still pass, because the store's public-comments-only predicate is untouched. The fix is the loop that keeps requesting source pages until the source total is exhausted, applying `CanReadTicket` before the final `Skip`/`Take`. Chapter 02's "pushing authorization into the query" section shows how a database does the same thing in one statement.
