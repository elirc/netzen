# 3. Work through safe mutation and invalidation

The key domain repair is in [`Ticket.cs`](../support-ticketing-app/backend/SupportDesk.Api/Domain/Ticket.cs). `ReplaceTags` fully enumerates and normalizes its input before touching `_tags`:

```csharp
public void ReplaceTags(IEnumerable<string> tags, DateTimeOffset now)
{
    if (tags is null) throw new DomainException("Tags are required.");
    var normalized = tags.Select(tag => NormalizeTag(tag ?? string.Empty))
        .Where(tag => tag.Length > 0)
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .Order()
        .ToArray();
    _tags.Clear();
    _tags.AddRange(normalized);
    Touch(now);
}
```

The materialization point is the safety boundary. If an enumerable yields `changed` and then throws, `_tags.Clear()` has not run, so the original tags and version remain intact. Once the array exists, clearing and adding cannot fail because of that input enumeration. `NormalizeTag` trims and lowercases; `Distinct` ignores case; `Order` stabilizes the result. The version advances once only after the replacement succeeds.

The other domain guards are similar. `ChangeStatus` rejects an undefined `TicketStatus` and rejects every change from `Closed`. `UpdatePriority` rejects an undefined `TicketPriority`. `AddComment` rejects an undefined `CommentVisibility` and blank body. `Ticket.Create` gained the same priority guard in the 2026-09-17 fix pass (`Ticket.cs:101`, see the follow-up below); before that it silently fell through to the SLA switches' fallback arm.

The notifier in [`SignalRTicketNotifier.cs`](../support-ticketing-app/backend/SupportDesk.Api/Realtime/SignalRTicketNotifier.cs) deliberately sends no payload:

```csharp
public Task TicketUpdatedAsync(TicketSummaryDto ticket, CancellationToken cancellationToken) =>
    _hub.Clients.Group("inbox").SendAsync("tickets.changed", cancellationToken);
```

The argument `ticket` is used only to satisfy the application interface; it is not serialized. This prevents a broad hub group from receiving customer email, comment text, or an internal note. The browser's [`api.ts`](../support-ticketing-app/frontend/src/api.ts) registers `tickets.changed`, and [`App.tsx`](../support-ticketing-app/frontend/src/App.tsx) responds by calling `refreshTickets()` and, for `selectedId`, `getTicket(selectedId)`. Server filtering and detail visibility are applied again.

The version check completes the mutation story. `replaceTicketTags`, `changeTicketStatus`, and `addComment` all send the version read in the selected detail. The store compares that value under `_writeLock`; a stale editor receives a concurrency error instead of silently overwriting a newer comment or tag set. The successful handler then maps the committed snapshot and notifies the inbox. This is optimistic concurrency for one in-memory process. It is not a durable transaction across a database and SignalR, so a future production design would need an explicit delivery strategy rather than assuming the hub call is an outbox.

**Exercise 3.** Given tags `{" Billing ", "billing", "VIP"}`, state the stored order and version change. Then describe what remains unchanged when enumeration throws after its first item, and list the fields carried by the SignalR event.

## Follow-up: the half-applied enum guard at creation

The delivered change added `Enum.IsDefined` to `Ticket.ChangeStatus`, `Ticket.UpdatePriority` and `Ticket.AddComment` — but not to `Ticket.Create`. `Ticket.Create(..., (TicketPriority)99, ...)` was therefore accepted, and the two SLA `switch` expressions fell through to their `_ =>` arm, so the ticket got the *Low* due dates while reporting priority 99. A half-applied invariant is worse than none: it teaches that guards are optional, and it puts a value in the store that every later `UpdatePriority` call would reject.

`Ticket.Create` now runs the same check as `UpdatePriority`, after the required-field guards and before the SLA switches:

```csharp
if (!Enum.IsDefined(priority))
{
    throw new DomainException("Ticket priority is not supported.");
}
```

`TicketSafetyTests.Create_RejectsUndefinedPriority` pins it and asserts the exact message. The lesson generalises: when you add a guard to a mutator, grep for every other entry point that can set the same field. Constructors and factory methods are the ones people forget, because "the callers always pass a real enum" stops being true the moment JSON deserialisation is in the path.

## Follow-up: one hub connection per mount

The delivered hub `useEffect` listed `[selectedId, filters]` as dependencies, and its cleanup stops the SignalR connection — so selecting a ticket or typing in the search box tore down and reopened the WebSocket. `App.tsx` now creates the connection once (`[]`) and reads the changing values through `filtersRef` / `selectedIdRef`; `refreshTickets` reads `filtersRef.current` for the same reason. Chapter 07 explains why a long-lived callback must read state through a ref rather than out of its closure.
