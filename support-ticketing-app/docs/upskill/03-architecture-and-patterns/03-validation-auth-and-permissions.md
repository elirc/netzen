# 03 Validation Auth And Permissions

## Validation Layers

| Layer | Evidence | Current responsibility |
| --- | --- | --- |
| Browser | Required fields in new ticket form at `frontend/src/App.tsx:372-387` | Basic UX validation only. |
| Endpoint/model binding | Request records at `backend/SupportDesk.Api/Application/Commands/CreateTicket.cs:5-12`, `backend/SupportDesk.Api/Application/Commands/AddComment.cs:5-8` | JSON shape binding and enum parsing. |
| Domain | Required create fields at `backend/SupportDesk.Api/Domain/Ticket.cs:83-96`; comment body check at `backend/SupportDesk.Api/Domain/Ticket.cs:190-193` | Invariants that must hold regardless of caller. |
| Error mapping | `backend/SupportDesk.Api/Auth/ApiExceptionHandler.cs:19-41` | Converts domain and app exceptions to problem details. |

## Authentication Boundary

This repo uses demo header identity. The server reads headers and fallback defaults at `backend/SupportDesk.Api/Auth/HeaderUserContext.cs:8-32`. The client sends matching demo headers at `frontend/src/api.ts:19-24`.

Production warning: this is not authentication. A real system would validate JWTs, cookies, sessions, or an identity provider before creating `IUserContext`.

## Authorization Rules

| Operation | Rule | Evidence |
| --- | --- | --- |
| Read ticket | Admin/manager, assignee, team member, or customer email | `backend/SupportDesk.Api/Application/Permissions.cs:7-11` |
| Create ticket | Any defined role | `backend/SupportDesk.Api/Application/Permissions.cs:13-14` |
| Comment | Internal requires staff and read access; public allows staff with read access or customer | `backend/SupportDesk.Api/Application/Permissions.cs:16-22` |
| Assign | Admin/manager or readable ticket for agent | `backend/SupportDesk.Api/Application/Permissions.cs:24-26` |
| Change status | Admin/manager or readable ticket for agent | `backend/SupportDesk.Api/Application/Permissions.cs:28-30` |
| Tag | Admin/manager or readable ticket for agent | `backend/SupportDesk.Api/Application/Permissions.cs:32-34` |
| Read internal notes | Admin/manager or readable ticket for agent | `backend/SupportDesk.Api/Application/Permissions.cs:36-38` |

## Junior Misses Vs Senior Checks

| Junior often misses | Senior checks |
| --- | --- |
| Hiding a UI button is not authorization. | Handler checks exist before every mutation: `backend/SupportDesk.Api/Application/Commands/AddComment.cs:31-35`, `backend/SupportDesk.Api/Application/Commands/AssignTicket.cs:27-31`, `backend/SupportDesk.Api/Application/Commands/ChangeTicketStatus.cs:28-32`. |
| Read authorization matters too. | `GetTicketHandler` rejects unauthorized reads at `backend/SupportDesk.Api/Application/Queries/GetTicket.cs:16-24`. |
| Internal notes need read filtering. | Detail mapping filters comments at `backend/SupportDesk.Api/Application/Dto.cs:92-120`. |
| Demo auth is enough. | Headers are client-controlled at `frontend/src/api.ts:19-24`; production needs verified authn. |
| IDs are harmless. | `Guid id` route values at `backend/SupportDesk.Api/Endpoints/TicketEndpoints.cs:31-47` require resource checks to prevent IDOR. |

## Drill

Design a permission test matrix for `CanReadTicket` at `backend/SupportDesk.Api/Application/Permissions.cs:7-11`: admin, manager, assigned agent, team agent, unrelated agent, customer. Include one test for internal note visibility using `backend/SupportDesk.Api/Application/Dto.cs:92-120`.
