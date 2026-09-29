# 02 Mid-Level Feature Tickets

## Ticket 1: Priority Change Workflow

**Difficulty:** Medium **Time:** 3-5h **Skills:** API, handler, domain, UI, tests

**Story:** As an agent, I want to change ticket priority so SLA expectations can reflect urgency.

**Acceptance criteria:**

- [ ] Add command request with `Priority` and `ExpectedVersion`.
- [ ] Use `ITicketStore.UpdateAsync` at `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:28-49`.
- [ ] Check permission, likely `CanChangeStatus` equivalent at `backend/SupportDesk.Api/Application/Permissions.cs:28-30`.
- [ ] Decide whether SLA recalculates from `backend/SupportDesk.Api/Domain/Ticket.cs:98-112`.
- [ ] Add UI control near `frontend/src/App.tsx:279-284`.

**Risk:** Recalculating SLA may surprise agents. Document product decision.

## Ticket 2: Conflict Refresh UX

**Difficulty:** Medium **Time:** 3h **Skills:** concurrency, UX

**Acceptance criteria:** On 409 from `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:39-42`, UI offers refresh and preserves draft comment.

**Read first:** `frontend/src/App.tsx:111-123`, `frontend/src/api.ts:36-39`.

**Risk:** Losing user-written comments.

## Ticket 3: Extract React Hooks

**Difficulty:** Medium **Time:** 4-6h **Skills:** React architecture

**Acceptance criteria:** Extract data loading/mutations from `frontend/src/App.tsx:35-185` into hooks while preserving behavior.

**Read first:** SignalR lifecycle at `frontend/src/App.tsx:58-69`.

**Risk:** Duplicate connections or stale closures.

## Ticket 4: Add API Integration Tests

**Difficulty:** Medium **Time:** 5-8h **Skills:** ASP.NET testing

**Acceptance criteria:** Create test project and cover create, stale comment conflict, forbidden read, internal note visibility.

**Read first:** endpoints at `backend/SupportDesk.Api/Endpoints/TicketEndpoints.cs:10-49`, exception handler at `backend/SupportDesk.Api/Auth/ApiExceptionHandler.cs:17-42`.

**Risk:** Requires package installation and SDK availability.

## Ticket 5: Team-Based SignalR Groups

**Difficulty:** Medium **Time:** 5-8h **Skills:** SignalR, authz

**Acceptance criteria:** Clients receive only relevant team ticket events.

**Read first:** current group join at `backend/SupportDesk.Api/Realtime/TicketHub.cs:5-8`, team permission at `backend/SupportDesk.Api/Application/Permissions.cs:7-11`.

**Risk:** Current header auth is spoofable; keep demo caveat or add real auth first.

## Ticket 6: Server-Side Canned Reply Search

**Difficulty:** Medium **Time:** 3-5h **Skills:** endpoint/query/UI

**Acceptance criteria:** Add optional query/tag filters to `/api/canned-replies`.

**Read first:** store at `backend/SupportDesk.Api/Infrastructure/InMemoryCannedReplyStore.cs:8-34`, endpoint at `backend/SupportDesk.Api/Endpoints/CannedReplyEndpoints.cs:8-12`.

**Risk:** Content visibility rules if internal-only replies are added later.

## Ticket 7: Add Time Abstraction

**Difficulty:** Medium **Time:** 3-5h **Skills:** testability

**Acceptance criteria:** Replace direct `DateTimeOffset.UtcNow` in handlers with injectable clock.

**Read first:** `backend/SupportDesk.Api/Application/Commands/CreateTicket.cs:34`, `backend/SupportDesk.Api/Application/Commands/AddComment.cs:37`, `AssignTicket.cs:33`, `ChangeTicketStatus.cs:34`, `ReplaceTicketTags.cs:32`.

**Risk:** Overengineering if no tests follow.

## Ticket 8: Add Rate Limiting

**Difficulty:** Medium **Time:** 4-6h **Skills:** security, ASP.NET middleware

**Acceptance criteria:** Add rate limits for create/comment endpoints.

**Read first:** route mapping at `backend/SupportDesk.Api/Endpoints/TicketEndpoints.cs:34-38`, middleware order at `backend/SupportDesk.Api/Program.cs:51-59`.

**Risk:** Avoid blocking SignalR or local dev unintentionally.

## Ticket 9: Add Search Debounce

**Difficulty:** Medium **Time:** 2-4h **Skills:** React effects

**Acceptance criteria:** Search input waits briefly before API call.

**Read first:** filter effect at `frontend/src/App.tsx:50-52`, query serialization at `frontend/src/api.ts:44-53`.

**Risk:** Keep status/tag filters responsive.

## Ticket 10: Add Sort Controls

**Difficulty:** Medium **Time:** 4-6h **Skills:** API contract, store query, UI

**Acceptance criteria:** User can sort by updated date or SLA due date.

**Read first:** current ordering at `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:101-104`.

**Risk:** Contract drift between C# criteria and TS filters.

## Ticket 11: Add Customer Reply Mode

**Difficulty:** Medium **Time:** 5-8h **Skills:** permissions, UI, tests

**Acceptance criteria:** Customer demo identity can add public comments but cannot internal-note or assign.

**Read first:** comment permission at `backend/SupportDesk.Api/Application/Permissions.cs:16-22`, UI visibility toggle at `frontend/src/App.tsx:314-320`.

**Risk:** UI must not be the only enforcement layer.

## Ticket 12: Add Basic CI

**Difficulty:** Medium **Time:** 2-4h **Skills:** GitHub Actions

**Acceptance criteria:** Workflow runs backend build and frontend build.

**Read first:** backend project `backend/SupportDesk.Api/SupportDesk.Api.csproj:1-8`, frontend scripts `frontend/package.json:6-9`.

**Risk:** Requires lockfile and SDK version decision.
