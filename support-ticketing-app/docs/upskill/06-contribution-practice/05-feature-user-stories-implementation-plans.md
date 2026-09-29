# 05 Feature User Stories With Implementation Plans

These 20 feature stories are designed for a junior software engineer to practice real product work in this codebase. Each story includes a concrete implementation plan, examples, references, risk notes, and suggested checks.

Before starting any story:

1. Read the referenced files first.
2. Write down the current flow in your own words.
3. Make the smallest working change.
4. Add or update tests if the repo has a test project by then.
5. Manually verify the UI/API behavior.
6. Keep refactors separate unless the story explicitly asks for them.

Current important anchors:

- API route mapping: `backend/SupportDesk.Api/Endpoints/TicketEndpoints.cs:10-49`
- Command handlers: `backend/SupportDesk.Api/Application/Commands/CreateTicket.cs:14-51`, `backend/SupportDesk.Api/Application/Commands/AddComment.cs:10-47`
- Query handlers: `backend/SupportDesk.Api/Application/Queries/SearchTickets.cs:3-30`, `backend/SupportDesk.Api/Application/Queries/GetTicket.cs:3-26`
- Domain aggregate: `backend/SupportDesk.Api/Domain/Ticket.cs:3-239`
- Store contract: `backend/SupportDesk.Api/Application/ITicketStore.cs:5-12`
- In-memory store: `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:7-120`
- Permissions: `backend/SupportDesk.Api/Application/Permissions.cs:5-39`
- DTOs: `backend/SupportDesk.Api/Application/Dto.cs:5-134`
- React API client: `frontend/src/api.ts:17-137`
- React app: `frontend/src/App.tsx:35-426`
- TypeScript contracts: `frontend/src/types.ts:1-63`

## Story 1: Change Ticket Priority

**User story:** As a support agent, I want to change a ticket's priority so that SLA urgency can be corrected when new information arrives.

**Why this matters:** Priority drives SLA timestamps during ticket creation at `backend/SupportDesk.Api/Domain/Ticket.cs:98-112`. The domain already has `UpdatePriority` at `backend/SupportDesk.Api/Domain/Ticket.cs:226-230`, but no endpoint or UI calls it.

**Read first:**

- `backend/SupportDesk.Api/Application/Commands/ChangeTicketStatus.cs:5-42` - model for a mutation command.
- `backend/SupportDesk.Api/Application/Commands/AssignTicket.cs:3-41` - model for `ExpectedVersion`.
- `backend/SupportDesk.Api/Domain/Ticket.cs:226-230` - existing domain method.
- `frontend/src/App.tsx:279-284` - current priority badge area.
- `frontend/src/api.ts:98-107` - status mutation API pattern.

**Implementation plan:**

1. Add a command request record, for example `ChangeTicketPriorityRequest`, with `TicketPriority Priority` and `int ExpectedVersion`.
2. Add a handler `ChangeTicketPriorityHandler`.
3. Inject `ITicketStore`, `IUserContext`, and `IRealtimeTicketNotifier`, matching `ChangeTicketStatusHandler` at `backend/SupportDesk.Api/Application/Commands/ChangeTicketStatus.cs:11-20`.
4. Inside the handler, call `_store.UpdateAsync(ticketId, request.ExpectedVersion, ticket => { ... })`.
5. Check permission before mutation. A simple first pass can reuse `Permissions.CanChangeStatus` at `backend/SupportDesk.Api/Application/Permissions.cs:28-30`, but name the product decision in the PR.
6. Call `ticket.UpdatePriority(request.Priority, DateTimeOffset.UtcNow)`.
7. Broadcast `TicketUpdatedAsync` like `backend/SupportDesk.Api/Application/Commands/ChangeTicketStatus.cs:38-39`.
8. Register the handler in `backend/SupportDesk.Api/Program.cs:39-46`.
9. Add a route in `backend/SupportDesk.Api/Endpoints/TicketEndpoints.cs:40-47`, for example `PUT /api/tickets/{id}/priority`.
10. Add a frontend API function similar to `changeTicketStatus` in `frontend/src/api.ts:98-107`.
11. Add a `<select>` near `frontend/src/App.tsx:279-284` to choose priority.
12. Use `selected.version` just like status change does at `frontend/src/App.tsx:141`.

**Example request shape:**

```json
{
  "priority": "Urgent",
  "expectedVersion": 4
}
```

**Illustrative fake code: not from this repo.**

```csharp
public sealed record ChangeTicketPriorityRequest(
    TicketPriority Priority,
    int ExpectedVersion);
```

**Acceptance criteria:**

- [ ] Agent can change priority from the detail pane.
- [ ] API rejects stale versions with 409 through existing store behavior.
- [ ] API rejects unauthorized users.
- [ ] Live inbox row updates after priority change.
- [ ] Priority enum remains string-based JSON, consistent with `backend/SupportDesk.Api/Program.cs:24-32`.

**Suggested checks:**

- Manual: open a ticket, change priority, confirm badge changes.
- Manual: change status first in another tab, then try priority change with stale page and confirm conflict.
- Future test: handler test for permission, version, notifier call.

**Common mistakes:**

- Forgetting `ExpectedVersion`.
- Updating only React state without API mutation.
- Adding priority logic to the endpoint instead of a handler.
- Recalculating SLA without a product decision.

## Story 2: Add Ticket Channels

**User story:** As an agent, I want to see whether a ticket came from email, chat, phone, or web so I can adjust my response style.

**Why this matters:** Support inboxes usually route and prioritize based on channel. This teaches contract changes across domain, DTOs, TypeScript, seed data, create form, and search.

**Read first:**

- Domain enums at `backend/SupportDesk.Api/Domain/TicketEnums.cs:3-33`.
- Ticket properties at `backend/SupportDesk.Api/Domain/Ticket.cs:50-70`.
- Create request at `backend/SupportDesk.Api/Application/Commands/CreateTicket.cs:5-12`.
- DTOs at `backend/SupportDesk.Api/Application/Dto.cs:5-65`.
- TypeScript types at `frontend/src/types.ts:1-63`.
- New ticket form at `frontend/src/App.tsx:372-387`.

**Implementation plan:**

1. Add a `TicketChannel` enum in `TicketEnums.cs`, for example `Email`, `Chat`, `Phone`, `Web`.
2. Add a `Channel` property to `Ticket`.
3. Update the private `Ticket` constructor to accept channel.
4. Update `Ticket.Create` to accept channel and assign it.
5. Update `Snapshot()` to copy channel.
6. Add `TicketChannel Channel` to `CreateTicketRequest`.
7. Pass `request.Channel` from `CreateTicketHandler` into `Ticket.Create`.
8. Add `Channel` to `TicketSummaryDto` and `TicketDetailDto`.
9. Update `ToSummaryDto` and `ToDetailDto`.
10. Add `channel` to `TicketSummary` and `TicketDetail` in TypeScript.
11. Add a channel select to `NewTicketForm`.
12. Include channel in `createTicket` input and JSON.
13. Display channel in ticket rows or detail header.
14. Update seed data calls to `Ticket.Create`.

**Example TypeScript addition:**

```ts
export type TicketChannel = "Email" | "Chat" | "Phone" | "Web";
```

**Acceptance criteria:**

- [ ] New tickets require a channel.
- [ ] Seed tickets have channels.
- [ ] Ticket list/detail display channel.
- [ ] C# DTOs and TS types match.
- [ ] Existing endpoints still return valid JSON.

**Suggested checks:**

- Compare `TicketSummaryDto` and `TicketSummary` field-by-field.
- Create one ticket per channel.
- Search still works after constructor signature changes.

**Common mistakes:**

- Updating the backend DTO but forgetting `frontend/src/types.ts`.
- Forgetting `Snapshot()`.
- Adding the field only to detail but not summary.

## Story 3: Add Due-Soon SLA Filter

**User story:** As an agent, I want to filter tickets due within the next hour so I can prevent SLA breaches before they happen.

**Why this matters:** The app already tracks SLA due timestamps and breached state at `backend/SupportDesk.Api/Domain/Ticket.cs:62-70`. This story teaches query criteria and UI filters.

**Read first:**

- `TicketSearchCriteria` at `backend/SupportDesk.Api/Application/TicketSearchCriteria.cs:5-12`.
- Endpoint query params at `backend/SupportDesk.Api/Endpoints/TicketEndpoints.cs:14-28`.
- Store filter logic at `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:51-107`.
- UI filter state at `frontend/src/types.ts:57-63`.
- Filter controls at `frontend/src/App.tsx:200-235`.

**Implementation plan:**

1. Add `bool? DueSoon` to `TicketSearchCriteria`.
2. Add `bool? dueSoon = null` to the search endpoint parameters.
3. Pass `dueSoon` into the criteria constructor.
4. In `InMemoryTicketStore.SearchAsync`, define due soon as first response due or resolution due within one hour and not closed.
5. Use the existing `now` variable at `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:55`.
6. Add `dueSoon: boolean` to `TicketFilters` in TypeScript.
7. Initialize it in `emptyFilters` in `frontend/src/App.tsx:27-33`.
8. Serialize it in `listTickets` in `frontend/src/api.ts:44-53`.
9. Add a checkbox next to the existing breached checkbox at `frontend/src/App.tsx:227-234`.
10. Display due soon count if useful, but keep the first change small.

**Example query:**

```http
GET /api/tickets?dueSoon=true
```

**Acceptance criteria:**

- [ ] Filter returns tickets with first-response or resolution due within one hour.
- [ ] Closed tickets are not considered due soon.
- [ ] Filter can combine with status/tag/search.
- [ ] UI checkbox updates results.

**Suggested checks:**

- Use seed data timestamps from `backend/SupportDesk.Api/Infrastructure/SeedData.cs:18-57`.
- Temporarily adjust seed timestamps during local manual testing, then do not keep debug-only changes.
- Future test: search returns due soon and excludes closed.

**Common mistakes:**

- Using local time instead of UTC.
- Treating already breached tickets as due soon without deciding product behavior.
- Forgetting to update `TicketFilters`.

## Story 4: Add Ticket Watchers

**User story:** As a manager, I want to watch important tickets so I can follow updates even when I am not assigned.

**Why this matters:** Watchers teach resource relationships, read permissions, mutation endpoints, and UI state. It also prepares for notification features.

**Read first:**

- Ticket properties at `backend/SupportDesk.Api/Domain/Ticket.cs:50-70`.
- Tag replacement pattern at `backend/SupportDesk.Api/Domain/Ticket.cs:219-224`.
- Permission rules at `backend/SupportDesk.Api/Application/Permissions.cs:7-39`.
- DTO mapping at `backend/SupportDesk.Api/Application/Dto.cs:67-134`.
- Assignment UI at `frontend/src/App.tsx:334-344`.

**Implementation plan:**

1. Add a private `List<string> _watcherIds` to `Ticket`.
2. Add public `IReadOnlyList<string> WatcherIds => _watcherIds`.
3. Update constructor to accept watchers.
4. Update `Snapshot()` to copy watchers.
5. Add methods `AddWatcher(string userId, DateTimeOffset now)` and `RemoveWatcher(string userId, DateTimeOffset now)`.
6. Normalize watcher ids by trimming and ignoring blank values.
7. Update `Permissions.CanReadTicket` so watchers can read tickets.
8. Add watcher ids to `TicketDetailDto`; consider summary too if rows need it.
9. Add command handlers for watch/unwatch, using `ExpectedVersion`.
10. Add endpoints, for example `PUT /api/tickets/{id}/watchers/{userId}` and `DELETE /api/tickets/{id}/watchers/{userId}`.
11. Add frontend API functions.
12. Add a watchers section in properties panel.

**Illustrative fake code: not from this repo.**

```csharp
public void AddWatcher(string userId, DateTimeOffset now)
{
    var normalized = userId.Trim();
    if (!_watcherIds.Contains(normalized, StringComparer.OrdinalIgnoreCase))
    {
        _watcherIds.Add(normalized);
        Touch(now);
    }
}
```

**Acceptance criteria:**

- [ ] Manager/agent can add themselves as watcher when they can read the ticket.
- [ ] Watcher can continue reading the ticket.
- [ ] Duplicate watchers are not added.
- [ ] Watcher changes increment version.
- [ ] Watcher changes broadcast updated summary.

**Suggested checks:**

- Add watcher to a ticket, refresh detail, confirm watcher appears.
- Try adding duplicate watcher.
- Try unrelated user reading before/after watcher addition.

**Common mistakes:**

- Forgetting watcher list in `Snapshot()`.
- Letting anyone add anyone as watcher without permission decision.
- Adding watchers to UI only.

## Story 5: Add Ticket Escalation

**User story:** As an agent, I want to escalate a ticket to a manager so high-risk issues get attention quickly.

**Why this matters:** Escalation is a real support workflow. It teaches new domain state, permissions, internal notes, and eventing.

**Read first:**

- Status enum at `backend/SupportDesk.Api/Domain/TicketEnums.cs:3-11`.
- Ticket priority and status properties at `backend/SupportDesk.Api/Domain/Ticket.cs:56-59`.
- Internal note handling at `backend/SupportDesk.Api/Domain/Ticket.cs:183-217`.
- Update handler patterns at `backend/SupportDesk.Api/Application/Commands/ChangeTicketStatus.cs:22-40`.

**Implementation plan:**

1. Decide whether escalation is a boolean, timestamp, or separate enum. For a first implementation, add `DateTimeOffset? EscalatedAt` and `string? EscalatedBy`.
2. Add those properties to `Ticket`.
3. Update constructor and `Snapshot()`.
4. Add method `Escalate(string userId, DateTimeOffset now)`.
5. Decide invariant: closed tickets cannot be escalated, or can they? Put this in domain.
6. Add `EscalateTicketRequest` with `ExpectedVersion`.
7. Add `EscalateTicketHandler`.
8. Check `CanChangeStatus` or add `CanEscalate` in `Permissions`.
9. Optionally add an internal note automatically saying who escalated. If you do, call `AddComment` with `CommentVisibility.Internal`.
10. Add endpoint `PUT /api/tickets/{id}/escalation`.
11. Add DTO fields.
12. Add UI button in properties panel.
13. Display escalated state in header or row.

**Example domain decision:**

```text
Escalation is allowed for New, Open, Pending, and OnHold.
Escalation is not allowed for Solved or Closed.
```

**Acceptance criteria:**

- [ ] Escalation increments version.
- [ ] Escalation is visible in ticket detail.
- [ ] Escalation is blocked for closed tickets if that rule is chosen.
- [ ] Unauthorized users cannot escalate.
- [ ] Live updates fire.

**Suggested checks:**

- Escalate one seed ticket.
- Try escalating a solved/closed ticket after status change.
- Verify internal note visibility if auto-note is added.

**Common mistakes:**

- Treating escalation as only a tag.
- Forgetting read/write permissions.
- Creating an internal note that customers can see.

## Story 6: Add Saved Inbox Views

**User story:** As an agent, I want saved views like "My Open Tickets" and "VIP Breaches" so I can switch queues quickly.

**Why this matters:** Views teach frontend state composition without needing backend persistence at first.

**Read first:**

- Current filter type at `frontend/src/types.ts:57-63`.
- Empty filters at `frontend/src/App.tsx:27-33`.
- Filter controls at `frontend/src/App.tsx:200-235`.
- API query serialization at `frontend/src/api.ts:44-53`.

**Implementation plan:**

1. Create a `SavedView` type in `frontend/src/types.ts`.
2. Define an array of saved views near `emptyFilters`.
3. Examples:
   - "All open": `{ status: "Open" }`
   - "SLA risk": `{ breached: true }`
   - "VIP": `{ tag: "vip" }`
4. Add buttons or a select above filters.
5. When a user picks a view, call `setFilters` with that view's filters.
6. Make sure unspecified values reset to defaults.
7. Keep this frontend-only in the first version.
8. Later extension: backend-persisted views per user.

**Illustrative fake code: not from this repo.**

```ts
const savedViews = [
  { name: "VIP breaches", filters: { ...emptyFilters, tag: "vip", breached: true } },
];
```

**Acceptance criteria:**

- [ ] User can apply at least three saved views.
- [ ] Applying a view updates the ticket list.
- [ ] Manual filters still work after applying a view.
- [ ] No API changes required.

**Suggested checks:**

- Apply "VIP", then clear tag manually.
- Apply "SLA risk", then create or inspect tickets.

**Common mistakes:**

- Merging view filters into old filters accidentally.
- Duplicating query serialization logic outside `frontend/src/api.ts`.

## Story 7: Add Assignment To Me Button

**User story:** As an agent, I want a one-click "Assign to me" button so I can claim a ticket quickly.

**Why this matters:** This is a UI feature that reuses an existing API, making it a good junior-to-mid exercise.

**Read first:**

- Demo user headers at `frontend/src/api.ts:19-24`.
- Assignment API at `frontend/src/api.ts:86-96`.
- Assignment function in UI at `frontend/src/App.tsx:125-135`.
- Assignment properties panel at `frontend/src/App.tsx:334-344`.

**Implementation plan:**

1. Export the demo user id from `frontend/src/api.ts`, or define a local constant in `App.tsx` for the learning version.
2. Add a button near assignment controls labeled "Assign to me".
3. On click, call `assignTicket(selected.id, "agent-1", teamDraft || selected.teamId || "", selected.version)`.
4. Prefer using the same `saveAssignment` flow by setting `assigneeDraft` first, but beware React state updates are async.
5. The simplest path is to call `assignTicket` directly in a new function.
6. Update `selected`, `tickets`, `assigneeDraft`, and `teamDraft` after success.

**Example behavior:**

```text
Before: Assignee is empty.
Click "Assign to me".
After: Assignee input and detail state show agent-1.
```

**Acceptance criteria:**

- [ ] Button appears when a ticket is selected.
- [ ] Click assigns ticket to `agent-1`.
- [ ] Version conflict is handled by existing error banner.
- [ ] Ticket row updates.

**Suggested checks:**

- Click button twice.
- Click after changing ticket in another tab.

**Common mistakes:**

- Assuming `setAssigneeDraft("agent-1")` updates before calling `saveAssignment`.
- Forgetting `expectedVersion`.

## Story 8: Add Comment Draft Preservation

**User story:** As an agent, I want my unsent comment draft preserved when a conflict happens so I do not lose work.

**Why this matters:** The current UI clears the comment body only after success at `frontend/src/App.tsx:115-119`, but conflicts need a deliberate UX.

**Read first:**

- Submit comment flow at `frontend/src/App.tsx:111-123`.
- Error parsing at `frontend/src/api.ts:36-39`.
- Store conflict at `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:39-42`.

**Implementation plan:**

1. Confirm current behavior: `setCommentBody("")` happens only after `addComment` succeeds.
2. Add a specific conflict message if the error contains "version".
3. Add a "Refresh ticket" button near the error banner when conflict occurs.
4. Keep `commentBody` unchanged during refresh.
5. In the refresh function, call `openTicket(selected.id)`.
6. After refresh, let user click Send again.
7. Optional: show current `selected.version` in the composer for learning.

**Acceptance criteria:**

- [ ] Stale comment conflict does not clear draft text.
- [ ] User can refresh ticket detail.
- [ ] After refresh, user can submit the preserved draft.
- [ ] Generic errors still display normally.

**Suggested checks:**

- Two-tab stale comment scenario.
- Network failure simulation if possible.

**Common mistakes:**

- Clearing draft in `catch`.
- Refreshing and accidentally resetting selected ticket state without preserving draft.

## Story 9: Add Ticket Activity Timeline

**User story:** As an agent, I want to see assignment, status, tag, and comment events in a timeline so I understand what happened on a ticket.

**Why this matters:** Timeline teaches event modeling. The current code stores comments only, not workflow events.

**Read first:**

- Comment model at `backend/SupportDesk.Api/Domain/TicketComment.cs:3-26`.
- Ticket mutation methods at `backend/SupportDesk.Api/Domain/Ticket.cs:158-224`.
- Detail DTO at `backend/SupportDesk.Api/Application/Dto.cs:27-48`.
- Conversation UI at `frontend/src/App.tsx:287-324`.

**Implementation plan:**

1. Create a domain type like `TicketActivity`.
2. Include fields: `Id`, `Kind`, `ActorId`, `ActorName`, `Message`, `CreatedAt`.
3. Add private `_activities` list to `Ticket`.
4. Add `Activities` read-only property.
5. Update constructor and `Snapshot()`.
6. Add an activity in `Assign`, `ChangeStatus`, `ReplaceTags`, and `AddComment`.
7. Add `ActivityDto`.
8. Add activities to `TicketDetailDto`.
9. Add TS type.
10. Render timeline either above comments or in properties panel.
11. Decide whether internal note activity is visible to customers.

**Illustrative fake activity kinds:**

```text
CommentAdded
InternalNoteAdded
Assigned
StatusChanged
TagsChanged
Escalated
```

**Acceptance criteria:**

- [ ] Assignment creates activity.
- [ ] Status change creates activity.
- [ ] Comment creates activity.
- [ ] Detail view displays timeline in chronological order.
- [ ] Visibility is safe for internal notes.

**Suggested checks:**

- Seed data should generate activities if you update seed mutation calls.
- Verify customer visibility after internal note.

**Common mistakes:**

- Making activities mutable.
- Forgetting snapshot copy.
- Leaking internal note body through activity message.

## Story 10: Add Customer Satisfaction Rating

**User story:** As a customer, I want to rate a solved ticket so the support team can measure service quality.

**Why this matters:** CSAT is a real support metric. It teaches status-dependent validation and customer-only actions.

**Read first:**

- Status transitions at `backend/SupportDesk.Api/Domain/Ticket.cs:171-180`.
- Permissions at `backend/SupportDesk.Api/Application/Permissions.cs:7-39`.
- Detail DTO at `backend/SupportDesk.Api/Application/Dto.cs:27-48`.
- Customer identity rule at `backend/SupportDesk.Api/Application/Permissions.cs:7-11`.

**Implementation plan:**

1. Add properties to `Ticket`: `int? SatisfactionRating`, `string? SatisfactionComment`, `DateTimeOffset? SatisfactionRatedAt`.
2. Add method `RateSatisfaction(string customerId, int rating, string? comment, DateTimeOffset now)`.
3. Validate rating is 1 through 5.
4. Validate status is `Solved` or `Closed`.
5. Add command request with `Rating`, `Comment`, `ExpectedVersion`.
6. Add permission method `CanRateSatisfaction`.
7. Only allow the customer matching `CustomerEmail`, or admin for testing if desired.
8. Add endpoint `PUT /api/tickets/{id}/satisfaction`.
9. Add DTO fields.
10. Add UI section shown when ticket is solved/closed.

**Example request:**

```json
{
  "rating": 5,
  "comment": "Fast and clear.",
  "expectedVersion": 7
}
```

**Acceptance criteria:**

- [ ] Rating must be 1-5.
- [ ] Rating only allowed for solved/closed tickets.
- [ ] Unauthorized agents cannot rate as customer.
- [ ] Rating appears in detail.
- [ ] Version increments.

**Suggested checks:**

- Try rating an Open ticket.
- Try rating with 0 and 6.
- Try rating as unrelated user.

**Common mistakes:**

- Trusting UI to hide rating controls.
- Forgetting status validation in domain.

## Story 11: Add Ticket Merge

**User story:** As an agent, I want to merge duplicate tickets so customer conversations are not split across multiple threads.

**Why this matters:** Merge is a complex support workflow. It teaches cross-aggregate operations and why not every rule belongs inside a single aggregate.

**Read first:**

- Store `GetAsync` and `UpdateAsync` at `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:22-49`.
- Ticket comments at `backend/SupportDesk.Api/Domain/Ticket.cs:183-217`.
- Status change at `backend/SupportDesk.Api/Domain/Ticket.cs:171-180`.

**Implementation plan:**

1. Define merge behavior carefully:
   - Source ticket is closed.
   - Target ticket receives a public or internal note.
   - Comments are not physically copied in first version.
2. Add request: `TargetTicketId`, `ExpectedSourceVersion`, `ExpectedTargetVersion`.
3. Store currently updates one ticket at a time. Decide whether to extend store with a transaction-like method or accept first-version limitation.
4. Preferred learning design: add `MergeTicketsAsync` to `ITicketStore` so both tickets update under one lock in `InMemoryTicketStore`.
5. Add handler `MergeTicketHandler`.
6. Check user can read and change both tickets.
7. In domain, add a method such as `MarkMergedInto(targetTicketNumber, now)`.
8. Add route `POST /api/tickets/{id}/merge`.
9. Add UI action in properties panel.
10. Broadcast updates for both tickets.

**Acceptance criteria:**

- [ ] Source ticket records merged target.
- [ ] Source ticket is closed or solved based on product decision.
- [ ] Target gets an internal note.
- [ ] Both versions are checked.
- [ ] Both updates happen atomically in in-memory store.

**Suggested checks:**

- Merge two tickets.
- Try merging with stale target.
- Try merging without permission to target.

**Common mistakes:**

- Updating source then failing target, leaving inconsistent state.
- Forgetting to notify clients about both tickets.

## Story 12: Add Attachments Metadata

**User story:** As a customer, I want to attach screenshots to a ticket so agents can diagnose issues faster.

**Why this matters:** Full file upload is risky. Start with attachment metadata only to practice safe incremental design.

**Read first:**

- Comment model at `backend/SupportDesk.Api/Domain/TicketComment.cs:3-26`.
- Add comment flow at `backend/SupportDesk.Api/Application/Commands/AddComment.cs:23-45`.
- Composer UI at `frontend/src/App.tsx:301-324`.

**Implementation plan:**

1. Do not implement binary upload in first version.
2. Create `AttachmentMetadata` with `FileName`, `ContentType`, `SizeBytes`, `Url`.
3. Decide whether attachments belong to comments or tickets. First version: comment attachments.
4. Add `IReadOnlyList<AttachmentMetadata>` to `TicketComment`.
5. Update `AddCommentRequest` with attachments list.
6. Validate:
   - file name not blank
   - size positive
   - allowed content types
   - URL optional or trusted depending on design
7. Add DTO and TS types.
8. Add UI inputs for fake metadata.
9. Display attachments under comments.

**Acceptance criteria:**

- [ ] Comment can include zero or more attachment metadata records.
- [ ] Invalid metadata is rejected.
- [ ] Attachments render under comment.
- [ ] No binary file upload is added yet.

**Suggested checks:**

- Add comment with one PNG metadata item.
- Try blank filename.
- Try very large size.

**Common mistakes:**

- Implementing real upload without security design.
- Rendering untrusted URL unsafely.

## Story 13: Add Agent Presence Indicator

**User story:** As an agent, I want to see when another agent is viewing the same ticket so we avoid duplicate work.

**Why this matters:** Presence teaches SignalR groups, connection lifecycle, and ephemeral state.

**Read first:**

- Hub group join at `backend/SupportDesk.Api/Realtime/TicketHub.cs:5-8`.
- Client SignalR connection at `frontend/src/api.ts:124-137`.
- Selected ticket effect at `frontend/src/App.tsx:58-69`.

**Implementation plan:**

1. Add hub methods `JoinTicket(ticketId)` and `LeaveTicket(ticketId)`.
2. Use groups named like `ticket:{ticketId}`.
3. Track presence in memory inside a singleton presence service.
4. Because current auth is demo headers, decide how hub gets user identity. SignalR headers are sent at `frontend/src/api.ts:126`.
5. On ticket selection, client invokes `JoinTicket`.
6. On selection change or cleanup, client invokes `LeaveTicket`.
7. Broadcast presence list to the ticket group.
8. Add UI display near ticket header.

**Acceptance criteria:**

- [ ] Opening a ticket joins presence.
- [ ] Leaving or switching ticket removes presence.
- [ ] Presence updates live for other clients.
- [ ] Presence state is not persisted.

**Suggested checks:**

- Two browser windows with different demo headers.
- Switch selected tickets and watch presence update.

**Common mistakes:**

- Persisting presence in ticket domain.
- Forgetting cleanup on disconnect.
- Trusting unverified identity in production.

## Story 14: Add Bulk Tagging

**User story:** As a manager, I want to apply a tag to multiple tickets so I can organize campaigns or incidents quickly.

**Why this matters:** Bulk actions teach batch commands, partial failure decisions, permissions, and concurrency.

**Read first:**

- Tag mutation at `backend/SupportDesk.Api/Domain/Ticket.cs:219-224`.
- Replace tags handler at `backend/SupportDesk.Api/Application/Commands/ReplaceTicketTags.cs:20-38`.
- Store update method at `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:28-49`.
- Ticket list rendering at `frontend/src/App.tsx:244-265`.

**Implementation plan:**

1. Add selection checkboxes to ticket rows.
2. Track selected ticket ids in React state.
3. Add a bulk tag input and "Apply tag" button.
4. Backend design decision:
   - Simple version: frontend loops existing tag endpoint per ticket.
   - Better version: add `POST /api/tickets/bulk-tags`.
5. For a junior implementation, start with frontend loop using existing `replaceTicketTags`.
6. Fetch each selected ticket detail first if current row summary lacks full tags/version. Better: use current summary `tags` and `version`.
7. For each ticket, append tag if missing and call replace.
8. Handle partial failures by showing count succeeded/failed.
9. Later backend version can update atomically or report per-ticket results.

**Acceptance criteria:**

- [ ] User can select multiple visible tickets.
- [ ] User can apply one tag to all selected tickets.
- [ ] Duplicate tag is not added.
- [ ] Partial failures are visible.
- [ ] Existing per-ticket permissions still apply.

**Suggested checks:**

- Select two tickets and add `incident-123`.
- Try adding existing tag.
- Trigger stale version on one ticket.

**Common mistakes:**

- Replacing all tags with only the new tag.
- Hiding partial failures.
- Assuming summaries always have fresh versions.

## Story 15: Add Ticket Export To JSON

**User story:** As a manager, I want to export a ticket as JSON so I can share a sanitized support record for audits.

**Why this matters:** Export teaches read permissions, data minimization, and contract design.

**Read first:**

- Detail query at `backend/SupportDesk.Api/Application/Queries/GetTicket.cs:14-24`.
- Detail mapping at `backend/SupportDesk.Api/Application/Dto.cs:92-120`.
- Internal note permission at `backend/SupportDesk.Api/Application/Permissions.cs:36-38`.

**Implementation plan:**

1. Decide whether export uses existing detail DTO or a new export DTO.
2. Prefer new `TicketExportDto` so export contract is explicit.
3. Add query handler or method to get export.
4. Reuse `Permissions.CanReadTicket`.
5. Decide whether internal notes are included:
   - Managers/admins yes.
   - Customers no.
6. Add endpoint `GET /api/tickets/{id}/export`.
7. In frontend, add "Export JSON" button.
8. Client can call endpoint and create a downloadable blob.
9. Include filename like `T-001001-export.json`.

**Acceptance criteria:**

- [ ] Export respects read permissions.
- [ ] Customer export excludes internal notes.
- [ ] JSON includes ticket number, subject, comments, status, SLA.
- [ ] UI downloads JSON file.

**Suggested checks:**

- Export as agent and customer.
- Inspect JSON for internal note leakage.

**Common mistakes:**

- Returning raw domain object.
- Reusing detail DTO without thinking about export-specific fields.

## Story 16: Add Ticket Reopen

**User story:** As an agent, I want to reopen a solved ticket when a customer replies so the conversation can continue.

**Why this matters:** Current domain forbids reopening closed tickets at `backend/SupportDesk.Api/Domain/Ticket.cs:171-176`. Reopen forces a careful workflow decision.

**Read first:**

- Status rules at `backend/SupportDesk.Api/Domain/Ticket.cs:171-180`.
- Add public comment behavior at `backend/SupportDesk.Api/Domain/Ticket.cs:210-213`.
- Status handler at `backend/SupportDesk.Api/Application/Commands/ChangeTicketStatus.cs:22-40`.

**Implementation plan:**

1. Define product rule:
   - Solved can reopen to Open.
   - Closed cannot reopen.
2. Update `ChangeStatus` domain logic to allow `Solved -> Open`.
3. Keep `Closed` protected.
4. Consider clearing `ClosedAt` when reopened. Current code already sets `ClosedAt = null` for non-solved/closed at `backend/SupportDesk.Api/Domain/Ticket.cs:178-179`.
5. Add tests for Solved -> Open and Closed -> Open.
6. Decide whether customer public comment automatically reopens Solved tickets. If yes, update `AddComment`.
7. UI already has status select, so backend change may be enough.

**Acceptance criteria:**

- [ ] Solved ticket can become Open.
- [ ] Closed ticket still cannot reopen.
- [ ] Reopen increments version.
- [ ] Reopen broadcasts update.

**Suggested checks:**

- Change Open -> Solved -> Open.
- Change Open -> Closed -> Open and confirm rejection.

**Common mistakes:**

- Removing all closed-state protection.
- Forgetting to test customer reply behavior.

## Story 17: Add Agent Collision Warning

**User story:** As an agent, I want a warning if the ticket changed since I opened it so I do not overwrite another agent's work.

**Why this matters:** The backend already rejects stale writes. This story improves the frontend before the user submits.

**Read first:**

- Live update effect at `frontend/src/App.tsx:58-69`.
- Upsert function at `frontend/src/App.tsx:408-415`.
- Version display at `frontend/src/App.tsx:359-362`.
- Store versioning at `backend/SupportDesk.Api/Domain/Ticket.cs:232-236`.

**Implementation plan:**

1. Add state `remoteVersionWarning`.
2. In SignalR callback, when summary id matches selected id and summary version is greater than selected.version, set warning.
3. Do not automatically overwrite comment draft.
4. Show banner: "This ticket changed in another session. Refresh before saving."
5. Add refresh button that calls `openTicket(selected.id)`.
6. Clear warning after successful refresh.
7. Make sure regular user-initiated mutations update selected and clear warning.

**Acceptance criteria:**

- [ ] Warning appears when selected ticket receives newer live summary.
- [ ] Draft comment is preserved.
- [ ] Refresh updates selected ticket.
- [ ] Warning clears after refresh.

**Suggested checks:**

- Two-tab update.
- Add comment in tab A while tab B has draft text.

**Common mistakes:**

- Auto-refreshing and deleting draft.
- Comparing dates instead of version.

## Story 18: Add Manager Dashboard Metrics

**User story:** As a manager, I want dashboard metrics for open, urgent, breached, and unassigned tickets so I can spot queue health quickly.

**Why this matters:** Metrics can start client-side, but later should move server-side for scale.

**Read first:**

- Ticket summary type at `frontend/src/types.ts:5-26`.
- Ticket list state at `frontend/src/App.tsx:36-38`.
- SLA count at `frontend/src/App.tsx:77-80`.

**Implementation plan:**

1. Add a derived `metrics` object with `useMemo`.
2. Count:
   - open-ish statuses: New/Open/Pending/OnHold
   - urgent priority
   - breached SLA
   - no assignee
3. Render metric tiles near `queue-meta` at `frontend/src/App.tsx:237-240`.
4. Style with compact CSS.
5. Keep metrics based on currently visible tickets; label it clearly.
6. Later extension: API metrics endpoint for all visible tickets, not just current page.

**Acceptance criteria:**

- [ ] Dashboard shows four metrics.
- [ ] Metrics update when filters change.
- [ ] Metrics update after live ticket events.
- [ ] Label says "visible tickets" or equivalent.

**Suggested checks:**

- Apply status filter and confirm counts change.
- Create urgent ticket and confirm urgent count updates.

**Common mistakes:**

- Presenting visible-page metrics as global queue metrics.
- Recomputing manually in multiple places instead of `useMemo`.

## Story 19: Add Server-Side Ticket Metrics Endpoint

**User story:** As a manager, I want queue metrics from the server so counts are correct even when the UI is paginated or filtered.

**Why this matters:** This is the backend counterpart to Story 18. It teaches query endpoints, DTOs, permissions, and aggregate counts.

**Read first:**

- Search query handler at `backend/SupportDesk.Api/Application/Queries/SearchTickets.cs:14-28`.
- Store search at `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:51-107`.
- Permission read rule at `backend/SupportDesk.Api/Application/Permissions.cs:7-11`.
- Endpoint patterns at `backend/SupportDesk.Api/Endpoints/TicketEndpoints.cs:14-29`.

**Implementation plan:**

1. Add `TicketMetricsDto` with counts: `TotalVisible`, `Open`, `Urgent`, `Breached`, `Unassigned`.
2. Add query handler `GetTicketMetricsHandler`.
3. Decide whether metrics accept same filters as search. Start with same `TicketSearchCriteria`.
4. Call store search with page size 100 for current in-memory version, or add a store method that returns all matching tickets.
5. Apply `Permissions.CanReadTicket`.
6. Count visible tickets.
7. Register handler in `Program.cs`.
8. Add endpoint `GET /api/tickets/metrics`.
9. Add frontend API function.
10. Replace or compare client-side metrics.

**Acceptance criteria:**

- [ ] Metrics respect read permissions.
- [ ] Metrics can use search filters.
- [ ] Endpoint returns stable DTO.
- [ ] UI displays server metrics.

**Suggested checks:**

- Call endpoint with `tag=vip`.
- Change demo headers and compare counts.

**Common mistakes:**

- Counting unauthorized tickets.
- Reusing paged result and counting only first page without naming that limitation.

## Story 20: Add Audit-Friendly Public Transcript View

**User story:** As a customer success manager, I want a public transcript view that excludes internal notes so I can share a clean conversation history externally.

**Why this matters:** This teaches data minimization and explicit read models. It is related to export but can be shown in-app.

**Read first:**

- Detail DTO filtering at `backend/SupportDesk.Api/Application/Dto.cs:92-120`.
- Comment visibility enum at `backend/SupportDesk.Api/Domain/TicketEnums.cs:21-25`.
- Conversation render at `frontend/src/App.tsx:287-324`.

**Implementation plan:**

1. Add endpoint `GET /api/tickets/{id}/public-transcript`.
2. Add `PublicTranscriptDto` with ticket number, subject, customer name, public comments only.
3. In handler, check `CanReadTicket`.
4. Filter comments to `CommentVisibility.Public` regardless of staff role.
5. Add frontend API function.
6. Add "Public transcript" button in detail header.
7. Render transcript in a modal-like section or a separate panel.
8. Include copy/download as plain text if desired.
9. Do not include internal notes, internal activities, assignee ids, or private tags unless explicitly decided.

**Example transcript fields:**

```json
{
  "number": "T-001002",
  "subject": "Cannot log in after SSO migration",
  "publicComments": []
}
```

**Acceptance criteria:**

- [ ] Transcript includes public comments only.
- [ ] Internal notes are excluded even for managers.
- [ ] Unauthorized users cannot access transcript.
- [ ] UI clearly labels it as public-safe.

**Suggested checks:**

- Add internal note and public reply to same ticket.
- Confirm transcript includes only public reply.
- Try customer and manager identities.

**Common mistakes:**

- Reusing full detail DTO and relying on UI to hide internal notes.
- Including internal tags or assignee fields by accident.

## Suggested Order For A Junior Engineer

1. Story 7: Add Assignment To Me Button
2. Story 6: Add Saved Inbox Views
3. Story 1: Change Ticket Priority
4. Story 3: Add Due-Soon SLA Filter
5. Story 8: Add Comment Draft Preservation
6. Story 18: Add Manager Dashboard Metrics
7. Story 2: Add Ticket Channels
8. Story 20: Add Audit-Friendly Public Transcript View

After that, move into stories that require deeper design: watchers, escalation, metrics endpoint, merge, presence, and attachments.
