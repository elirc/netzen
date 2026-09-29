# 05 Quality Engineering

Quality engineering asks: how do we know this works, stays correct, and fails safely?

Current reality:

- No tests were found in inventory.
- No CI workflow was found.
- Error mapping exists at `backend/SupportDesk.Api/Auth/ApiExceptionHandler.cs:17-42`.
- Permission rules exist at `backend/SupportDesk.Api/Application/Permissions.cs:5-39`.
- Concurrency exists at `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:39-42`.

Use this section to design the missing safety net before adding larger features.
