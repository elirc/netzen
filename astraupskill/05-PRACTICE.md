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

**Exercise 5a (predict).** For the sequence "submit filter A, submit filter B, response A arrives, response B arrives", write the value of `listRequest.current` and of `tickets` after each of the four steps.

**Exercise 5b (code).** In `App.tsx`, temporarily put `[selectedId, filters]` back on the hub effect. Add a `console.log("hub connect")` inside the effect body and a `console.log("hub stop")` in its cleanup, run `npm run dev`, and type five characters into the search box. Count the log lines. Then restore `[]` and repeat. Write down the two counts and one sentence on what the difference costs a server with 500 agents connected.

**Exercise 5c (code).** Revert `Ticket.ReplaceTags` to the single-expression `_tags.Clear(); _tags.AddRange(<lazy query>);` form. **Predict which assertion of `ReplaceTags_DoesNotPartiallyMutate_WhenEnumerationFails` fails first** — the version equality or the tag contents — and what the reported values will be. Then run

```powershell
$env:DOTNET_ROLL_FORWARD = "Major"
dotnet test support-ticketing-app/backend/SupportDesk.Api.Tests/SupportDesk.Api.Tests.csproj -c Release --filter FullyQualifiedName~ReplaceTags
```

and compare. Restore the materialising version and re-run to green.

**Exercise 5d (code).** Delete the `Enum.IsDefined` guard from `Ticket.Create`, run the suite, and confirm `Create_RejectsUndefinedPriority` is the only failure. Then, *without* restoring it, check what due dates a priority-99 ticket receives by reading the two SLA switches. Restore the guard.

## Lab: red-first against the original code

The delivered change is described in chapter 03. Reproduce its failures before trusting it:

1. Using the before/after description in chapter 03, restore the pre-change `SearchTicketsHandler` shape: take one source page of 100, filter by permission, then `Skip`/`Take`.
2. Predict what `Search_UsesAuthorizedPopulationAcrossSourcePages_AndHidesInternalMarkers` reports for `result.Total` and `result.Items.Count` with the 121-ticket fixture.
3. Run the suite, record the exact failure message, and check your prediction.
4. Restore the delivered handler, re-run to green, and write one sentence naming the line that fixed it.

**Exercise 5.** Rewrite a hypothetical `tickets.changed` callback so it refetches the list and selected detail while suppressing a stale detail response. Include the reconnect sequence `JoinInbox` then refresh, and list the draft states that must survive.
