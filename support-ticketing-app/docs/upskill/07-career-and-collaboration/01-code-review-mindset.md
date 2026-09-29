# 01 Code Review Mindset

Review in layers:

1. Does it work?
2. Is it correct under edge cases?
3. Will it stay correct as the repo changes?
4. Does it fit the architecture?
5. Is it kind to future maintainers?

## Repo Checklist

| Question | Anchor |
| --- | --- |
| Is every mutation protected by permission? | Existing checks at `backend/SupportDesk.Api/Application/Commands/AddComment.cs:31-35`, `AssignTicket.cs:27-31`, `ChangeTicketStatus.cs:28-32`, `ReplaceTicketTags.cs:27-30`. |
| Does every mutation include concurrency? | Existing request records include `ExpectedVersion` at `backend/SupportDesk.Api/Application/Commands/AddComment.cs:5-8`, `AssignTicket.cs:3-6`, `ChangeTicketStatus.cs:5-7`, `ReplaceTicketTags.cs:3-5`. |
| Are domain invariants in the domain? | `backend/SupportDesk.Api/Domain/Ticket.cs:72-239`. |
| Are internal notes filtered server-side? | `backend/SupportDesk.Api/Application/Dto.cs:92-120`. |
| Does UI state cleanup subscriptions? | `frontend/src/App.tsx:58-69`. |
| Are errors predictable for clients? | `backend/SupportDesk.Api/Auth/ApiExceptionHandler.cs:17-42`, `frontend/src/api.ts:36-39`. |

## Good Comment Examples

> Blocking: this endpoint changes ticket state but bypasses the version check pattern in `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:39-42`. Please route through `UpdateAsync` and add an `ExpectedVersion` field.

> Important: hiding the internal-note checkbox in React is useful UX, but the server still needs to enforce visibility. The current server-side read filter is at `backend/SupportDesk.Api/Application/Dto.cs:94-96`.

> Optional: this component extraction is easier to review if behavior stays identical first, especially the SignalR cleanup at `frontend/src/App.tsx:66-68`.
