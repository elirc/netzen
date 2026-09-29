# 04 Performance Thinking

Performance work starts with measurement. This repo has no metrics/profiling yet, so treat all items as likely hotspots or design risks, not proven bottlenecks.

## Likely Hotspots

| Area | Evidence | Risk | Measurement |
| --- | --- | --- | --- |
| Search full scan | `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:55-107` | Latency and allocations grow with tickets | Benchmark search over 1k, 10k, 100k tickets. |
| Snapshot copies | `backend/SupportDesk.Api/Domain/Ticket.cs:136-156` | Allocation pressure | `dotnet-counters` GC metrics after SDK installed. |
| Store lock | `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:28-49` | Serialized writes | Load test concurrent comments. |
| React all-in-one component | `frontend/src/App.tsx:35-426` | Unnecessary rerenders | React Profiler after app runs. |
| SignalR reconnect effect | `frontend/src/App.tsx:58-69` | Connection churn on selection changes | Browser network panel. |
| Unbounded live group | `backend/SupportDesk.Api/Realtime/TicketHub.cs:7` | All clients receive all inbox events | Count event fanout under many agents. |

## Transferable Checks

- Server latency: measure P50/P95 per endpoint.
- DB future: inspect generated SQL and indexes for filters currently in `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:58-104`.
- Client render: profile before memoizing.
- Bundle: run `npm run build` and inspect Vite output after dependencies are installed.
- Allocations: avoid premature micro-optimizing until `Snapshot()` pressure is measured.

## Drill

Design a performance test for search. Use filters from `backend/SupportDesk.Api/Endpoints/TicketEndpoints.cs:17-25`, generate tickets shaped like `backend/SupportDesk.Api/Infrastructure/SeedData.cs:18-57`, and record latency plus allocation.
