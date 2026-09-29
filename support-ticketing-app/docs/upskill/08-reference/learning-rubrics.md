# Learning Rubrics

## Junior

| Skill | Observable behavior | Self-check |
| --- | --- | --- |
| Runs project | Can explain inferred backend/frontend commands from `backend/SupportDesk.Api/SupportDesk.Api.csproj:1-8` and `frontend/package.json:6-9` | I can start both once SDK/deps exist. |
| Reads a flow | Can trace create ticket from `frontend/src/App.tsx:163-185` to `backend/SupportDesk.Api/Domain/Ticket.cs:72-134` | I can name each file and owner. |
| Small changes | Can add a canned reply at `backend/SupportDesk.Api/Infrastructure/InMemoryCannedReplyStore.cs:8-34` | I keep changes scoped. |
| Safe debugging | Can follow problem details from `backend/SupportDesk.Api/Auth/ApiExceptionHandler.cs:17-42` to `frontend/src/api.ts:36-39` | I reproduce before changing. |

## Mid-Level

| Skill | Observable behavior | Self-check |
| --- | --- | --- |
| Cross-layer design | Can add a mutation following endpoint/handler/domain/store/UI patterns | I include permission and concurrency. |
| Testing judgment | Can design tests for `backend/SupportDesk.Api/Domain/Ticket.cs:72-239` and `backend/SupportDesk.Api/Application/Permissions.cs:7-39` | I test behavior, not implementation trivia. |
| Review quality | Flags missing auth/version/DTO impacts using anchors | My comments are specific and actionable. |
| Debugging | Narrows stale comment conflict through `frontend/src/App.tsx:116`, `frontend/src/api.ts:74-83`, and `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:39-42` | I identify layer before fix. |

## Senior

| Skill | Observable behavior | Self-check |
| --- | --- | --- |
| Architecture critique | Explains tradeoffs of in-memory store, demo auth, direct SignalR sends | I separate confirmed risk from hypothesis. |
| Migration design | Plans EF/auth/outbox changes with rollout and rollback | I reduce blast radius. |
| Reliability | Adds idempotency, observability, and failure recovery to side effects | I ask how we know it broke. |
| Mentorship | Turns review into learning without blocking unnecessarily | I protect maintainers and help contributors grow. |

## Self Assessment

Score each 1-3:

- 1: I can point to code.
- 2: I can explain why it exists.
- 3: I can predict failure modes and propose tests.

Target progression:

- Weekend: mostly 1s.
- Two weeks: mix of 1s and 2s.
- Eight weeks: mostly 2s with several 3s.
- Senior practice: consistent 3s on architecture/security/reliability topics.
