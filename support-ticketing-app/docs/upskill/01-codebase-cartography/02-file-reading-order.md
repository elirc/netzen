# 02 File Reading Order

## Junior Path

| Order | File | Why | Look for |
| --- | --- | --- | --- |
| 1 | `README.md:1-74` | Product promise and run notes | Feature list, demo headers, API list. |
| 2 | `backend/SupportDesk.Api/Program.cs:10-63` | Server composition | DI lifetimes, route mapping, CORS, SignalR. |
| 3 | `frontend/package.json:6-18` | Client tooling | What `npm run dev` and `npm run build` do. |
| 4 | `frontend/src/types.ts:1-63` | Client contracts | Enum strings, summary vs detail shapes. |
| 5 | `frontend/src/api.ts:17-42` | Network boundary | API base URL, demo headers, error shape. |
| 6 | `frontend/src/App.tsx:35-80` | UI state model | State variables and derived SLA count. |
| 7 | `frontend/src/App.tsx:82-185` | UI behavior | Fetch, open, comment, assign, status, tags, create. |
| 8 | `backend/SupportDesk.Api/Endpoints/TicketEndpoints.cs:10-49` | API surface | Route-to-handler mapping. |
| 9 | `backend/SupportDesk.Api/Application/Commands/CreateTicket.cs:27-49` | First command | Permission, domain call, store, notifier. |
| 10 | `backend/SupportDesk.Api/Domain/Ticket.cs:72-134` | Creation invariant | Required fields, SLA by priority, version 1. |

## Mid-Level Path

| Order | File | Why | Look for |
| --- | --- | --- | --- |
| 11 | `backend/SupportDesk.Api/Application/Permissions.cs:7-39` | Resource authorization | Role, assignee, team, customer boundaries. |
| 12 | `backend/SupportDesk.Api/Auth/HeaderUserContext.cs:8-32` | Auth boundary | Header parsing, defaults, trust assumption. |
| 13 | `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:28-49` | Concurrency | Lock, expected version, mutation boundary. |
| 14 | `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:51-107` | Search/filtering | Filtering order, paging, unbounded scans. |
| 15 | `backend/SupportDesk.Api/Application/Queries/SearchTickets.cs:14-28` | Query authorization | Store search before permission filtering. |
| 16 | `backend/SupportDesk.Api/Application/Dto.cs:67-134` | Mapping | Summary/detail DTOs and internal note filtering. |
| 17 | `backend/SupportDesk.Api/Realtime/SignalRTicketNotifier.cs:15-22` | Side effects | Event names and payload shapes. |
| 18 | `frontend/src/api.ts:124-137` | SignalR client | Reconnect, handlers, group join. |
| 19 | `backend/SupportDesk.Api/Auth/ApiExceptionHandler.cs:17-42` | Error contract | Exception-to-status mapping. |
| 20 | `backend/SupportDesk.Api/Infrastructure/SeedData.cs:18-57` | Demo data | Realistic workflow states. |

## Senior Path

| Order | File | Why | Look for |
| --- | --- | --- | --- |
| 21 | `backend/SupportDesk.Api/Program.cs:33-46` | DI lifetime critique | Singleton store, scoped user context, scoped notifier. |
| 22 | `backend/SupportDesk.Api/Domain/Ticket.cs:183-217` | Side-effect-free domain mutation | First response logic and status transition. |
| 23 | `backend/SupportDesk.Api/Domain/Ticket.cs:219-238` | Normalization and versioning | Tag normalization, `Touch`, unused priority update. |
| 24 | `frontend/src/App.tsx:58-69` | Effect lifecycle | SignalR connection tied to selected ticket. |
| 25 | `frontend/src/App.tsx:111-161` | Optimistic concurrency UX | Stale version failures surface as error text. |
| 26 | `frontend/src/App.tsx:287-363` | UI ownership | Conversation vs properties panel. |
| 27 | `frontend/src/styles.css:1-274` | Responsive layout | Grid layout, sticky composer, mobile behavior. |
| 28 | `backend/SupportDesk.Api/SupportDesk.Api.csproj:1-8` | Package absence | No persistence, validation, auth, or test packages. |
| 29 | `backend/SupportDesk.Api/Properties/launchSettings.json:4-10` | Local runtime | Port and environment. |
| 30 | `frontend/tsconfig.json:1-22` | Type strictness | Strict TypeScript and JSX settings. |
