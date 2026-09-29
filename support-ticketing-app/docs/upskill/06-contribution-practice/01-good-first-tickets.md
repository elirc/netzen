# 01 Good First Tickets

## Ticket 1: Add A Canned Reply

**Difficulty:** Easy **Time:** 20m **Skills:** reading data stores, UI verification

**Story:** As an agent, I want a shipping-delay canned reply so I can answer common questions quickly.

**Acceptance criteria:**

- [ ] New reply exists in `backend/SupportDesk.Api/Infrastructure/InMemoryCannedReplyStore.cs:8-34`.
- [ ] Reply appears in composer select at `frontend/src/App.tsx:307-313`.

**Read first:** `backend/SupportDesk.Api/Infrastructure/InMemoryCannedReplyStore.cs:8-34`.

**Files likely touched:** `backend/SupportDesk.Api/Infrastructure/InMemoryCannedReplyStore.cs`.

**Review questions:** Is the body customer-safe? Are tags normalized?

## Ticket 2: Add Empty Tags Helper Text

**Difficulty:** Easy **Time:** 30m **Skills:** React rendering

**Story:** As an agent, I want empty tag state to be clear.

**Acceptance criteria:** Show a small "No tags" text when `selected.tags` is empty.

**Read first:** `frontend/src/App.tsx:353-357`.

**Files likely touched:** `frontend/src/App.tsx`.

**Review questions:** Does layout still fit mobile? See CSS media rule at `frontend/src/styles.css:255-274`.

## Ticket 3: Clarify Concurrency Error Copy

**Difficulty:** Easy **Time:** 30m **Skills:** error UX

**Acceptance criteria:** Stale version errors from `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:39-42` produce a friendlier UI message.

**Read first:** `frontend/src/api.ts:36-39`, `frontend/src/App.tsx:268-270`.

**Files likely touched:** `frontend/src/api.ts` or `frontend/src/App.tsx`.

## Ticket 4: Add Priority Badge Title

**Difficulty:** Easy **Time:** 20m **Skills:** JSX, accessibility

**Acceptance criteria:** Priority badge includes a title attribute explaining priority.

**Read first:** `frontend/src/App.tsx:391-393`.

**Files likely touched:** `frontend/src/App.tsx`.

## Ticket 5: Document Demo Headers In UI Source

**Difficulty:** Easy **Time:** 20m **Skills:** comments, security awareness

**Acceptance criteria:** Add a concise comment above demo headers explaining they are not production auth.

**Read first:** `frontend/src/api.ts:19-24`, `backend/SupportDesk.Api/Auth/HeaderUserContext.cs:12-20`.

**Files likely touched:** `frontend/src/api.ts`.

## Ticket 6: Add Status Count Display

**Difficulty:** Easy **Time:** 45m **Skills:** derived state

**Acceptance criteria:** Sidebar shows counts by status using ticket list.

**Read first:** `frontend/src/App.tsx:77-80`, `frontend/src/App.tsx:237-240`.

**Files likely touched:** `frontend/src/App.tsx`, maybe `frontend/src/styles.css`.

## Ticket 7: Add OnHold Seed Ticket

**Difficulty:** Easy **Time:** 30m **Skills:** seed data

**Acceptance criteria:** Seed data contains one OnHold ticket.

**Read first:** `backend/SupportDesk.Api/Infrastructure/SeedData.cs:18-57`, enum at `backend/SupportDesk.Api/Domain/TicketEnums.cs:3-11`.

**Files likely touched:** `backend/SupportDesk.Api/Infrastructure/SeedData.cs`.

## Ticket 8: Improve New Ticket Created Location

**Difficulty:** Easy **Time:** 30m **Skills:** HTTP basics

**Acceptance criteria:** `Created` response location includes the new ticket id.

**Read first:** `backend/SupportDesk.Api/Endpoints/TicketEndpoints.cs:34-35`.

**Files likely touched:** `backend/SupportDesk.Api/Endpoints/TicketEndpoints.cs`.

**What could go wrong:** Handler currently returns the DTO inline, so endpoint needs access to `Id`.

## Ticket 9: Add Loading State For Canned Replies

**Difficulty:** Easy **Time:** 45m **Skills:** React state

**Acceptance criteria:** Composer select shows loading or disabled state until replies load.

**Read first:** `frontend/src/App.tsx:54-56`, `frontend/src/App.tsx:307-313`.

**Files likely touched:** `frontend/src/App.tsx`.

## Ticket 10: Add Page Size Constant

**Difficulty:** Easy **Time:** 20m **Skills:** API client cleanup

**Acceptance criteria:** Replace magic `"50"` with named constant.

**Read first:** `frontend/src/api.ts:44-45`.

**Files likely touched:** `frontend/src/api.ts`.

## Ticket 11: Add Comment Count To Detail Header

**Difficulty:** Easy **Time:** 30m **Skills:** DTO usage

**Acceptance criteria:** Detail header displays `comments.length`.

**Read first:** DTO has comments at `frontend/src/types.ts:37-40`; header at `frontend/src/App.tsx:273-285`.

**Files likely touched:** `frontend/src/App.tsx`.

## Ticket 12: Add Tag Trimming Note To Docs

**Difficulty:** Easy **Time:** 20m **Skills:** documentation

**Acceptance criteria:** README mentions tag normalization.

**Read first:** `backend/SupportDesk.Api/Domain/Ticket.cs:219-238`.

**Files likely touched:** `README.md`.

## Ticket 13: Add Assignee Placeholder

**Difficulty:** Easy **Time:** 15m **Skills:** UI polish

**Acceptance criteria:** Assignment agent input has placeholder `agent-1`.

**Read first:** `frontend/src/App.tsx:334-344`.

**Files likely touched:** `frontend/src/App.tsx`.

## Ticket 14: Add Canned Reply API Error State

**Difficulty:** Easy **Time:** 45m **Skills:** error handling

**Acceptance criteria:** Failure to load canned replies has a specific error message.

**Read first:** `frontend/src/App.tsx:54-56`, shared error banner at `frontend/src/App.tsx:268-270`.

**Files likely touched:** `frontend/src/App.tsx`.

## Ticket 15: Add Verification Note For Missing Tests

**Difficulty:** Easy **Time:** 20m **Skills:** documentation discipline

**Acceptance criteria:** README clearly states tests are not yet present.

**Read first:** Current run notes at `README.md:40-74`.

**Files likely touched:** `README.md`.

## Ticket 16: Improve SLA Label Copy

**Difficulty:** Easy **Time:** 20m **Skills:** UI copy

**Acceptance criteria:** SLA labels distinguish "due" from "breached".

**Read first:** `frontend/src/App.tsx:347-350`, `frontend/src/App.tsx:399-405`.

**Files likely touched:** `frontend/src/App.tsx`.
