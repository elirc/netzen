# 01 System Map

## Project Graph

| Node | Owns | Evidence | Boundary rule |
| --- | --- | --- | --- |
| `backend/SupportDesk.Api` | ASP.NET Core host, API endpoints, app handlers, domain, in-memory infrastructure, SignalR | SDK and target framework at `backend/SupportDesk.Api/SupportDesk.Api.csproj:1-8`; host at `backend/SupportDesk.Api/Program.cs:10-63` | Keep HTTP concerns in endpoints/host, workflow in handlers, invariants in domain. |
| `frontend` | React inbox UI and API client | scripts/deps at `frontend/package.json:6-18`; app state at `frontend/src/App.tsx:35-48` | Keep browser state and presentation here; do not duplicate backend authorization. |
| `docs/upskill` | Learning lab only | This file | No product code changes. |

## Runtime Ownership

| Runtime | Entry | Major dependencies |
| --- | --- | --- |
| Server | `backend/SupportDesk.Api/Program.cs:10-63` | DI registrations at `backend/SupportDesk.Api/Program.cs:33-46`, routes at `backend/SupportDesk.Api/Program.cs:56-59`, seed load at `backend/SupportDesk.Api/Program.cs:61`. |
| Browser | `frontend/src/main.tsx:1-9` | React app in `frontend/src/App.tsx:35-426`, API wrapper in `frontend/src/api.ts:26-137`, types in `frontend/src/types.ts:1-63`. |
| Live channel | `backend/SupportDesk.Api/Realtime/TicketHub.cs:5-8` | Server sends events at `backend/SupportDesk.Api/Realtime/SignalRTicketNotifier.cs:15-22`; client subscribes at `frontend/src/api.ts:124-137`. |

## Public Interfaces

| Interface | Evidence | Notes |
| --- | --- | --- |
| Ticket search | `backend/SupportDesk.Api/Endpoints/TicketEndpoints.cs:14-29` | Query params become `TicketSearchCriteria` at `backend/SupportDesk.Api/Endpoints/TicketEndpoints.cs:17-28`. |
| Ticket detail | `backend/SupportDesk.Api/Endpoints/TicketEndpoints.cs:31-32` | Handler checks read authorization at `backend/SupportDesk.Api/Application/Queries/GetTicket.cs:14-24`. |
| Ticket create | `backend/SupportDesk.Api/Endpoints/TicketEndpoints.cs:34-35` | Request shape is `backend/SupportDesk.Api/Application/Commands/CreateTicket.cs:5-12`. |
| Comment create | `backend/SupportDesk.Api/Endpoints/TicketEndpoints.cs:37-38` | Payload includes `ExpectedVersion` at `backend/SupportDesk.Api/Application/Commands/AddComment.cs:5-8`. |
| Assignment/status/tags | `backend/SupportDesk.Api/Endpoints/TicketEndpoints.cs:40-47` | Each mutation routes to a separate command handler. |
| Canned replies | `backend/SupportDesk.Api/Endpoints/CannedReplyEndpoints.cs:8-12` | Store is static in-memory data at `backend/SupportDesk.Api/Infrastructure/InMemoryCannedReplyStore.cs:8-34`. |

## Data Model

The main aggregate is `Ticket`, which owns status, priority, assignment, SLA timestamps, tags, comments, and version at `backend/SupportDesk.Api/Domain/Ticket.cs:50-70`. Comments are immutable objects defined at `backend/SupportDesk.Api/Domain/TicketComment.cs:3-26`. Shared enum vocabulary is centralized at `backend/SupportDesk.Api/Domain/TicketEnums.cs:3-33`.

## Missing Subsystems To Notice

| Requested area | Current repo state | How to treat it |
| --- | --- | --- |
| EF Core/persistence | No EF package in `backend/SupportDesk.Api/SupportDesk.Api.csproj:1-8`; active persistence registration uses `InMemoryTicketStore` at `backend/SupportDesk.Api/Program.cs:35`. | Learn the repository pattern now; later design an EF-backed store. |
| Background service | No hosted service registration in `backend/SupportDesk.Api/Program.cs:33-46`. | Use SLA recalculation or notification reminders as a future worker kata. |
| Tests | No test project found in file inventory. | Add unit and API tests as first quality work. |
| CI | No `.github/workflows` found in file inventory. | Add build/test workflow after tests exist. |
