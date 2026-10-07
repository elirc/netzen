# 5. Practice: repair stale inbox behavior

This chapter states the task; the step-by-step narration that used to be here is now the answer key in [`06-SOLUTIONS-AND-REVIEW.md`](06-SOLUTIONS-AND-REVIEW.md) under "Exercise 5 walkthrough (moved from chapter 05)". Try the tasks before reading it.

## The scenario

A branch puts a `TicketSummary` inside the SignalR event and lets the browser upsert that object directly. Its refresh effect accepts whichever filtered response resolves last. The files you may change: [`App.tsx`](../support-ticketing-app/frontend/src/App.tsx), [`api.ts`](../support-ticketing-app/frontend/src/api.ts), [`SignalRTicketNotifier.cs`](../support-ticketing-app/backend/SupportDesk.Api/Realtime/SignalRTicketNotifier.cs).

## Acceptance criteria

1. No notifier method sends ticket data. `TicketCreatedAsync`, `TicketUpdatedAsync` and `TicketCommentedAsync` all send `"tickets.changed"` with no payload, so a group containing users of different roles cannot receive a body one of them may not read.
2. Every client refetch goes through `listTickets` / `getTicket`, so permissions, public-comment search and current filters are applied by the server.
3. An older list response cannot replace a newer one (list request number), and an older detail response cannot call `setSelected` (detail request number).
4. One SignalR connection per mount. Changing filters or selection must not stop and restart it.
5. The hub callback acts on the *current* filters and selection, read through refs, not on the values captured when it was created.
6. A hub-triggered refresh leaves `commentBody`, `visibility`, `tagDraft`, `assigneeDraft` and `teamDraft` untouched.
7. Reconnect re-invokes `JoinInbox` and then refreshes.

## Exercises (do these before reading chapter 06)

Every exercise has a **Goal** and a **Check**. There is no frontend test runner, so frontend checks are either source-based (cite the line) or hand-run in `npm run dev`; backend checks run `dotnet test` (four `[Fact]` cases on an unmodified tree).

**Exercise 5a (predict).** **Goal:** the filter inputs call `setFilters` on every change (`App.tsx:232-259`), which re-runs the `[filters]` effect at lines 60-63. Starting from a mounted page where `listRequest.current` is `n`, change the filter to A, then to B; then response A arrives, then response B. Write `listRequest.current` and `tickets` after each of the four steps. Watch both places that increment the counter. **Check (source):** your numbers must account for the effect cleanup on line 62 *and* the `++listRequest.current` on line 108, and for the guard on line 116.

**Exercise 5b (hand-run).** **Goal:** in `App.tsx`, temporarily put `[selectedId, filters]` back on the hub effect (line 94). Add a `console.log("hub connect")` inside the effect body and a `console.log("hub stop")` in its cleanup, run `npm run dev`, and type five characters into the search box. Count the log lines. Then restore `[]` and repeat. **Check (hand-run):** record both counts; note that `main.tsx` wraps `App` in `<StrictMode>`, which in development mounts, unmounts and re-mounts effects once at start-up, so expect one extra connect/stop pair in *both* runs. Write one sentence on what the difference costs a server with 500 agents connected.

**Exercise 5c (code).** **Goal:** revert `Ticket.ReplaceTags` to the snapshot's single-expression form, `_tags.Clear(); _tags.AddRange(tags.Select(NormalizeTag).Where(tag => tag.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase).Order());` ([`snapshots/Ticket-before.cs.txt`](snapshots/Ticket-before.cs.txt), line 222). **Predict which assertion of `ReplaceTags_DoesNotPartiallyMutate_WhenEnumerationFails` fails first** — the version equality or the tag contents — and the exact *actual* value reported. Hint: what does `Order()` have to do before it can yield its first element? **Check (runnable):**

```powershell
$env:DOTNET_ROLL_FORWARD = "Major"
dotnet test support-ticketing-app/backend/SupportDesk.Api.Tests/SupportDesk.Api.Tests.csproj -c Release --filter FullyQualifiedName~ReplaceTags
```

and compare. Restore the materialising version and re-run to green.

**Exercise 5d (code).** **Goal:** delete the `Enum.IsDefined` guard from `Ticket.Create` (`Ticket.cs:101-104`) and predict the suite result. Then, *without* restoring it, work out what due dates a priority-99 ticket receives by reading the two SLA switches (lines 106-119). **Check (runnable):** `dotnet test` reports exactly one failure, `Create_RejectsUndefinedPriority`. Restore the guard and confirm 4/4.

## Lab: red-first against the original code

The delivered change is described in chapter 03. Reproduce its failures before trusting it:

1. Restore the pre-change `SearchTicketsHandler` body from [`snapshots/SearchTickets-before.cs.txt`](snapshots/SearchTickets-before.cs.txt): one source page of 100, filter by permission, then `Skip`/`Take`. Leave the store as it is.
2. Predict what `Search_UsesAuthorizedPopulationAcrossSourcePages_AndHidesInternalMarkers` reports for `result.Total` and `result.Items.Count` with the 121-ticket fixture. To predict, you need to know which source page `T-000001` lands on: read the store's ordering (`InMemoryTicketStore.cs:101`) and the `UpdatedAt` each fixture ticket gets.
3. Run the suite, record the exact failure message, and check your prediction.
4. Restore the delivered handler, re-run to green, and write one sentence naming the line that fixed it.

**Check (runnable):** step 3's failure message matches your step-2 prediction, and step 4 is back to 4/4.

**Exercise 5e (write).** **Goal:** rewrite a hypothetical `tickets.changed` callback so it refetches the list and selected detail while suppressing a stale detail response. Include the reconnect sequence `JoinInbox` then refresh, and list the draft states that must survive. **Check (source):** compare with `App.tsx:69-94` and `api.ts:124-138`; your version must read selection through a ref, guard `setSelected` with a request number, and leave all five draft states untouched.

**Exercise 5f (senior, source).** **Goal:** user clicks ticket A; a `tickets.changed` event arrives; while the hub's `getTicket(A)` is in flight the user clicks ticket B and `openTicket(B)` completes first. Trace which ticket `selected` shows at the end and what `selectedId` holds. **Check (source):** your trace cites `openTicket` (`App.tsx:124-137`, which bumps no request counter) and the hub guard on line 79 (`active && request === detailRequest`); compare with finding F1 in chapter 06.

**Exercise 6 (design review).** **Goal:** review a future change that pushes permission filtering into a database query. Write the invariant it must preserve, the visibility condition it must retain, and the evidence needed before replacing the current all-pages in-memory approach. **Check (review):** your predicate matches `Permissions.CanReadTicket` (`Permissions.cs:7-11`) clause for clause, including Manager, and keeps the public-comments-only text match (`InMemoryTicketStore.cs:67`).
