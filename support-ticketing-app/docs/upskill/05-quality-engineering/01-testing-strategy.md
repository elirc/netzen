# 01 Testing Strategy

## Test Layers

| Layer | What belongs here | Repo anchors |
| --- | --- | --- |
| Domain unit tests | SLA calculation, validation, status transitions, tag normalization, version increments | `backend/SupportDesk.Api/Domain/Ticket.cs:72-239` |
| Permission unit tests | Role/resource matrix | `backend/SupportDesk.Api/Application/Permissions.cs:7-39` |
| Handler unit tests | Permission failures, concurrency propagation, notifier call, DTO return | `backend/SupportDesk.Api/Application/Commands/AddComment.cs:23-45` |
| API integration tests | Route binding, status codes, problem details, JSON enum strings, CORS if needed | `backend/SupportDesk.Api/Endpoints/TicketEndpoints.cs:10-49`, `backend/SupportDesk.Api/Auth/ApiExceptionHandler.cs:17-42` |
| UI tests | Filter behavior, composer behavior, conflict error display | `frontend/src/App.tsx:200-324` |
| E2E tests | Create ticket, add comment, live update | Flow anchors in `01-codebase-cartography/05-key-flows.md` |

## What Not To Test

- Do not test that C# auto-properties work at `backend/SupportDesk.Api/Domain/Ticket.cs:50-68`.
- Do not snapshot the entire React tree from `frontend/src/App.tsx:187-426`; test behavior.
- Do not mock the domain aggregate when testing handlers; the domain is the rule owner.

## First Test Matrix

| Behavior | Expected | Anchor |
| --- | --- | --- |
| Empty subject rejected | 400 through domain exception | `backend/SupportDesk.Api/Domain/Ticket.cs:83-86`, `backend/SupportDesk.Api/Auth/ApiExceptionHandler.cs:23-25` |
| Urgent SLA | first response 15 min, resolution 4 hours | `backend/SupportDesk.Api/Domain/Ticket.cs:98-109` |
| Closed ticket cannot reopen | domain exception | `backend/SupportDesk.Api/Domain/Ticket.cs:171-180` |
| Stale version rejected | 409 | `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:39-42`, `backend/SupportDesk.Api/Auth/ApiExceptionHandler.cs:21-23` |
| Customer cannot see internal note | internal comment absent from detail | `backend/SupportDesk.Api/Application/Dto.cs:92-120` |
| Unauthorized ticket read rejected | 403 | `backend/SupportDesk.Api/Application/Queries/GetTicket.cs:16-24` |

## Flake Prevention

- Abstract time before deep SLA tests; current code uses `DateTimeOffset.UtcNow` in handlers at `backend/SupportDesk.Api/Application/Commands/CreateTicket.cs:34`.
- Use deterministic ticket numbers by controlling store sequence at `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:11-13`.
- Avoid tests depending on seed data timestamps from `backend/SupportDesk.Api/Infrastructure/SeedData.cs:18-57`.
