# 06 Architecture Critique

## Strong Choices

| Choice | Evidence | Why it is strong |
| --- | --- | --- |
| Thin endpoints | `backend/SupportDesk.Api/Endpoints/TicketEndpoints.cs:14-47` | Keeps HTTP binding separate from workflow. |
| Domain aggregate | `backend/SupportDesk.Api/Domain/Ticket.cs:72-239` | Centralizes invariants and transitions. |
| Handler-per-command | `backend/SupportDesk.Api/Application/Commands/AddComment.cs:10-47` | Makes mutations easy to review and test. |
| Central permissions | `backend/SupportDesk.Api/Application/Permissions.cs:5-39` | Reduces duplicated authorization logic. |
| Explicit DTOs | `backend/SupportDesk.Api/Application/Dto.cs:5-65` | Separates domain from public contract. |
| Optimistic concurrency | `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:39-42` | Teaches lost update prevention. |

## Risks And Tradeoffs

| Priority | Risk | Evidence | Impact | Confidence |
| --- | --- | --- | --- | --- |
| High | Demo header auth is not production auth | Server trusts headers at `backend/SupportDesk.Api/Auth/HeaderUserContext.cs:12-20`; client sends them at `frontend/src/api.ts:19-24` | Anyone can impersonate roles if deployed as-is. | Confirmed |
| High | No automated tests | No test project in inventory; mutation rules at `backend/SupportDesk.Api/Domain/Ticket.cs:171-217` are untested | Regressions likely as features grow. | Confirmed |
| High | In-memory persistence loses data | Store registered singleton at `backend/SupportDesk.Api/Program.cs:35`; implementation uses dictionary at `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:7-20` | Restart loses tickets. | Confirmed |
| Medium | Realtime events are not durable | Sends directly at `backend/SupportDesk.Api/Realtime/SignalRTicketNotifier.cs:15-22` | Clients miss events during disconnect. | Confirmed |
| Medium | Search is full scan | `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:55-107` | Latency grows with tickets. | Confirmed |
| Medium | Search pages after auth in handler | `backend/SupportDesk.Api/Application/Queries/SearchTickets.cs:17-28` | Paging can be inconsistent once permissions matter. | Confirmed |
| Medium | Large React component | `frontend/src/App.tsx:35-426` | Harder to test and reason as UI grows. | Confirmed |
| Low | Time is not abstracted | `DateTimeOffset.UtcNow` in handlers at `backend/SupportDesk.Api/Application/Commands/CreateTicket.cs:34` and `backend/SupportDesk.Api/Application/Commands/AddComment.cs:37` | Time-sensitive tests are harder. | Confirmed |

## If I Owned This For 3 Months

1. Add tests first.
   - Plan: domain unit tests for `Ticket` at `backend/SupportDesk.Api/Domain/Ticket.cs:72-239`, handler tests with fake `ITicketStore` from `backend/SupportDesk.Api/Application/ITicketStore.cs:5-12`, API tests for endpoints at `backend/SupportDesk.Api/Endpoints/TicketEndpoints.cs:10-49`.
   - Migration: none.
   - Risk reduction: prevents accidental workflow regressions.

2. Replace demo auth with real authentication.
   - Plan: introduce auth middleware/policies, remove trusted headers from `frontend/src/api.ts:19-24`, keep `IUserContext` abstraction at `backend/SupportDesk.Api/Application/IUserContext.cs:5-11`.
   - Migration: keep local dev auth profile only.
   - Tests: IDOR/read/write permission matrix.

3. Add EF Core persistence.
   - Plan: implement `ITicketStore` with `DbContext`, map version token, push search and permission filters into query.
   - Anchors: current store contract `backend/SupportDesk.Api/Application/ITicketStore.cs:5-12`; current search filters `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:58-104`.
   - Rollback: keep in-memory store selectable via configuration during migration.

4. Add durable event/outbox for notifications.
   - Plan: command writes ticket and event; background publisher sends SignalR.
   - Anchors: current direct send at `backend/SupportDesk.Api/Application/Commands/AddComment.cs:41-45`.
   - Tests: event written once, publisher retries safely.

5. Split frontend into hooks/components.
   - Plan: extract `useTickets`, `useTicketDetail`, `TicketList`, `TicketDetail`, `PropertiesPanel`.
   - Anchor: current state/mutations in `frontend/src/App.tsx:35-185` and rendering in `frontend/src/App.tsx:187-426`.
   - Tests: component and hook tests once tooling exists.

## Confirmed Vs Hypothesis

- Confirmed: No EF package in `backend/SupportDesk.Api/SupportDesk.Api.csproj:1-8`.
- Confirmed: In-memory store registered in DI at `backend/SupportDesk.Api/Program.cs:35`.
- Confirmed: Trusted headers create user context at `backend/SupportDesk.Api/Auth/HeaderUserContext.cs:12-20`.
- Investigate: Actual compile behavior after installing .NET SDK; `dotnet` was unavailable during verification.
- Investigate: Browser runtime behavior after installing npm dependencies; dependencies were not installed.
