# SupportDesk safety course

This nine-file course follows a support ticket through domain mutation, optimistic concurrency, permission-aware search, a data-free SignalR invalidation, and the React refresh that follows it. It is written against the project at [`../`](../). Read chapters 1 through 7 in sequence, then use [`VERIFICATION.md`](VERIFICATION.md) to reproduce the checks from the project folders.

The source of truth is the executable project under [`../support-ticketing-app/`](../support-ticketing-app/). The domain aggregate is [`Ticket.cs`](../support-ticketing-app/backend/SupportDesk.Api/Domain/Ticket.cs). It owns `Status`, `Priority`, `Tags`, `Comments`, `Version`, SLA timestamps, and mutation rules. [`InMemoryTicketStore.cs`](../support-ticketing-app/backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs) stores snapshots, locks writes, checks `expectedVersion`, and searches a deliberately small in-memory population. Application handlers enforce the current policy in [`Permissions.cs`](../support-ticketing-app/backend/SupportDesk.Api/Application/Permissions.cs), while [`SearchTickets.cs`](../support-ticketing-app/backend/SupportDesk.Api/Application/Queries/SearchTickets.cs) gathers source pages before final permission-aware pagination.

The browser code is [`App.tsx`](../support-ticketing-app/frontend/src/App.tsx) and [`api.ts`](../support-ticketing-app/frontend/src/api.ts). App state keeps server data separate from drafts: `commentBody`, `visibility`, `tagDraft`, `assigneeDraft`, and `teamDraft` survive a list refresh. A list request number suppresses an older filter response. The SignalR connection listens for `tickets.changed`, refetches the list, and refetches the selected detail while retaining those draft values. Reconnect invokes `JoinInbox` before the refresh callback; this restores group membership after reconnect.

Keep the evidence boundaries explicit. The recorded backend check is `root-dotnet-test-01`: three xUnit tests pass with TRX under .NET 8 targeting, using `DOTNET_ROLL_FORWARD=Major` on a .NET 10 runtime. It covers tag enumeration failure, undefined status/priority/visibility, and a 121-ticket customer search where the matching ticket is on a later source page and an internal marker produces no customer result. A later fix pass added a fourth case, `Create_RejectsUndefinedPriority`, which closes the enum guard that was missing from `Ticket.Create`; the fix-pass re-run reported 4/4 (observed 2026-09-17), and the test file still holds exactly four `[Fact]` methods (counted 2026-10-06). `root-web-build-02` passes after React type dependencies were added; the existing `vite-env.d.ts` is retained. There is no live SignalR test, test server, browser automation, database, durable outbox, or real authentication evidence.

## Prerequisites and glossary

**Prerequisites:** C# with `async`/`await`; React function components using `useState`, `useEffect` and `useRef`; a LINQ chain; the difference between a request and a socket. The sibling `netgum` course is a natural predecessor.

| term | meaning | where in this project |
| --- | --- | --- |
| aggregate | the object loaded, changed and saved as one unit | `Ticket` in `support-ticketing-app/backend/SupportDesk.Api/Domain/Ticket.cs` |
| invariant guard | the code keeping a rule true, such as rejecting an undefined priority | `Ticket.Create` and `UpdatePriority` in `Domain/Ticket.cs` |
| expected version | the version a writer read, compared before the mutation applies | the `expectedVersion` parameter of `UpdateAsync` in `Infrastructure/InMemoryTicketStore.cs` |
| concurrency conflict | the rejection when another writer got there first, returned as 409 | `ConcurrencyException` in `Infrastructure/InMemoryTicketStore.cs` |
| deferred execution | a LINQ query does nothing until enumerated, so the throw lands late | the tag sequence in `Ticket.ReplaceTags`, dissected in `03-WORKED-CHANGE.md` |
| permission policy | the per-action authorization rules, separate from authentication | `Permissions.CanReadTicket` in `Application/Permissions.cs` |
| source page | the chunk pulled from the store to filter, not the page the user asked for | the `sourcePage` loop in `Application/Queries/SearchTickets.cs` |
| authorization before pagination | filter by who may see a row, then count and page the survivors | `CanReadTicket` preceding `Skip`/`Take` in `SearchTickets.cs` |
| data-free invalidation | the event says only that something changed; each client refetches | `SendAsync("tickets.changed", ...)` in `Realtime/SignalRTicketNotifier.cs` |
| group membership | the named set of connections a broadcast reaches, rejoined on reconnect | `JoinInbox` in `Realtime/TicketHub.cs` |
| stale response guard | the increasing request number that discards an older in-flight reply | the `listRequest` ref in `frontend/src/App.tsx` |
| draft state | what the user typed but has not sent, which survives a refresh | `commentBody` and `tagDraft` in `frontend/src/App.tsx` |

## Before you start

**What you need installed.**

| Tool | Version used | Unlocks |
|---|---|---|
| .NET SDK 10.x (10.0.302 here) | the projects target `net8.0` | `dotnet test` (4 xUnit cases). With no .NET 8 runtime installed, set `DOTNET_ROLL_FORWARD=Major` first, or the test host aborts with "You must install or update .NET to run this application". |
| Node 22.x | | `npm install` + `npm run build` in `support-ticketing-app/frontend` (a TypeScript/bundle check, **not** a test) and `npm run dev` for the two browser exercises |

No database server, no Docker, no Python. There is no frontend test runner in this project: every frontend claim in this course is source-reviewed or hand-run, and `VERIFICATION.md` says so.

**Time estimate (about 4-6 hours, two sittings).**

| Chapter | Estimate |
|---|---|
| 01 Codebase map | 20 min |
| 02 Concepts (incl. the EF Core section) | 60 min |
| 03 Worked change | 45 min |
| 04 Testing and debugging | 40 min |
| 05 Practice (4 exercises + red-first lab) | 90 min |
| 06 Solutions and review | 30 min |
| 07 Trace lab | 40 min |
| VERIFICATION | 10 min |


The eight reading checkpoints are:

1. [`01-CODEBASE-MAP.md`](01-CODEBASE-MAP.md): request, mutation, and notification boundaries.
2. [`02-CONCEPTS.md`](02-CONCEPTS.md): atomic aggregate edits, permissions, visibility, and pagination.
3. [`03-WORKED-CHANGE.md`](03-WORKED-CHANGE.md): current C# and TypeScript patterns with actual field names.
4. [`04-TESTING-AND-DEBUGGING.md`](04-TESTING-AND-DEBUGGING.md): complete backend test fixture and focused debugging evidence.
5. [`05-PRACTICE.md`](05-PRACTICE.md): repair exercises for search, tags, SignalR, and stale React state.
6. [`06-SOLUTIONS-AND-REVIEW.md`](06-SOLUTIONS-AND-REVIEW.md): answers for exercises 0-8 (including 5a-5e and the red-first lab), verified review findings, and review limits.
7. [`07-TRACE-LAB.md`](07-TRACE-LAB.md): concrete traces for customer visibility, concurrency, and reconnect.
8. [`VERIFICATION.md`](VERIFICATION.md): portable restore, test, build, and documentation checks.

**Exercise 0.** Predict the result when a customer searches for `INTERNAL-ONLY-MARKER` and when two writers submit version 3. Name the first policy boundary in each path and the observable result.
