# 02 Framework Mental Models

## ASP.NET Core Pipeline

**Concept:** Requests enter middleware, then matched endpoints. Middleware order controls cross-cutting behavior.

**Real code:** Services are configured before `builder.Build()` at `backend/SupportDesk.Api/Program.cs:12-49`; middleware and endpoints are set after build at `backend/SupportDesk.Api/Program.cs:51-59`.

**Why it matters:** `app.UseExceptionHandler()` at `backend/SupportDesk.Api/Program.cs:53` can translate exceptions from later endpoints; CORS at `backend/SupportDesk.Api/Program.cs:54` must allow the React origin configured at `backend/SupportDesk.Api/Program.cs:16-20`.

**Failure modes:** Wrong middleware order, missing CORS credentials for SignalR, endpoint exceptions leaking stack traces.

**Drill:** Move mentally through a failed comment request and name each middleware/endpoint stage.

## Dependency Injection Lifetimes

**Concept:** Singleton lives for the process, scoped lives for a request, transient is new per resolution.

**Real code:** `IUserContext` is scoped at `backend/SupportDesk.Api/Program.cs:33-34`; stores are singleton at `backend/SupportDesk.Api/Program.cs:35-36`; handlers are scoped at `backend/SupportDesk.Api/Program.cs:39-46`; notifier is scoped at `backend/SupportDesk.Api/Program.cs:37`.

**Why it matters:** A singleton must not capture a scoped service. This repo avoids that by injecting `IUserContext` into scoped handlers, not into `InMemoryTicketStore`.

**Failure modes:** Captive dependency, shared mutable state, request data leaking between users.

**Drill:** Explain why `InMemoryTicketStore` can be singleton only because it manages locking at `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:7-49`.

## Minimal API Binding

**Concept:** ASP.NET Core binds route values, query parameters, body records, services, and cancellation tokens by type and position.

**Real code:** Search binds query params and handler service at `backend/SupportDesk.Api/Endpoints/TicketEndpoints.cs:14-28`; comment binds route id, body record, handler, and token at `backend/SupportDesk.Api/Endpoints/TicketEndpoints.cs:37-38`.

**Failure modes:** Ambiguous body binding, enum parse failures, missing validation attributes, accidental public endpoint.

**Drill:** For each parameter in `backend/SupportDesk.Api/Endpoints/TicketEndpoints.cs:14-25`, label source: service, query, route, body, or framework.

## React Render Model

**Concept:** State changes schedule renders. Effects run after commit and can clean up subscriptions.

**Real code:** State is declared at `frontend/src/App.tsx:35-48`; ticket refresh effect depends on `filters` at `frontend/src/App.tsx:50-52`; SignalR effect creates and cleans up a connection at `frontend/src/App.tsx:58-69`; derived SLA count uses `useMemo` at `frontend/src/App.tsx:77-80`.

**Failure modes:** Stale closures, effect reconnect loops, missing cleanup, over-fetching, state derived but stored separately.

**Drill:** Explain why the SignalR effect depends on `selectedId` at `frontend/src/App.tsx:69` and what reconnect behavior that causes.

## Data Fetching

**Concept:** The API wrapper centralizes headers, JSON, and error translation.

**Real code:** `request<T>` wraps fetch at `frontend/src/api.ts:26-42`; UI uses `listTickets` in `frontend/src/App.tsx:82-94`; mutations call typed API helpers at `frontend/src/App.tsx:111-161`.

**Failure modes:** Hidden auth assumptions, swallowed problem details, duplicated fetch logic, contract drift between C# DTOs and TS types.

**Drill:** Compare `TicketSummaryDto` at `backend/SupportDesk.Api/Application/Dto.cs:5-25` with `TicketSummary` at `frontend/src/types.ts:5-26`.
