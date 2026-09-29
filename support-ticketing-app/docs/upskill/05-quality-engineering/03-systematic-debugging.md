# 03 Systematic Debugging

Use this loop: reproduce, narrow layer, hypothesize, test cheaply, fix root cause, add regression coverage.

## Scenario: Comment Fails With Concurrency Conflict

**Reproduction:** Open the same ticket in two tabs, add a comment in tab A, then add a comment in tab B without refreshing.

**First question:** Is this expected stale state or a broken store?

**Narrowing path:**

1. Confirm UI sends `selected.version` at `frontend/src/App.tsx:116`.
2. Confirm API sends `expectedVersion` at `frontend/src/api.ts:74-83`.
3. Confirm store check at `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:39-42`.
4. Confirm 409 mapping at `backend/SupportDesk.Api/Auth/ApiExceptionHandler.cs:21-23`.

**Useful probes:** Network panel response body, breakpoint in `InMemoryTicketStore.UpdateAsync`.

**Regression test to add:** API test for stale version returns 409.

**Senior lesson:** Conflict is not a bug by itself; missing refresh UX may be.

## Scenario: Customer Sees Internal Note

**Reproduction:** Use customer user id matching `CustomerEmail`, fetch ticket detail after internal note.

**First question:** Server leak or UI filtering failure?

**Narrowing path:** Check read filter at `backend/SupportDesk.Api/Application/Dto.cs:92-120`, permission rule at `backend/SupportDesk.Api/Application/Permissions.cs:36-38`, user identity at `backend/SupportDesk.Api/Auth/HeaderUserContext.cs:12-20`.

**Regression test to add:** Customer detail excludes internal comments.

**Senior lesson:** Sensitive data must be filtered before it crosses the network.

## Scenario: Live Updates Duplicate

**Reproduction:** Select several tickets, then create/comment from another tab and observe repeated row updates.

**First question:** Server double-send or client double-subscribe?

**Narrowing path:** Check server event send at `backend/SupportDesk.Api/Realtime/SignalRTicketNotifier.cs:15-22`; check client connection effect at `frontend/src/App.tsx:58-69`; verify cleanup at `frontend/src/App.tsx:66-68`.

**Regression test to add:** UI-level test is hard without tooling; add manual verification and later component/hook test.

**Senior lesson:** Effect dependencies are part of architecture in real-time UIs.

## Scenario: Search Results Missing Tickets

**Reproduction:** Search with a tag/team and compare to seeded tickets.

**First question:** Store filter, permission filter, or UI filter?

**Narrowing path:** Seed data at `backend/SupportDesk.Api/Infrastructure/SeedData.cs:18-57`; store filters at `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:58-104`; auth filter at `backend/SupportDesk.Api/Application/Queries/SearchTickets.cs:19-21`; UI render at `frontend/src/App.tsx:244-265`.

**Regression test to add:** Search returns only visible tickets and correct total.

**Senior lesson:** Query correctness includes authorization semantics.

## Scenario: Canned Reply Does Not Appear

**Reproduction:** Add a canned reply and reload UI.

**First question:** Server data, endpoint, fetch, or render?

**Narrowing path:** Store list at `backend/SupportDesk.Api/Infrastructure/InMemoryCannedReplyStore.cs:8-34`; endpoint at `backend/SupportDesk.Api/Endpoints/CannedReplyEndpoints.cs:8-12`; client load at `frontend/src/App.tsx:54-56`; select render at `frontend/src/App.tsx:307-313`.

**Regression test to add:** API test for canned reply list and UI test for select options.

**Senior lesson:** Debug from boundary to boundary, not by guessing.
