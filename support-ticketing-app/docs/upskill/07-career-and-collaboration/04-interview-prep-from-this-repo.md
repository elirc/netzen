# 04 Interview Prep From This Repo

Use this repo to practice mid-level interviews. For each prompt, answer with concrete anchors, then generalize.

## C#/.NET Runtime Questions

### Q1: Why does this code use async if the store is in-memory?

Anchor: handler awaits store/notifier at `backend/SupportDesk.Api/Application/Commands/CreateTicket.cs:46-48`; store returns `Task.FromResult` at `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:15-25`.

Junior answer: The methods are async because endpoints await them.

Mid-level adds: The async shape keeps the application layer ready for real I/O like EF or SignalR without changing endpoint contracts.

Senior includes: Async does not make CPU work free. The in-memory store still does work synchronously; when replaced with DB calls, cancellation and transaction boundaries matter.

### Q2: Explain DI lifetimes here.

Anchor: registrations at `backend/SupportDesk.Api/Program.cs:33-46`.

Junior answer: Services are registered so handlers can use them.

Mid-level adds: Stores are singleton; handlers/user context are scoped per request. This avoids storing request identity in a singleton.

Senior includes: Singleton mutable state requires locking and snapshots, visible at `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:7-49`.

### Q3: How is optimistic concurrency implemented?

Anchor: version property at `backend/SupportDesk.Api/Domain/Ticket.cs:66`, version check at `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:39-42`, UI sends version at `frontend/src/App.tsx:116`.

Junior answer: It checks the ticket version.

Mid-level adds: Mutations include `ExpectedVersion`; stale writes return 409 through `backend/SupportDesk.Api/Auth/ApiExceptionHandler.cs:21-23`.

Senior includes: This prevents lost updates but needs conflict UX, tests, and a DB-backed row version if persistence changes.

## ASP.NET / API Questions

### Q4: Trace a ticket create request.

Anchor chain: `frontend/src/App.tsx:163-185`, `frontend/src/api.ts:59-72`, `backend/SupportDesk.Api/Endpoints/TicketEndpoints.cs:34-35`, `backend/SupportDesk.Api/Application/Commands/CreateTicket.cs:27-49`, `backend/SupportDesk.Api/Domain/Ticket.cs:72-134`.

Junior answer: The form posts to the API and creates a ticket.

Mid-level adds: Endpoint binds a record, handler checks permission, domain validates and computes SLA, store saves, notifier broadcasts.

Senior includes: The flow lacks real auth, durability, tests, and time abstraction.

### Q5: Where should validation live?

Anchors: browser required fields at `frontend/src/App.tsx:372-387`; domain validation at `backend/SupportDesk.Api/Domain/Ticket.cs:83-96`.

Junior answer: In the form.

Mid-level adds: Browser validation helps UX, but domain validation protects every caller.

Senior includes: Add structured validation errors for API consumers without moving invariants out of the domain.

## React Questions

### Q6: Explain the render/effect model in this app.

Anchors: state at `frontend/src/App.tsx:35-48`; effects at `frontend/src/App.tsx:50-75`.

Junior answer: State changes update the UI.

Mid-level adds: Filter changes fetch tickets, mount loads canned replies, SignalR effect subscribes and cleans up.

Senior includes: SignalR effect depends on `selectedId` at `frontend/src/App.tsx:69`, causing reconnection on selection changes; that is a design tradeoff to revisit.

### Q7: How does the frontend handle API errors?

Anchors: `frontend/src/api.ts:36-39`, `frontend/src/App.tsx:268-270`.

Junior answer: It shows an error.

Mid-level adds: The API wrapper parses problem details and throws an Error that the UI stores.

Senior includes: Good start, but conflict and validation errors need typed handling instead of generic message display.

## Debugging Questions

### Q8: A customer reports seeing an internal note. How do you debug?

Anchors: comment visibility enum at `backend/SupportDesk.Api/Domain/TicketEnums.cs:21-25`, mapping filter at `backend/SupportDesk.Api/Application/Dto.cs:92-120`, user context at `backend/SupportDesk.Api/Auth/HeaderUserContext.cs:12-20`.

Junior answer: Check the UI.

Mid-level adds: Verify whether the server sent the internal note before looking at rendering.

Senior includes: Treat it as a data leakage incident; add regression tests for staff/customer visibility and audit SignalR comment payload at `backend/SupportDesk.Api/Realtime/SignalRTicketNotifier.cs:21-22`.

### Q9: Search is slow. What do you inspect?

Anchor: full scan at `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:55-107`.

Junior answer: Check the search function.

Mid-level adds: Measure query latency, result count, and filters.

Senior includes: With EF, push filters and auth into SQL, add indexes, avoid paging-after-auth semantics from `backend/SupportDesk.Api/Application/Queries/SearchTickets.cs:17-28`.

## System Design Questions

### Q10: How would you make this production-ready?

Anchors: in-memory store at `backend/SupportDesk.Api/Program.cs:35`, demo auth at `backend/SupportDesk.Api/Auth/HeaderUserContext.cs:12-20`, direct notifier at `backend/SupportDesk.Api/Realtime/SignalRTicketNotifier.cs:15-22`.

Junior answer: Add a database and login.

Mid-level adds: Add EF, real auth, tests, CI, logging, and deployment config.

Senior includes: Plan migrations, rollback, outbox, permission tests, observability, performance budgets, and incremental rollout.

## Code Review Questions

### Q11: A PR adds a new mutation endpoint. What do you check?

Anchors: endpoint pattern at `backend/SupportDesk.Api/Endpoints/TicketEndpoints.cs:40-47`, concurrency at `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:39-42`, permissions at `backend/SupportDesk.Api/Application/Permissions.cs:24-34`.

Junior answer: Check if it compiles.

Mid-level adds: Check permission, expectedVersion, domain invariant, tests, and DTO contract.

Senior includes: Check whether the new mutation commits the product to a workflow state, audit/replay/eventing needs, and rollback.

## Behavioral Prompts

### Q12: Tell me about a time you improved reliability.

Use this repo answer: "I noticed direct SignalR sends at `backend/SupportDesk.Api/Realtime/SignalRTicketNotifier.cs:15-22` after store updates such as `backend/SupportDesk.Api/Application/Commands/AddComment.cs:41-45`. For small apps this is fine, but production needs an outbox. I proposed an incremental migration with tests for event persistence and retry."

Junior version: "I added SignalR."

Mid-level version: "I added tests and retries."

Senior version: "I separated commit from publish, made events idempotent, added observability, and designed rollback."
