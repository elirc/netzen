# 05 Pattern Catalog

## Pattern: Minimal API Route Group

**Problem:** Keep public HTTP routes discoverable without controllers.

**General shape:** Map a route group, bind request data, call a handler.

**Real example:** `backend/SupportDesk.Api/Endpoints/TicketEndpoints.cs:10-49`

**Why it works:** Endpoints stay thin and handlers carry business workflow.

**Failure modes:** Business logic creeps into endpoint lambdas.

**Use when:** APIs are small and route definitions fit in one file.

**Avoid when:** You need extensive filters, conventions, or controller ecosystem features.

**Drill:** Add a route sketch for priority change using current handlers as model.

## Pattern: CQRS-ish Command Handler

**Problem:** Mutations need orchestration without bloated endpoints.

**Real example:** `backend/SupportDesk.Api/Application/Commands/AddComment.cs:23-45`

**Second:** `backend/SupportDesk.Api/Application/Commands/CreateTicket.cs:27-49`

**Why it works:** Permission, mutation, persistence, notification, and DTO return have one owner.

**Failure modes:** Handler becomes transaction script with domain rules duplicated.

**Use when:** A workflow has multiple steps.

**Avoid when:** The endpoint is pure read/simple pass-through.

**Drill:** Identify the one line that calls domain logic in each command.

## Pattern: Query Handler

**Problem:** Reads need different optimization and shaping than writes.

**Real example:** `backend/SupportDesk.Api/Application/Queries/SearchTickets.cs:14-28`

**Second:** `backend/SupportDesk.Api/Application/Queries/GetTicket.cs:14-24`

**Why it works:** Read authorization and DTO shaping are explicit.

**Failure modes:** Paging before auth, accidental leakage, unbounded queries.

**Drill:** Explain why `SearchTicketsHandler` maps summaries, not details.

## Pattern: Aggregate Root

**Problem:** Related state must change consistently.

**Real example:** `backend/SupportDesk.Api/Domain/Ticket.cs:3-239`

**Why it works:** Assignment, status, comments, tags, SLA, and version share one invariant boundary.

**Failure modes:** Setters become public, handlers mutate internals directly.

**Drill:** Mark all public mutation methods in `Ticket`.

## Pattern: Static Factory

**Problem:** Object creation must enforce invariants.

**Real example:** `backend/SupportDesk.Api/Domain/Ticket.cs:72-134`

**Why it works:** Required fields, normalization, SLA, number, and initial version happen together.

**Failure modes:** Separate constructors bypass rules.

**Drill:** Add expected tests for empty subject, empty description, empty email.

## Pattern: Optimistic Concurrency Token

**Problem:** Prevent lost updates from stale clients.

**Real example:** Version check at `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:39-42`

**Second:** UI sends version at `frontend/src/App.tsx:116`, `frontend/src/App.tsx:129`, `frontend/src/App.tsx:141`, `frontend/src/App.tsx:155`

**Why it works:** Mutations only apply when client state matches server state.

**Failure modes:** Poor conflict UX, missing version on new mutation endpoints.

**Drill:** Write a fake stale update response and expected UI behavior.

## Pattern: Snapshot Copy

**Problem:** Avoid shared mutable aggregate instances escaping the store.

**Real example:** `backend/SupportDesk.Api/Domain/Ticket.cs:136-156`

**Second:** Store returns snapshots at `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:18-25`

**Why it works:** Callers cannot mutate the stored instance accidentally.

**Failure modes:** Snapshot is shallow for child objects if child objects become mutable.

**Drill:** Explain why immutable `TicketComment` at `backend/SupportDesk.Api/Domain/TicketComment.cs:3-26` helps.

## Pattern: Resource Permission Function

**Problem:** Authorization depends on the specific ticket.

**Real example:** `backend/SupportDesk.Api/Application/Permissions.cs:7-39`

**Second:** Used before mutation at `backend/SupportDesk.Api/Application/Commands/ReplaceTicketTags.cs:22-33`

**Why it works:** Rules are centralized and reusable.

**Failure modes:** Endpoint added without calling permission function.

**Drill:** Find every call to `Permissions.` and classify read/write.

## Pattern: DTO Mapping

**Problem:** Domain shape should not leak directly to clients.

**Real example:** `backend/SupportDesk.Api/Application/Dto.cs:67-134`

**Second:** TS client contracts at `frontend/src/types.ts:1-63`

**Why it works:** API controls public fields and read visibility.

**Failure modes:** DTO and TS type drift; leaking internal notes.

**Drill:** Compare each `TicketSummaryDto` field to `TicketSummary`.

## Pattern: Problem Details Exception Mapping

**Problem:** Clients need predictable error responses.

**Real example:** `backend/SupportDesk.Api/Auth/ApiExceptionHandler.cs:17-42`

**Second:** Client reads problem detail at `frontend/src/api.ts:36-39`

**Why it works:** App exceptions map to HTTP status consistently.

**Failure modes:** Catch-all hides logs; validation errors lack field detail.

**Drill:** Map `ConcurrencyException` to expected UI message.

## Pattern: Central API Client

**Problem:** Avoid repeated fetch/header/error code.

**Real example:** `frontend/src/api.ts:26-42`

**Second:** Typed functions at `frontend/src/api.ts:44-121`

**Why it works:** All calls share base URL and headers.

**Failure modes:** Hardcoded demo identity becomes production risk.

**Drill:** Add `AbortController` design without product code.

## Pattern: Hook-Based UI State

**Problem:** React state coordinates forms, selection, list, and live events.

**Real example:** `frontend/src/App.tsx:35-80`

**Second:** Mutations update state at `frontend/src/App.tsx:111-185`

**Why it works:** One component is readable for a learning app.

**Failure modes:** Component grows too large, stale closures, reconnection churn.

**Drill:** Name three pieces of state that could move into custom hooks.

## Pattern: Realtime Notifier Abstraction

**Problem:** Handlers should not depend directly on SignalR.

**Real example:** Interface at `backend/SupportDesk.Api/Application/IRealtimeTicketNotifier.cs:3-8`

**Second:** Implementation at `backend/SupportDesk.Api/Realtime/SignalRTicketNotifier.cs:6-23`

**Why it works:** Application layer can be tested with a fake notifier.

**Failure modes:** Event payloads still become implicit contracts.

**Drill:** Design a fake notifier for tests.

## Pattern: Seed Data

**Problem:** Local app needs realistic state without external setup.

**Real example:** `backend/SupportDesk.Api/Infrastructure/SeedData.cs:18-57`

**Why it works:** Demonstrates assigned, public-comment, internal-note, and pending tickets.

**Failure modes:** Seed data can hide empty-state bugs.

**Drill:** Add an unassigned urgent ticket scenario in a test plan.

## Pattern: Canned Reply Store

**Problem:** UI needs reusable content.

**Real example:** `backend/SupportDesk.Api/Infrastructure/InMemoryCannedReplyStore.cs:8-34`

**Second:** Composer select at `frontend/src/App.tsx:307-313`

**Why it works:** Keeps content outside UI component.

**Failure modes:** No editing, permissioning, localization, or versioning.

**Drill:** Design CRUD endpoints for canned replies.

## Pattern: String Enum Contract

**Problem:** Browser and server must agree on vocabulary.

**Real example:** JSON enum converters at `backend/SupportDesk.Api/Program.cs:24-32`

**Second:** TS unions at `frontend/src/types.ts:1-3`

**Why it works:** Payloads are readable and strongly typed.

**Failure modes:** Rename becomes breaking API change.

**Drill:** Plan a backward-compatible enum rename.
