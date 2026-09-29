# 04 Side Effects Async And Reliability

## Current Side Effects

| Side effect | Evidence | Reliability note |
| --- | --- | --- |
| SignalR ticket created | `backend/SupportDesk.Api/Application/Commands/CreateTicket.cs:46-48`; `backend/SupportDesk.Api/Realtime/SignalRTicketNotifier.cs:15-16` | Fires after in-memory save. No durable outbox or retry. |
| SignalR ticket updated | `backend/SupportDesk.Api/Application/Commands/AssignTicket.cs:37-39`, `backend/SupportDesk.Api/Application/Commands/ChangeTicketStatus.cs:38-40`, `backend/SupportDesk.Api/Application/Commands/ReplaceTicketTags.cs:36-38` | Same payload shape as list summary. |
| SignalR ticket commented | `backend/SupportDesk.Api/Application/Commands/AddComment.cs:41-45`; `backend/SupportDesk.Api/Realtime/SignalRTicketNotifier.cs:21-22` | Sends comment payload too; check internal note leakage. |
| Seed data | `backend/SupportDesk.Api/Program.cs:61`; `backend/SupportDesk.Api/Infrastructure/SeedData.cs:8-57` | Happens on startup; in-memory only. |

## Async Boundaries

Handlers are async at `backend/SupportDesk.Api/Application/Commands/CreateTicket.cs:27-49` and `backend/SupportDesk.Api/Application/Commands/AddComment.cs:23-45`. Store methods honor cancellation at `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:15-30`, and SignalR sends accept cancellation at `backend/SupportDesk.Api/Realtime/SignalRTicketNotifier.cs:15-22`.

## Missing Background/Queue Infrastructure

No `IHostedService`, `BackgroundService`, queue, webhook, retry policy, `IHttpClientFactory`, or Polly registration was found in `backend/SupportDesk.Api/Program.cs:33-46` or repository inventory.

## Reliability Concepts

| Concept | How to teach it here |
| --- | --- |
| Idempotency | Re-sending `AddCommentRequest` at `backend/SupportDesk.Api/Application/Commands/AddComment.cs:5-8` creates duplicate comments; future API could add idempotency keys. |
| Outbox | SignalR sends at `backend/SupportDesk.Api/Realtime/SignalRTicketNotifier.cs:15-22` are not durable; a DB-backed system can write events transactionally then publish. |
| Timeout | Fetch wrapper at `frontend/src/api.ts:26-42` has no timeout/abort; future client can use `AbortController`. |
| Retry | SignalR reconnects at `frontend/src/api.ts:124-129`; HTTP mutations should not blindly retry without idempotency. |
| Failure visibility | Errors show in UI at `frontend/src/App.tsx:268-270`; server lacks structured logs around handler failures. |

## Drill

Design an SLA reminder worker using current SLA properties at `backend/SupportDesk.Api/Domain/Ticket.cs:62-70`. Include: schedule, query, idempotency key, retry, alert, and test strategy.
