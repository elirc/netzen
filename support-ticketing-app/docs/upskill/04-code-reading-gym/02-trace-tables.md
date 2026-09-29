# 02 Trace Tables

## Trace 1: UI To API Search

| Step | File | Value shape | Owner | Transformation | Risk |
| --- | --- | --- | --- | --- | --- |
| 1 | `frontend/src/App.tsx:200-235` | form controls | UI | user input updates `filters` | no debounce |
| 2 | `frontend/src/App.tsx:50-52` | `TicketFilters` | React | effect calls refresh | repeated fetches |
| 3 | `frontend/src/api.ts:44-53` | URLSearchParams | API client | serializes filters | omitted empty values |
| 4 | `backend/SupportDesk.Api/Endpoints/TicketEndpoints.cs:14-29` | `TicketSearchCriteria` | endpoint | binds query params | enum parse failure |
| 5 | `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:58-104` | tickets | store | filters in memory | full scan |
| 6 | `backend/SupportDesk.Api/Application/Queries/SearchTickets.cs:19-28` | summaries | handler | auth filter + page | paging/auth semantics |

## Trace 2: Create Ticket

| Step | File | Value shape | Owner | Transformation | Risk |
| --- | --- | --- | --- | --- | --- |
| 1 | `frontend/src/App.tsx:163-185` | FormData | UI | builds create input | weak validation |
| 2 | `frontend/src/api.ts:59-72` | JSON | API client | POST body | trusted headers |
| 3 | `backend/SupportDesk.Api/Application/Commands/CreateTicket.cs:27-44` | request record | handler | permission + domain factory | time hardcoded |
| 4 | `backend/SupportDesk.Api/Domain/Ticket.cs:83-133` | aggregate | domain | validate + SLA | invariant bug risk |
| 5 | `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:15-20` | snapshot | store | save | non-durable |
| 6 | `backend/SupportDesk.Api/Realtime/SignalRTicketNotifier.cs:15-16` | summary | SignalR | broadcast | missed event |

## Trace 3: Add Internal Note

| Step | File | Value shape | Owner | Transformation | Risk |
| --- | --- | --- | --- | --- | --- |
| 1 | `frontend/src/App.tsx:314-320` | checkbox | UI | picks Internal | wrong visibility |
| 2 | `frontend/src/App.tsx:111-123` | body/version | UI | submit comment | stale version |
| 3 | `backend/SupportDesk.Api/Application/Commands/AddComment.cs:27-39` | working copy | handler/store | permission + mutate | forbidden path |
| 4 | `backend/SupportDesk.Api/Domain/Ticket.cs:183-217` | comment | domain | append + touch | first response logic |
| 5 | `backend/SupportDesk.Api/Application/Dto.cs:92-120` | detail DTO | mapper | hides internal notes if needed | leakage |

## Trace 4: Permission Denial

| Step | File | Value shape | Owner | Transformation | Risk |
| --- | --- | --- | --- | --- | --- |
| 1 | `backend/SupportDesk.Api/Auth/HeaderUserContext.cs:12-20` | headers | auth stub | role/team/user context | spoofable |
| 2 | `backend/SupportDesk.Api/Application/Queries/GetTicket.cs:16-24` | ticket | query handler | permission check | IDOR if skipped |
| 3 | `backend/SupportDesk.Api/Application/Permissions.cs:7-11` | user + ticket | permissions | boolean | rule drift |
| 4 | `backend/SupportDesk.Api/Auth/ApiExceptionHandler.cs:21-23` | exception | middleware | 403 problem details | weak logging |
| 5 | `frontend/src/api.ts:36-39` | HTTP response | client | JS Error | user sees plain detail |

## Trace 5: Realtime Comment Event

| Step | File | Value shape | Owner | Transformation | Risk |
| --- | --- | --- | --- | --- | --- |
| 1 | `backend/SupportDesk.Api/Application/Commands/AddComment.cs:41-45` | ticket + comment | handler | notifier call | no outbox |
| 2 | `backend/SupportDesk.Api/Realtime/SignalRTicketNotifier.cs:21-22` | anonymous payload | SignalR | event send | payload contract implicit |
| 3 | `frontend/src/api.ts:131-133` | payload.ticket | client | callback | ignores comment body |
| 4 | `frontend/src/App.tsx:58-64` | summary | UI | upsert and maybe reload detail | reconnect effect |

## Trace 6: Canned Reply Selection

| Step | File | Value shape | Owner | Transformation | Risk |
| --- | --- | --- | --- | --- | --- |
| 1 | `backend/SupportDesk.Api/Infrastructure/InMemoryCannedReplyStore.cs:8-34` | list | store | returns static replies | no editing |
| 2 | `backend/SupportDesk.Api/Endpoints/CannedReplyEndpoints.cs:8-12` | DTO list | endpoint | returns OK | no auth |
| 3 | `frontend/src/App.tsx:54-56` | list | UI | loads on mount | no loading UI |
| 4 | `frontend/src/App.tsx:307-313` | reply body | UI | sets composer text | overwrites draft |
