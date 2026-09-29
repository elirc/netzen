# 04 Review Katas

## Kata 1: Add Priority Change Endpoint

**Author intent:** Let agents edit ticket priority.

**Fake diff summary:** Adds `PUT /api/tickets/{id}/priority` and calls `ticket.UpdatePriority`.

**Resembles:** Existing mutation routes at `backend/SupportDesk.Api/Endpoints/TicketEndpoints.cs:40-47` and domain method at `backend/SupportDesk.Api/Domain/Ticket.cs:226-230`.

**Expected findings:** Blocking: must include `expectedVersion` and permission check. Important: should broadcast update. Optional: decide whether SLA should recalculate.

**Good review comment example:** > Blocking: this mutation needs the same optimistic concurrency pattern as assignment/status. Please add `ExpectedVersion` like `backend/SupportDesk.Api/Application/Commands/AssignTicket.cs:3-6` and route through `ITicketStore.UpdateAsync` at `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:28-49`.

## Kata 2: Show Internal Notes To Everyone

**Author intent:** Simplify comment rendering.

**Fake diff summary:** Removes server-side comment filter.

**Resembles:** `backend/SupportDesk.Api/Application/Dto.cs:92-120`.

**Expected findings:** Blocking: internal note leakage. Important: add permission tests.

**Good review comment example:** > Blocking: this removes the read authorization boundary for internal notes. The current filter at `backend/SupportDesk.Api/Application/Dto.cs:94-96` is server-side so sensitive notes never cross to unauthorized clients.

## Kata 3: Client Retries Failed Comments

**Author intent:** Improve flaky network behavior.

**Fake diff summary:** Automatically retries POST comment three times.

**Resembles:** `frontend/src/api.ts:74-83` and domain append at `backend/SupportDesk.Api/Domain/Ticket.cs:203-216`.

**Expected findings:** Blocking: duplicate comment risk without idempotency key. Important: conflict handling.

**Good review comment example:** > Important: POST comment is not idempotent today; each successful retry appends a new comment at `backend/SupportDesk.Api/Domain/Ticket.cs:203`. Please add an idempotency design before automatic retries.

## Kata 4: Add Team SignalR Groups

**Author intent:** Reduce update noise.

**Fake diff summary:** Sends all team tickets to group named by `TeamId`.

**Resembles:** Current single inbox group at `backend/SupportDesk.Api/Realtime/TicketHub.cs:7` and notifier at `backend/SupportDesk.Api/Realtime/SignalRTicketNotifier.cs:15-22`.

**Expected findings:** Blocking: group join must be authorized. Important: unassigned tickets.

**Good review comment example:** > Blocking: group membership must derive from verified user context, not a client-provided group name. Current auth is header-based at `backend/SupportDesk.Api/Auth/HeaderUserContext.cs:12-20`, so document whether this remains demo-only or add real auth first.

## Kata 5: Replace Store With Static List

**Author intent:** Simplify in-memory storage.

**Fake diff summary:** Removes `ConcurrentDictionary`, lock, and snapshots.

**Resembles:** `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:7-49`.

**Expected findings:** Blocking: thread safety and lost update risk. Important: mutable aggregate escaping.

**Good review comment example:** > Blocking: the current singleton store is shared across requests at `backend/SupportDesk.Api/Program.cs:35`, so removing `_writeLock` and snapshots from `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:9-25` reintroduces shared mutable state bugs.

## Kata 6: Add Search By Comment Author

**Author intent:** Help agents find conversations by staff member.

**Fake diff summary:** Adds comment author matching to search.

**Resembles:** Search body match at `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:58-68`.

**Expected findings:** Important: privacy, performance, tests for internal note visibility.

**Good review comment example:** > Important: searching comment author is fine, but be careful not to let unauthorized users infer internal note participants. Search results are later permission-filtered at `backend/SupportDesk.Api/Application/Queries/SearchTickets.cs:19-21`; tests should cover unrelated agents and customers.

## Kata 7: Split App Component

**Author intent:** Improve frontend maintainability.

**Fake diff summary:** Extracts `TicketList`, `Composer`, `PropertiesPanel`.

**Resembles:** Large component at `frontend/src/App.tsx:35-426`.

**Expected findings:** Important: preserve state ownership and effect cleanup. Optional: add component tests once tooling exists.

**Good review comment example:** > This split looks useful. Please keep the SignalR connection lifecycle equivalent to `frontend/src/App.tsx:58-69`; duplicate connections would cause repeated list updates.

## Kata 8: Add Real Auth

**Author intent:** Replace demo headers with JWT.

**Fake diff summary:** Adds authentication middleware and maps claims to `IUserContext`.

**Resembles:** Current user context abstraction at `backend/SupportDesk.Api/Application/IUserContext.cs:5-11` and header implementation at `backend/SupportDesk.Api/Auth/HeaderUserContext.cs:6-32`.

**Expected findings:** Blocking: tests for role/team/customer claims. Important: remove client demo headers at `frontend/src/api.ts:19-24`.

**Good review comment example:** > Good direction. Please keep `IUserContext` as the app-layer contract, but remove trusted client identity headers from `frontend/src/api.ts:19-24` and add API tests proving `Permissions.CanReadTicket` still protects routes.
