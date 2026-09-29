# 01 Boundaries And Layers

## Layer Ownership

| Layer | Owns | Must not own | Evidence |
| --- | --- | --- | --- |
| Host | Service registration, middleware, route/hub mapping | Domain rules | `backend/SupportDesk.Api/Program.cs:12-63` |
| Endpoint | HTTP route shape and binding | Business decisions | `backend/SupportDesk.Api/Endpoints/TicketEndpoints.cs:10-49` |
| Application handler | Workflow orchestration, permission check, store call, side-effect trigger | Low-level storage details | `backend/SupportDesk.Api/Application/Commands/AddComment.cs:23-45` |
| Domain | Invariants, state transitions, normalization | HTTP, SignalR, React | `backend/SupportDesk.Api/Domain/Ticket.cs:72-239` |
| Infrastructure | Persistence mechanics and canned reply data | UI state, request headers | `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:7-120` |
| Client API | Network details and TS response types | Server authorization | `frontend/src/api.ts:26-137` |
| UI | Browser state, forms, rendering | Domain invariants | `frontend/src/App.tsx:35-426` |

## Boundary Examples

- Good separation: `CreateTicketHandler` checks permission and calls `Ticket.Create` at `backend/SupportDesk.Api/Application/Commands/CreateTicket.cs:27-44`; the domain owns required-field validation and SLA calculation at `backend/SupportDesk.Api/Domain/Ticket.cs:83-112`.
- Good separation: endpoint routes are thin at `backend/SupportDesk.Api/Endpoints/TicketEndpoints.cs:34-47`; handlers own workflow.
- Possible boundary leak: `HeaderUserContext` trusts client-provided headers at `backend/SupportDesk.Api/Auth/HeaderUserContext.cs:12-20`, while the React client hardcodes those headers at `frontend/src/api.ts:19-24`. This is acceptable for a learning stub, not production auth.
- Possible boundary leak: search permission is applied after in-memory search at `backend/SupportDesk.Api/Application/Queries/SearchTickets.cs:17-28`; in a DB-backed system this should be pushed into the query for correctness and performance.

## Drill

For `AddCommentHandler` at `backend/SupportDesk.Api/Application/Commands/AddComment.cs:23-45`, annotate:

- Inputs
- Dependencies
- Authorization
- Domain mutation
- Store boundary
- Side effect
- Output contract

Self-grade: Basic finds each part. Solid explains why each part belongs in the handler. Strong identifies what changes if notifications become durable.
