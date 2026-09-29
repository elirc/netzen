# 03 Senior Build Projects

## Project 1: EF Core Persistence

**Problem:** Tickets disappear on restart because store is in-memory at `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:7-120`.

**Product value:** Durable customer support history.

**Design checklist:** schema, aggregate mapping, row version, indexes for filters, migrations, seed strategy, rollback to in-memory via config.

**Files/modules:** `backend/SupportDesk.Api/Application/ITicketStore.cs:5-12`, `backend/SupportDesk.Api/Program.cs:35`, `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:7-120`.

**Tests:** domain unchanged, API integration, migration smoke test, concurrency conflict.

**Security/performance:** push permission filters into database query; index status/team/assignee/tags/updated.

**Open questions:** owned entity vs normalized comments/tags, search technology, multi-tenant model.

## Project 2: Real Authentication And Authorization

**Problem:** Current identity trusts headers at `backend/SupportDesk.Api/Auth/HeaderUserContext.cs:12-20`.

**Product value:** Prevent impersonation and IDOR.

**Design checklist:** auth provider, claims model, team membership, customer identity, policy tests, local dev mode.

**Files/modules:** keep `IUserContext` at `backend/SupportDesk.Api/Application/IUserContext.cs:5-11`, replace `HeaderUserContext`, remove client demo headers at `frontend/src/api.ts:19-24`.

**Tests:** route-level 401/403, permission matrix at `backend/SupportDesk.Api/Application/Permissions.cs:7-39`.

**Rollout:** support demo mode separately from production mode.

## Project 3: Durable Realtime Outbox

**Problem:** SignalR sends are direct and non-durable at `backend/SupportDesk.Api/Realtime/SignalRTicketNotifier.cs:15-22`.

**Product value:** Reliable live inbox under disconnects and transient failures.

**Design checklist:** event table, publisher worker, idempotent sends, retry/backoff, observability, replay.

**Files/modules:** command handlers such as `backend/SupportDesk.Api/Application/Commands/AddComment.cs:41-45`, hub at `backend/SupportDesk.Api/Realtime/TicketHub.cs:5-8`.

**Tests:** event persisted once, publisher retries, no internal note leakage.

## Project 4: Full Test Harness

**Problem:** No tests exist for domain, handlers, API, or UI.

**Product value:** Safe feature growth.

**Design checklist:** test projects, builders, fake clock, fake notifier, WebApplicationFactory, frontend test runner.

**Files/modules:** `backend/SupportDesk.Api/Domain/Ticket.cs:72-239`, `backend/SupportDesk.Api/Application/Permissions.cs:7-39`, `frontend/src/App.tsx:35-426`.

**Rollout:** start with domain and permission tests, then API, then UI.

## Project 5: Search And SLA Scaling

**Problem:** Search is an in-memory full scan at `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:55-107`.

**Product value:** Fast inbox for large support queues.

**Design checklist:** DB-backed filters, SLA indexes, cursor pagination, query authorization, measurements.

**Files/modules:** `TicketSearchCriteria` at `backend/SupportDesk.Api/Application/TicketSearchCriteria.cs:5-12`, query handler at `backend/SupportDesk.Api/Application/Queries/SearchTickets.cs:14-28`.

**Tests:** result ordering, auth-filtered totals, breached SLA query.

## Project 6: Frontend Modularization

**Problem:** `frontend/src/App.tsx:35-426` owns all state, data loading, mutations, and rendering.

**Product value:** Easier UI changes and tests.

**Design checklist:** hooks, presentational components, error boundary, loading states, contract tests.

**Files/modules:** `frontend/src/api.ts:26-137`, `frontend/src/types.ts:1-63`, `frontend/src/App.tsx:35-426`.

**Risk:** Splitting too early can scatter logic; preserve behavior first.
