# 05 Security Checklist

## Pre-Merge Security Review

| Check | Current evidence | Status |
| --- | --- | --- |
| Authentication is verified server-side | Header auth at `backend/SupportDesk.Api/Auth/HeaderUserContext.cs:12-20`; client sends headers at `frontend/src/api.ts:19-24` | Demo only; production risk |
| Resource authorization on reads | `backend/SupportDesk.Api/Application/Queries/GetTicket.cs:16-24`, `backend/SupportDesk.Api/Application/Queries/SearchTickets.cs:19-21` | Present |
| Resource authorization on writes | `backend/SupportDesk.Api/Application/Commands/AddComment.cs:31-35`, `AssignTicket.cs:27-31`, `ChangeTicketStatus.cs:28-32`, `ReplaceTicketTags.cs:27-30` | Present |
| IDOR prevention | Route ids at `backend/SupportDesk.Api/Endpoints/TicketEndpoints.cs:31-47` rely on handler permissions | Present but needs tests |
| Internal note leakage | Filter at `backend/SupportDesk.Api/Application/Dto.cs:92-120` | Present but needs tests |
| Input validation | Domain checks at `backend/SupportDesk.Api/Domain/Ticket.cs:83-96`, `backend/SupportDesk.Api/Domain/Ticket.cs:190-193` | Partial |
| XSS | React escapes text rendered at `frontend/src/App.tsx:290-298` | Likely safe unless raw HTML added |
| CSRF | Header/fetch API, no cookies today | Reassess with cookie auth |
| CORS | Allows `http://localhost:5173` with credentials at `backend/SupportDesk.Api/Program.cs:16-20` | Dev only |
| SQL injection | No SQL/EF in repo | Not applicable yet |
| SSRF | No outbound HTTP | Not applicable yet |
| Secrets | No secrets config found; launch settings only env at `backend/SupportDesk.Api/Properties/launchSettings.json:9-10` | Low current risk |
| Rate limiting | No rate limiting in `backend/SupportDesk.Api/Program.cs:51-59` | Missing |
| File uploads | None found | Not applicable |
| Dependency risk | Dependencies in `frontend/package.json:11-18`; no lockfile found | Needs lockfile/audit policy |

## Senior Security Questions

1. Can an unrelated agent fetch a ticket by id? Test `backend/SupportDesk.Api/Application/Queries/GetTicket.cs:16-24`.
2. Can a customer infer internal notes through search? Review `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:61-67` and `backend/SupportDesk.Api/Application/Queries/SearchTickets.cs:19-21`.
3. Can a client join all SignalR updates? Current hub group is `backend/SupportDesk.Api/Realtime/TicketHub.cs:7`.
4. Does CORS with credentials stay dev-only? Current origin is `backend/SupportDesk.Api/Program.cs:16-20`.
