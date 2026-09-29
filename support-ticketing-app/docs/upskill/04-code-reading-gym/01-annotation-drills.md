# 01 Annotation Drills

## Drill 1: Program Composition

Annotate `backend/SupportDesk.Api/Program.cs:12-63`.

Prompt: Which lines register services, which lines configure middleware, which lines expose public interfaces, and which line mutates startup state?

Self-grade: Basic labels host sections. Solid explains DI lifetimes at `backend/SupportDesk.Api/Program.cs:33-46`. Strong identifies risks of startup seed load at `backend/SupportDesk.Api/Program.cs:61`.

## Drill 2: Ticket Creation Invariant

Annotate `backend/SupportDesk.Api/Domain/Ticket.cs:72-134`.

Prompt: Mark validation, normalization, SLA calculation, initial status, id, number, version.

Self-grade: Basic finds required fields. Solid explains priority-to-SLA. Strong proposes table-driven tests for all priorities.

## Drill 3: Comment Mutation

Annotate `backend/SupportDesk.Api/Domain/Ticket.cs:183-217`.

Prompt: What are the inputs, output, state changes, side effects inside aggregate, and edge cases?

Self-grade: Basic sees body validation. Solid sees first response and status updates. Strong questions `authorId != CustomerEmail` at `backend/SupportDesk.Api/Domain/Ticket.cs:205`.

## Drill 4: Optimistic Concurrency

Annotate `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:28-49`.

Prompt: Why clone, why lock, why check version, when can mutation throw?

Self-grade: Basic explains stale version. Solid explains lock scope. Strong proposes how EF would implement the same contract.

## Drill 5: Search Query

Annotate `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:51-107` and `backend/SupportDesk.Api/Application/Queries/SearchTickets.cs:14-28`.

Prompt: Which filters run in the store, which run in the handler, and what paging semantics result?

Self-grade: Basic lists filters. Solid traces page/pageSize. Strong identifies auth-after-search risk.

## Drill 6: Permission Matrix

Annotate `backend/SupportDesk.Api/Application/Permissions.cs:7-39`.

Prompt: Convert each method into a truth table.

Self-grade: Basic names roles. Solid includes resource ownership. Strong adds IDOR tests for `GetTicketHandler` at `backend/SupportDesk.Api/Application/Queries/GetTicket.cs:16-24`.

## Drill 7: DTO Visibility

Annotate `backend/SupportDesk.Api/Application/Dto.cs:92-120`.

Prompt: Which comments are filtered and why is this in mapping rather than UI?

Self-grade: Basic sees public/internal. Solid connects to `CanReadInternalNotes`. Strong tests customer cannot see internal notes.

## Drill 8: Error Boundary

Annotate `backend/SupportDesk.Api/Auth/ApiExceptionHandler.cs:17-42`.

Prompt: Map exception type to status code and client behavior.

Self-grade: Basic lists status. Solid follows client `request<T>` at `frontend/src/api.ts:36-39`. Strong proposes structured logging.

## Drill 9: React Effects

Annotate `frontend/src/App.tsx:50-80`.

Prompt: What triggers each effect? What cleans up? What might over-fetch or reconnect?

Self-grade: Basic labels dependencies. Solid explains cleanup at `frontend/src/App.tsx:66-68`. Strong proposes stable connection ownership.

## Drill 10: Mutation UX

Annotate `frontend/src/App.tsx:111-161`.

Prompt: How does each mutation use version, update state, and surface errors?

Self-grade: Basic sees API calls. Solid sees `upsertSummary` use. Strong designs conflict refresh UX.
