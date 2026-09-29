# 7. Trace lab: visibility, writes, and reconnect

Trace the backend test fixture's customer search. The store contains ticket `T-000001` for `customer@example.com`, followed by tickets `T-000002` through `T-000121` belonging to other customers. The first ticket has an internal comment body `INTERNAL-ONLY-MARKER`. A customer search with no query enters `SearchTicketsHandler` with page 1 and size 50. The handler requests source page 1 at size 100, maps only tickets accepted by `Permissions.CanReadTicket`, then requests source page 2 because the source total is 121. The customer-visible list contains one item, so final `Skip(0).Take(50)` returns `T-000001` and total 1. A query for the marker is filtered in `InMemoryTicketStore` before permission projection because only public comments participate in `Contains`; the final items and total are zero.

Trace an internal comment mutation from an agent. The API receives `AddCommentRequest(Body, Visibility = Internal, ExpectedVersion = 3)`. `AddCommentHandler` calls `CanComment`; the agent must both have a staff role and be able to read the ticket. `InMemoryTicketStore.UpdateAsync` takes `_writeLock`, confirms version 3, creates a working snapshot, and invokes `Ticket.AddComment`. The comment is appended, `Touch` changes `UpdatedAt` and version 4, and the store saves a snapshot. The notifier sends `tickets.changed` to group `inbox` with no payload. On the next detail request, `Dto.ToDetailDto` includes the internal comment only if `CanReadInternalNotes` passes.

Trace a competing write. Two requests both carry expected version 3. The lock serializes the critical section. The first stores version 4. The second reads current version 4, fails the equality check, and throws `ConcurrencyException`; its working copy is never mutated and no successful update notification should be emitted by the handler.

Trace the browser response. `connectTicketHub` registers `tickets.changed`; an event calls `refreshTickets`, and if `selectedId` exists it increments `detailRequest` before calling `getTicket(selectedId)`. If filters change while the request is pending, the connection is left alone — the effect now has an empty dependency list — and the callback picks up the new filters through `filtersRef.current` the next time it fires. The `[filters]` effect is what refetches the list. If a detail response arrives after a newer detail request, the sequence check suppresses it. If the connection reconnects, SignalR invokes `JoinInbox`, then calls the changed callback so the list and selected detail are refreshed. Draft state remains in its separate React variables.

A selected ticket can disappear after a refetch because permissions or filters changed. The current source leaves the existing selection logic visible in the effect that opens the first item when no detail is selected; it does not establish a durable selection policy for every disappearance. Treat "clear selection and explain why" as future product work, not as delivered behavior. Likewise, a reconnect callback proves that this client attempts to rejoin and refreshes after `JoinInbox`; it does not prove that a notification emitted during downtime was durably replayed.

## Why the hub connection is created once (and reads state through refs)

Look again at the hub effect in `App.tsx`. It used to declare `[selectedId, filters]` as its dependencies. That is what the exhaustive-deps lint rule asks for, and here it was wrong in a way worth understanding: an effect re-runs when a dependency changes, and this effect's cleanup *stops the SignalR connection*. So every keystroke in the search box tore down a WebSocket and opened a new one, re-ran `JoinInbox`, and left a window in which a `tickets.changed` event reached nobody. The lint rule protects you from stale closures; it does not know that this particular closure owns an expensive long-lived resource.

The fix is to separate the two concerns. The **resource** is created once: `useEffect(..., [])`, one connection per mount, stopped on unmount. The **state the callback needs** is read through refs that are kept current on every render:

```tsx
const filtersRef = useRef(filters);
filtersRef.current = filters;          // updated during render; refs do not trigger re-renders
const selectedIdRef = useRef(selectedId);
selectedIdRef.current = selectedId;
```

and the callback reads `selectedIdRef.current` rather than `selectedId`. `refreshTickets` was changed the same way, to read `filtersRef.current` instead of closing over `filters`, because the hub callback holds the copy of that function created on the first render.

The general rule: **a callback that outlives the render that created it must read changing values out of a ref, not out of its closure.** It applies to SignalR and WebSocket handlers, `setInterval`/`setTimeout` bodies, event listeners registered on `window`, and subscription callbacks from any client library. The tell-tale symptom of getting it wrong in the other direction — listing the values as dependencies — is a connection, timer or listener that is destroyed and recreated far more often than the thing it represents actually changes. The tell-tale symptom of getting it wrong in this direction — closing over state with `[]` and no ref — is a handler that forever acts on the values from the moment the component mounted.

Note what refs do *not* fix: a ref read is not reactive, so nothing re-renders when `filtersRef.current` changes. That is exactly what you want inside a callback and exactly what you must not use to compute JSX.

**Exercise 7.** Draw the event sequence for an internal note while an agent has typed an unsent `commentBody`: mutation, data-free event, list refetch, detail refetch, and draft preservation. Add the competing version-3 write and mark the point where it fails.
