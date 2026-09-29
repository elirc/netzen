# 02 Writing PRs And RFCs

## PR Template

```md
## What

## Why

## How

## Tests

## Risks

## Follow-ups
```

## Repo-Specific PR Guidance

- Mention which boundary changed: endpoint at `backend/SupportDesk.Api/Endpoints/TicketEndpoints.cs:10-49`, handler at `backend/SupportDesk.Api/Application/Commands/AddComment.cs:23-45`, domain at `backend/SupportDesk.Api/Domain/Ticket.cs:72-239`, store at `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:7-120`, or UI at `frontend/src/App.tsx:35-426`.
- If changing API contracts, cite DTO and TS types: `backend/SupportDesk.Api/Application/Dto.cs:5-65`, `frontend/src/types.ts:1-63`.
- If changing auth, include read/write permission matrix from `backend/SupportDesk.Api/Application/Permissions.cs:7-39`.
- If changing realtime, include server event names at `backend/SupportDesk.Api/Realtime/SignalRTicketNotifier.cs:15-22` and client handlers at `frontend/src/api.ts:131-133`.

## When To RFC

Use an RFC for:

- EF persistence replacing `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:7-120`.
- Real auth replacing `backend/SupportDesk.Api/Auth/HeaderUserContext.cs:6-32`.
- Outbox replacing direct notifier sends at `backend/SupportDesk.Api/Realtime/SignalRTicketNotifier.cs:15-22`.
- Large frontend split of `frontend/src/App.tsx:35-426`.

## RFC Template

```md
# RFC: [Title]

## Problem
## Goals
## Non-goals
## Current behavior with anchors
## Proposed design
## Alternatives
## Data model / contract changes
## Security
## Performance
## Test plan
## Rollout and rollback
## Open questions
```

## Commit Messages

Good:

- `Add stale-version conflict message to ticket composer`
- `Document demo header auth risk`
- `Add domain tests for ticket SLA rules`

Weak:

- `fix stuff`
- `refactor`
- `updates`
