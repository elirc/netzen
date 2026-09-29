# 06 Observability And Operations

## Current Observability

| Capability | Evidence | Status |
| --- | --- | --- |
| Exception-to-problem-details | `backend/SupportDesk.Api/Auth/ApiExceptionHandler.cs:17-42` | Present |
| Logging configuration | `backend/SupportDesk.Api/appsettings.Development.json:1-9` | Basic levels only |
| Structured logs in handlers | No `ILogger` injections in command/query handlers such as `backend/SupportDesk.Api/Application/Commands/AddComment.cs:10-47` | Missing |
| Health checks | No health check registration in `backend/SupportDesk.Api/Program.cs:33-59` | Missing |
| Metrics/tracing | No OpenTelemetry/metrics registration in `backend/SupportDesk.Api/Program.cs:33-59` | Missing |
| Deployment config | No Docker/infra files in inventory | Missing |

## How Would I Know This Broke?

| Flow | Signal to add | Anchor |
| --- | --- | --- |
| Search | endpoint latency, result count, 403 count | `backend/SupportDesk.Api/Application/Queries/SearchTickets.cs:14-28` |
| Create ticket | create count, validation failures, notifier failures | `backend/SupportDesk.Api/Application/Commands/CreateTicket.cs:27-49` |
| Comment | 409 rate, 403 rate, notification failures | `backend/SupportDesk.Api/Application/Commands/AddComment.cs:23-45` |
| SignalR | connected clients, reconnect count, send failures | `backend/SupportDesk.Api/Realtime/SignalRTicketNotifier.cs:15-22` |
| Auth | rejected reads/writes by role | `backend/SupportDesk.Api/Application/Permissions.cs:7-39` |

## Operations Drills

1. Add `ILogger<AddCommentHandler>` design: log ticket id, visibility, result, and failure, but never log full comment body from `backend/SupportDesk.Api/Application/Commands/AddComment.cs:5-8`.
2. Add `/health` design: app is healthy when process is up; later include DB when replacing `InMemoryTicketStore` at `backend/SupportDesk.Api/Program.cs:35`.
3. Add correlation id design: include it in problem details from `backend/SupportDesk.Api/Auth/ApiExceptionHandler.cs:35-40`.
4. Rollback plan for EF migration: keep the `ITicketStore` abstraction at `backend/SupportDesk.Api/Application/ITicketStore.cs:5-12` and configure implementation by environment.
