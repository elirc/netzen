# 01 .NET Runtime Model

## CLR, JIT, And Allocation

**Concept:** C# compiles to IL, and the CLR JIT compiles hot methods to native code at runtime. Objects such as `Ticket`, `TicketComment`, DTO records, and lists are heap allocations.

**Why it matters:** In a support inbox, high-volume search and live updates can create pressure through repeated snapshots and DTO mapping.

**Real code:** Search snapshots every ticket before filtering at `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:55-57`; DTO mapping creates records at `backend/SupportDesk.Api/Application/Dto.cs:69-90`; detail mapping creates a comments list at `backend/SupportDesk.Api/Application/Dto.cs:92-120`.

**Failure modes:** Excess allocations under load, large in-memory scans, GC pauses. In a DB-backed future, this shifts toward query shape and materialization.

**Drill:** Count the allocations in a search request that returns 50 tickets with 3 comments each. Basic: name snapshots and DTOs. Solid: explain why snapshots exist. Strong: propose a benchmark or profiling probe.

## Async/Await And Task

**Concept:** `async Task<T>` lets the request thread return to the pool while awaiting I/O. In this repo, many async methods wrap in-memory work, but the shape is ready for real I/O.

**Real code:** Endpoint lambdas await handlers at `backend/SupportDesk.Api/Endpoints/TicketEndpoints.cs:27-47`; handlers await store and notifier calls at `backend/SupportDesk.Api/Application/Commands/CreateTicket.cs:46-48`; store methods return `Task.FromResult` because no external I/O exists at `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:15-25`.

**Failure modes:** Sync-over-async deadlocks are less common in ASP.NET Core than old ASP.NET, but blocking on `.Result` would still waste threads. Long CPU work inside `async` methods still blocks.

**Drill:** Explain why `AddCommentHandler.HandleAsync` at `backend/SupportDesk.Api/Application/Commands/AddComment.cs:23-45` should stay async even though the current store is in-memory.

## CancellationToken

**Concept:** A cancellation token carries request-abort signals.

**Real code:** Endpoints accept `CancellationToken` at `backend/SupportDesk.Api/Endpoints/TicketEndpoints.cs:14-28`; store methods call `ThrowIfCancellationRequested` at `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:15-30`; SignalR send methods accept it at `backend/SupportDesk.Api/Realtime/SignalRTicketNotifier.cs:15-22`.

**Failure modes:** If later EF or HTTP calls ignore cancellation, abandoned requests keep doing work.

**Drill:** Add a test plan for cancellation in search. Do not write product code yet.

## Locks And Thread Safety

**Concept:** A singleton service shared by all requests must protect mutable state.

**Real code:** `ITicketStore` is singleton at `backend/SupportDesk.Api/Program.cs:35`; it uses `ConcurrentDictionary` plus `_writeLock` at `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:7-11`; updates hold the lock at `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:28-49`.

**Failure modes:** Captive dependency, deadlock from long work inside lock, mutable object escaping.

**Drill:** Explain why `Snapshot()` at `backend/SupportDesk.Api/Domain/Ticket.cs:136-156` matters when the store is singleton.

## IDisposable And Using

**Concept:** Scoped services and disposable resources must be released.

**Real code:** Seed data creates a scope with `using var` at `backend/SupportDesk.Api/Infrastructure/SeedData.cs:8-11`.

**Failure modes:** A future EF `DbContext` outside a request scope needs disposal; background services must create scopes deliberately.

**Drill:** Sketch how seed loading would change if `ITicketStore` became EF-backed.
