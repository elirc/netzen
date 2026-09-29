# 00 Fast Track

Use this when you want a weekend win: run the project, trace two flows, and make one safe contribution.

## Commands

| Command | Status | Evidence |
| --- | --- | --- |
| `cd backend/SupportDesk.Api && dotnet run` | Inferred, not executed | Target framework is `net8.0` at `backend/SupportDesk.Api/SupportDesk.Api.csproj:1-8`; launch URL is `http://localhost:5208` at `backend/SupportDesk.Api/Properties/launchSettings.json:4-10`. |
| `cd frontend && npm install` | Inferred, not executed | Scripts and dependencies are declared at `frontend/package.json:6-18`. |
| `cd frontend && npm run dev` | Inferred, not executed | Vite dev script is `frontend/package.json:6-9`; API base defaults to `http://localhost:5208` at `frontend/src/api.ts:17`. |
| `dotnet test` | Not available yet | No test project found in file inventory; see `08-reference/verification-log.md`. |

## First 10 Files

1. `README.md:1-74` - repo promise, run notes, demo users.
2. `backend/SupportDesk.Api/Program.cs:10-63` - service registration and HTTP pipeline.
3. `backend/SupportDesk.Api/Endpoints/TicketEndpoints.cs:10-49` - public ticket API surface.
4. `backend/SupportDesk.Api/Application/Commands/CreateTicket.cs:5-51` - command request shape and create flow.
5. `backend/SupportDesk.Api/Domain/Ticket.cs:72-134` - ticket creation invariant and SLA calculation.
6. `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:28-49` - optimistic concurrency.
7. `backend/SupportDesk.Api/Application/Permissions.cs:7-39` - authorization rules.
8. `frontend/src/api.ts:26-42` - shared fetch wrapper and demo identity headers.
9. `frontend/src/App.tsx:82-185` - client-side data mutations.
10. `frontend/src/types.ts:1-63` - TypeScript API contracts.

## Weekend Flow 1: Create Ticket

Trace UI submit at `frontend/src/App.tsx:163-185`, API client call at `frontend/src/api.ts:59-72`, endpoint binding at `backend/SupportDesk.Api/Endpoints/TicketEndpoints.cs:34-35`, handler authorization and persistence at `backend/SupportDesk.Api/Application/Commands/CreateTicket.cs:27-49`, domain validation and SLA creation at `backend/SupportDesk.Api/Domain/Ticket.cs:83-133`, and SignalR notification at `backend/SupportDesk.Api/Application/Commands/CreateTicket.cs:46-48`.

Drill: write a trace table with these columns: input value, owner, validation, mutation, output contract, failure mode.

## Weekend Flow 2: Add Comment

Trace form submit at `frontend/src/App.tsx:111-123`, API payload with `expectedVersion` at `frontend/src/api.ts:74-83`, endpoint at `backend/SupportDesk.Api/Endpoints/TicketEndpoints.cs:37-38`, concurrency gate at `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:28-49`, permission gate at `backend/SupportDesk.Api/Application/Commands/AddComment.cs:27-39`, domain mutation at `backend/SupportDesk.Api/Domain/Ticket.cs:183-217`, and live broadcast at `backend/SupportDesk.Api/Application/Commands/AddComment.cs:41-45`.

Drill: explain why the UI sends `selected.version` at `frontend/src/App.tsx:116` and what response the API should produce when stale.

## Small Safe Change

Add a canned reply. Read `backend/SupportDesk.Api/Infrastructure/InMemoryCannedReplyStore.cs:7-31`, add one reply, then verify it appears through `frontend/src/App.tsx:307-313`. This is safe because canned replies are read-only data exposed through `backend/SupportDesk.Api/Endpoints/CannedReplyEndpoints.cs:8-12`.

## Teach-Back

In five minutes, explain:

- Where runtime services are registered: `backend/SupportDesk.Api/Program.cs:33-46`.
- Where ticket invariants live: `backend/SupportDesk.Api/Domain/Ticket.cs:83-133` and `backend/SupportDesk.Api/Domain/Ticket.cs:171-217`.
- Where permissions happen before mutations: `backend/SupportDesk.Api/Application/Commands/AddComment.cs:27-39`, `backend/SupportDesk.Api/Application/Commands/AssignTicket.cs:23-35`.
- What this fast track skips: persistence migrations, real authentication, automated tests, deployment, observability, and production security hardening.
