# 03 Maintainer Communication

## Asking Without Outsourcing Thinking

Weak:

> How should I add real auth?

Better:

> I traced demo auth through `frontend/src/api.ts:19-24` and `backend/SupportDesk.Api/Auth/HeaderUserContext.cs:12-20`. I think we can preserve `IUserContext` at `backend/SupportDesk.Api/Application/IUserContext.cs:5-11` and replace only the implementation. Should local dev keep header auth behind an environment flag, or should we use test JWTs?

## Bug Report Template

```md
## Symptom
## Reproduction
## Expected
## Actual
## Anchors inspected
## Hypothesis
## Minimal fix idea
```

Example anchors for a stale comment conflict: `frontend/src/App.tsx:116`, `frontend/src/api.ts:74-83`, `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:39-42`.

## Feature Request Template

```md
## User story
## Current behavior
## Proposed behavior
## Files likely touched
## Risks
## Tests
```

## Responding To Review

Good:

> Good catch. I added the missing `ExpectedVersion` to match the existing mutation pattern at `backend/SupportDesk.Api/Application/Commands/AssignTicket.cs:3-6` and added a stale-version test plan.

Good disagreement:

> I agree the UI split is attractive, but this PR is focused on conflict UX. Since `frontend/src/App.tsx:35-426` is already large, I opened a follow-up ticket instead of mixing refactor with behavior change.
