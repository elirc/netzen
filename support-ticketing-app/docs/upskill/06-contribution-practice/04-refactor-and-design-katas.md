# 04 Refactor And Design Katas

## Kata 1: Boundary Leak Audit

Audit every place UI assumes backend rules. Start at `frontend/src/App.tsx:314-320` and compare to `backend/SupportDesk.Api/Application/Permissions.cs:16-22`.

Strong answer: distinguishes UX affordance from authorization.

## Kata 2: Outbox RFC

Write an RFC replacing direct SignalR sends at `backend/SupportDesk.Api/Realtime/SignalRTicketNotifier.cs:15-22` with durable events.

Strong answer: includes idempotency, retries, observability, and rollback.

## Kata 3: Split Store Interface

Critique whether `ITicketStore` at `backend/SupportDesk.Api/Application/ITicketStore.cs:5-12` should split commands and queries.

Strong answer: weighs simplicity against query optimization and testability.

## Kata 4: Type Safety Upgrade

Design value objects for `TeamId`, `AgentId`, and `CustomerEmail` now represented as strings at `backend/SupportDesk.Api/Domain/Ticket.cs:54-59`.

Strong answer: avoids overengineering while reducing invalid states.

## Kata 5: Design Migration For Tags

Current tags are string lists normalized at `backend/SupportDesk.Api/Domain/Ticket.cs:219-238`.

Strong answer: proposes normalized table, backfill, index, API compatibility, and tests.

## Kata 6: Reduce N+1 Before It Exists

No EF exists, but future detail endpoints at `backend/SupportDesk.Api/Application/Queries/GetTicket.cs:14-24` could load comments poorly.

Strong answer: explains eager loading/projections and how DTO mapping at `backend/SupportDesk.Api/Application/Dto.cs:92-120` can become a query projection.

## Kata 7: Review A Flawed PR

Fake PR: adds `Admin` check in UI only to hide assignment controls.

Real anchors: UI assignment at `frontend/src/App.tsx:334-344`; server assignment permission at `backend/SupportDesk.Api/Application/Commands/AssignTicket.cs:27-33`.

Strong answer: says UI can improve UX but server remains authority.

## Kata 8: Write A Rollback Plan

Scenario: EF migration causes slow searches.

Anchors: in-memory store contract at `backend/SupportDesk.Api/Application/ITicketStore.cs:5-12`, DI selection at `backend/SupportDesk.Api/Program.cs:35`.

Strong answer: feature flag/store selection, migration rollback, data backup, performance dashboards.
