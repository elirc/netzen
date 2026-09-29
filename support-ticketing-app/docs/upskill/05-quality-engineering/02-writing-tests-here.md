# 02 Writing Tests Here

No test project exists yet. These recipes are inferred and should be implemented after adding a test project.

## Recipe 1: Domain Happy Path

Command: `dotnet test --filter TicketCreate`

Read first: `backend/SupportDesk.Api/Domain/Ticket.cs:72-134`.

Assert:

- `Status == New` from `backend/SupportDesk.Api/Domain/Ticket.cs:121`.
- `Version == 1` from `backend/SupportDesk.Api/Domain/Ticket.cs:131`.
- Tags normalized through constructor logic at `backend/SupportDesk.Api/Domain/Ticket.cs:46`.

## Recipe 2: Validation Failure

Command: `dotnet test --filter EmptySubject`

Read first: `backend/SupportDesk.Api/Domain/Ticket.cs:83-96`.

Assert a `DomainException` for empty subject, description, and customer email.

## Recipe 3: Permission Failure

Command: `dotnet test --filter CanReadTicket`

Read first: `backend/SupportDesk.Api/Application/Permissions.cs:7-39`.

Build fake users: admin, manager, assigned agent, team agent, unrelated agent, customer.

## Recipe 4: Concurrency Conflict

Command: `dotnet test --filter ExpectedVersion`

Read first: `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:28-49`.

Arrange a ticket at version 2, call update with version 1, assert `ConcurrencyException`.

## Recipe 5: API Problem Details

Command: `dotnet test --filter ProblemDetails`

Read first: `backend/SupportDesk.Api/Auth/ApiExceptionHandler.cs:17-42`.

Use `WebApplicationFactory` after adding package support. Assert 409 for stale version and JSON problem detail.

## Recipe 6: Internal Note Visibility

Command: `dotnet test --filter InternalNotes`

Read first: `backend/SupportDesk.Api/Application/Dto.cs:92-120`.

Assert staff sees internal note and customer does not.

## Recipe 7: UI Composer

Command: `npm test -- composer`

Read first: `frontend/src/App.tsx:301-324`.

After adding a UI test runner, verify canned reply selection sets the textarea and internal checkbox sends `Internal`.

## Recipe 8: SignalR Contract

Command: `dotnet test --filter RealtimeNotifier`

Read first: `backend/SupportDesk.Api/Realtime/SignalRTicketNotifier.cs:15-22` and `frontend/src/api.ts:131-133`.

Assert event names and payload shapes remain stable.
