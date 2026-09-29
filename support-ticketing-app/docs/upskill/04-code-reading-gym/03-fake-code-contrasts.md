# 03 Fake Code Contrasts

All code below is illustrative fake code: not from this repo.

## Contrast 1: Entity Leak

Bad:

```csharp
// Illustrative fake code: not from this repo.
return Results.Ok(ticket);
```

Better: map through DTOs like `backend/SupportDesk.Api/Application/Dto.cs:67-134`.

Risk: domain fields become public contract and internal notes can leak.

## Contrast 2: Missing Permission

Bad:

```csharp
// Illustrative fake code: not from this repo.
ticket.ChangeStatus(request.Status, now);
```

Better: check `Permissions.CanChangeStatus` as in `backend/SupportDesk.Api/Application/Commands/ChangeTicketStatus.cs:24-35`.

Risk: IDOR and unauthorized workflow changes.

## Contrast 3: No Concurrency Check

Bad:

```csharp
// Illustrative fake code: not from this repo.
mutate(current);
```

Better: compare `expectedVersion` before mutation at `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:39-42`.

Risk: lost updates.

## Contrast 4: UI-Only Validation

Bad:

```csharp
// Illustrative fake code: not from this repo.
// Trust required attributes from browser form.
```

Better: keep domain checks at `backend/SupportDesk.Api/Domain/Ticket.cs:83-96`.

Risk: API clients bypass browser validation.

## Contrast 5: Side Effect Before Commit

Bad:

```csharp
// Illustrative fake code: not from this repo.
await notifier.TicketCommentedAsync(summary, comment, ct);
await store.UpdateAsync(...);
```

Better: current handlers notify after store update, for example `backend/SupportDesk.Api/Application/Commands/AddComment.cs:27-45`.

Risk: clients see events for changes that never committed.

## Contrast 6: Swallowed Errors

Bad:

```ts
// Illustrative fake code: not from this repo.
catch { setError("Something went wrong"); }
```

Better: client extracts problem detail at `frontend/src/api.ts:36-39` and UI shows it at `frontend/src/App.tsx:268-270`.

Risk: debugging loses root cause.

## Contrast 7: Hardcoded Enum Drift

Bad:

```ts
// Illustrative fake code: not from this repo.
const statuses = ["open", "closed"];
```

Better: match server enum names from `backend/SupportDesk.Api/Domain/TicketEnums.cs:3-11` and client union at `frontend/src/types.ts:1`.

Risk: API binding fails or UI sends invalid values.

## Contrast 8: Search Without Bounds

Bad:

```csharp
// Illustrative fake code: not from this repo.
return tickets.Where(Matches).ToList();
```

Better: current store clamps page size at `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:101-104`.

Risk: memory and latency spikes.

## Contrast 9: Effect Without Cleanup

Bad:

```tsx
// Illustrative fake code: not from this repo.
useEffect(() => { connectTicketHub(onSummary); }, []);
```

Better: cleanup stops the connection at `frontend/src/App.tsx:66-68`.

Risk: duplicate subscriptions and memory leaks.

## Contrast 10: Internal Notes In Browser Filter Only

Bad:

```ts
// Illustrative fake code: not from this repo.
comments.filter(c => c.visibility !== "Internal")
```

Better: server filters detail DTOs at `backend/SupportDesk.Api/Application/Dto.cs:92-120`.

Risk: sensitive data already crossed the network.
