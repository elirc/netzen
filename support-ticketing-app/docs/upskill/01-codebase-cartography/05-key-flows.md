# 05 Key Flows

## Flow: Search Inbox

**Why it matters:** Search is the default support workflow. It crosses UI state, query serialization, endpoint binding, store filtering, permission filtering, DTO mapping, and rendering.

**Open first:** `frontend/src/App.tsx:200-235` - filter controls.

| Step | Owner | File | What happens | Data shape | Risk |
| --- | --- | --- | --- | --- | --- |
| 1 | UI | `frontend/src/App.tsx:36-52` | Filter state changes trigger refresh. | `TicketFilters` | Every keystroke fetches; no debounce. |
| 2 | Client API | `frontend/src/api.ts:44-53` | Filters become query params. | URL query | Empty values omitted. |
| 3 | Endpoint | `backend/SupportDesk.Api/Endpoints/TicketEndpoints.cs:14-29` | Model binding builds criteria. | `TicketSearchCriteria` | Enum parse errors become 400 via framework/handler. |
| 4 | Store | `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:51-107` | In-memory filtering and paging. | `PagedResult<Ticket>` | Full scan; not a DB query. |
| 5 | Query handler | `backend/SupportDesk.Api/Application/Queries/SearchTickets.cs:14-28` | Applies read permission and maps summaries. | `PagedResponse<TicketSummaryDto>` | Store returns top 100 before auth; possible paging semantics issue. |
| 6 | UI | `frontend/src/App.tsx:237-265` | Renders ticket rows. | `TicketSummary[]` | Stale selected detail after filtering. |

**Validation & authorization:** Read permission at `backend/SupportDesk.Api/Application/Queries/SearchTickets.cs:19-21` delegates to `backend/SupportDesk.Api/Application/Permissions.cs:7-11`.

**Persistence & side effects:** Search reads snapshots from `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:55-57`; no side effects.

**Tests:** No direct coverage found in repository inventory.

**Juniors miss:** Search authorization is not only a controller concern.

**Seniors notice:** Filtering before auth and paging after auth can behave differently from a database-backed query.

**Drill:** Add a trace row for a `tag=vip&breached=true` query and identify where each filter applies.

**Self-grade:** Basic: identify route and UI. Solid: explain DTO shape. Strong: explain paging/auth risk and a test.

## Flow: Create Ticket

**Why it matters:** This is the cleanest command path: UI to API to handler to aggregate to store to live notification.

**Open first:** `backend/SupportDesk.Api/Application/Commands/CreateTicket.cs:27-49` - command orchestration.

| Step | Owner | File | What happens | Data shape | Risk |
| --- | --- | --- | --- | --- | --- |
| 1 | UI | `frontend/src/App.tsx:163-185` | FormData becomes create input. | strings, priority, tags | Minimal client validation. |
| 2 | Client API | `frontend/src/api.ts:59-72` | POST `/api/tickets`. | JSON body | Client can send trusted headers. |
| 3 | Endpoint | `backend/SupportDesk.Api/Endpoints/TicketEndpoints.cs:34-35` | Request binds to record. | `CreateTicketRequest` | No explicit FluentValidation/data annotations. |
| 4 | Handler | `backend/SupportDesk.Api/Application/Commands/CreateTicket.cs:27-33` | Checks create permission. | `IUserContext` | Current permission allows all roles. |
| 5 | Domain | `backend/SupportDesk.Api/Domain/Ticket.cs:83-133` | Validates subject/description/email and computes SLA. | `Ticket` | Time source is direct `UtcNow`. |
| 6 | Store | `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:15-20` | Saves snapshot. | `Ticket` | No durability. |
| 7 | Realtime | `backend/SupportDesk.Api/Application/Commands/CreateTicket.cs:46-48` | Broadcasts created summary. | `TicketSummaryDto` | No retry/outbox. |

**Validation & authorization:** `Permissions.CanCreateTicket` at `backend/SupportDesk.Api/Application/Permissions.cs:13-14`; domain validation at `backend/SupportDesk.Api/Domain/Ticket.cs:83-96`.

**Persistence & side effects:** Store add at `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:15-20`; SignalR at `backend/SupportDesk.Api/Realtime/SignalRTicketNotifier.cs:15-16`.

**Tests:** No direct coverage found.

**Juniors miss:** Validation is split between client HTML required fields and domain checks.

**Seniors notice:** Creation uses `DateTimeOffset.UtcNow` directly in handler at `backend/SupportDesk.Api/Application/Commands/CreateTicket.cs:34`, making time-sensitive tests harder.

**Drill:** Write three invalid create inputs and name the layer that rejects each.

**Self-grade:** Basic: route to handler. Solid: explain aggregate invariant. Strong: propose time abstraction and tests.

## Flow: Add Public Comment Or Internal Note

**Why it matters:** Comments combine authorization, concurrency, visibility, domain mutation, and live updates.

**Open first:** `backend/SupportDesk.Api/Application/Commands/AddComment.cs:23-45` - mutation orchestration.

| Step | Owner | File | What happens | Data shape | Risk |
| --- | --- | --- | --- | --- | --- |
| 1 | UI | `frontend/src/App.tsx:301-324` | Composer collects body and visibility. | text + Public/Internal | Internal checkbox is easy to misuse. |
| 2 | UI | `frontend/src/App.tsx:111-123` | Sends selected version. | `expectedVersion` | Stale UI produces conflict. |
| 3 | Client API | `frontend/src/api.ts:74-83` | POST comment JSON. | body, visibility, expectedVersion | No optimistic retry UX. |
| 4 | Store | `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:28-49` | Checks version under lock. | mutable working copy | Mutate lambda runs inside lock. |
| 5 | Handler | `backend/SupportDesk.Api/Application/Commands/AddComment.cs:31-38` | Checks permission then calls domain. | `TicketComment?` | Side effect captured outside lambda. |
| 6 | Domain | `backend/SupportDesk.Api/Domain/Ticket.cs:183-217` | Validates body, appends comment, updates first response/status. | `TicketComment` | Customer/agent detection compares author id to email. |
| 7 | Mapper | `backend/SupportDesk.Api/Application/Dto.cs:92-120` | Hides internal comments unless allowed. | detail DTO | Read filtering must stay aligned with write rules. |
| 8 | Realtime | `backend/SupportDesk.Api/Realtime/SignalRTicketNotifier.cs:21-22` | Broadcasts ticket and comment. | anonymous payload | Client expects `payload.ticket`. |

**Validation & authorization:** `Permissions.CanComment` at `backend/SupportDesk.Api/Application/Permissions.cs:16-22`.

**Persistence & side effects:** Mutation through store at `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:44-47`; SignalR at `backend/SupportDesk.Api/Application/Commands/AddComment.cs:41-45`.

**Tests:** No direct coverage found.

**Juniors miss:** Internal note visibility requires read filtering, not just a write checkbox.

**Seniors notice:** A durable system would separate commit from notification with an outbox.

**Drill:** Explain what should happen if two agents comment from version 3 simultaneously.

**Self-grade:** Basic: explain expectedVersion. Solid: trace permission. Strong: design conflict UX and tests.

## Flow: Assignment And Status Change

**Why it matters:** Support tools are workflow machines. Assignment and status changes are small but high-impact state transitions.

**Open first:** `frontend/src/App.tsx:327-363` - property panel.

| Step | Owner | File | What happens | Data shape | Risk |
| --- | --- | --- | --- | --- | --- |
| 1 | UI | `frontend/src/App.tsx:125-147` | Saves assignment or status using current version. | strings/status + version | Same detail version can be reused after another mutation if stale. |
| 2 | Client API | `frontend/src/api.ts:86-107` | PUT assignment/status. | JSON | Assignee/team are free text. |
| 3 | Endpoint | `backend/SupportDesk.Api/Endpoints/TicketEndpoints.cs:40-44` | Routes to command handlers. | request records | No route-level authorization attributes. |
| 4 | Handler | `backend/SupportDesk.Api/Application/Commands/AssignTicket.cs:21-39` | Checks permission and assigns. | `TicketDetailDto` | Agent can assign if they can read. |
| 5 | Domain | `backend/SupportDesk.Api/Domain/Ticket.cs:158-180` | Assignment opens new ticket; status can close. | aggregate state | Closed tickets cannot reopen. |
| 6 | Realtime | `backend/SupportDesk.Api/Application/Commands/ChangeTicketStatus.cs:38-40` | Broadcasts updated summary. | summary DTO | No event version field beyond ticket version. |

**Validation & authorization:** `Permissions.CanAssign` at `backend/SupportDesk.Api/Application/Permissions.cs:24-26`; `Permissions.CanChangeStatus` at `backend/SupportDesk.Api/Application/Permissions.cs:28-30`.

**Persistence & side effects:** Store update at `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:28-49`.

**Tests:** No direct coverage found.

**Juniors miss:** Status rules belong in the domain aggregate, not only UI dropdowns.

**Seniors notice:** Free-text assignee/team has no referential integrity.

**Drill:** Propose an invalid status transition test using `backend/SupportDesk.Api/Domain/Ticket.cs:171-180`.

**Self-grade:** Basic: find handlers. Solid: explain domain transition. Strong: identify identity/team data model gap.

## Flow: Live Inbox Update

**Why it matters:** Real-time systems teach side effects, connection lifecycle, idempotency, and event contract drift.

**Open first:** `frontend/src/api.ts:124-137` - SignalR connection.

| Step | Owner | File | What happens | Data shape | Risk |
| --- | --- | --- | --- | --- | --- |
| 1 | Server host | `backend/SupportDesk.Api/Program.cs:29-32` | SignalR JSON protocol uses string enums. | hub JSON | DTO changes affect clients. |
| 2 | Server route | `backend/SupportDesk.Api/Program.cs:59` | Hub mapped to `/hubs/tickets`. | WebSocket/SSE/long polling | CORS credentials required. |
| 3 | Hub | `backend/SupportDesk.Api/Realtime/TicketHub.cs:5-8` | Client joins `inbox` group. | group membership | No per-team grouping. |
| 4 | Notifier | `backend/SupportDesk.Api/Realtime/SignalRTicketNotifier.cs:15-22` | Sends created/updated/commented events. | summary or payload | No replay for missed events. |
| 5 | Client | `frontend/src/api.ts:131-135` | Registers handlers and starts connection. | callback | Start errors are not surfaced. |
| 6 | UI | `frontend/src/App.tsx:58-69` | Upserts summaries and refreshes selected ticket. | state update | Effect reconnects when selected id changes. |

**Validation & authorization:** Hub group join has no resource authorization at `backend/SupportDesk.Api/Realtime/TicketHub.cs:7`; event visibility relies on summary-only payload except comments event includes comment at `backend/SupportDesk.Api/Realtime/SignalRTicketNotifier.cs:21-22`.

**Persistence & side effects:** Notifications fire after store calls in `backend/SupportDesk.Api/Application/Commands/CreateTicket.cs:46-48` and `backend/SupportDesk.Api/Application/Commands/AddComment.cs:41-45`.

**Tests:** No direct coverage found.

**Juniors miss:** Real-time events are public contracts too.

**Seniors notice:** Grouping all clients into `inbox` is a possible authorization and data minimization risk.

**Drill:** Design team-based SignalR groups using current `TeamId` evidence at `backend/SupportDesk.Api/Domain/Ticket.cs:59`.

**Self-grade:** Basic: name event paths. Solid: explain client upsert. Strong: propose missed-event recovery.

## Flow: Error And Conflict Response

**Why it matters:** Good systems make failures predictable. This repo centralizes exception-to-HTTP mapping.

**Open first:** `backend/SupportDesk.Api/Auth/ApiExceptionHandler.cs:17-42` - error translation.

| Step | Owner | File | What happens | Data shape | Risk |
| --- | --- | --- | --- | --- | --- |
| 1 | Store | `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:34-42` | Missing ticket or stale version throws. | exception | Message exposes current version. |
| 2 | Domain | `backend/SupportDesk.Api/Domain/Ticket.cs:83-96` | Invalid create throws domain exception. | exception | Validation not field-structured. |
| 3 | App | `backend/SupportDesk.Api/Application/Commands/AddComment.cs:31-35` | Forbidden mutation throws. | exception | Handler owns permission checks. |
| 4 | Middleware | `backend/SupportDesk.Api/Program.cs:48-53` | Exception handler is registered and used. | problem details | Ordering matters. |
| 5 | Exception handler | `backend/SupportDesk.Api/Auth/ApiExceptionHandler.cs:19-41` | Maps exceptions to status and problem details. | RFC7807-like JSON | Logs are absent. |
| 6 | Client | `frontend/src/api.ts:36-39` | Throws `Error` with problem detail. | JS Error | UI shows only message at `frontend/src/App.tsx:268-270`. |

**Validation & authorization:** Domain and permission exceptions map at `backend/SupportDesk.Api/Auth/ApiExceptionHandler.cs:21-25`.

**Persistence & side effects:** Conflict originates in store update at `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:39-42`.

**Tests:** No direct coverage found.

**Juniors miss:** A failed mutation should not broadcast a live event; this repo broadcasts only after update returns at `backend/SupportDesk.Api/Application/Commands/AddComment.cs:27-45`.

**Seniors notice:** There is no structured logging or correlation id.

**Drill:** Simulate stale version in two browser tabs and write the expected 409 path.

**Self-grade:** Basic: identify status code. Solid: explain exception mapping. Strong: design retry/refresh UX.
