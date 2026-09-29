# Risk Register

| Risk | Evidence | Impact | Likelihood | Suggested test | Suggested fix | Confidence |
| --- | --- | --- | --- | --- | --- | --- |
| Demo header auth can be spoofed | Server reads headers at `backend/SupportDesk.Api/Auth/HeaderUserContext.cs:12-20`; client sends them at `frontend/src/api.ts:19-24` | Critical in production | High if deployed | API tests with missing/forged identity | Add real auth, keep demo mode local | Confirmed |
| Data lost on restart | In-memory store registered at `backend/SupportDesk.Api/Program.cs:35`; dictionary at `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:7-20` | High | Certain on restart | Restart persistence smoke test | Add EF/database store | Confirmed |
| No automated tests | No test project found; rules at `backend/SupportDesk.Api/Domain/Ticket.cs:72-239` untested | High | High | Add domain/permission/API tests | Create test projects and CI | Confirmed |
| Internal note leakage through realtime event | Commented event includes comment at `backend/SupportDesk.Api/Realtime/SignalRTicketNotifier.cs:21-22`; all clients can join inbox at `backend/SupportDesk.Api/Realtime/TicketHub.cs:7` | High | Medium | SignalR payload test for internal note | Filter payload or group by authorization | Possible risk |
| Search auth/paging semantics | Search gets 100 then filters auth at `backend/SupportDesk.Api/Application/Queries/SearchTickets.cs:17-28` | Medium | Medium | Search total tests per role | Push auth into store/query | Confirmed |
| Full-scan search does not scale | Store scans at `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:55-107` | Medium | High with data growth | Benchmark search sizes | DB indexes/search service | Confirmed |
| Poor conflict UX | UI sends version at `frontend/src/App.tsx:116` and shows generic error at `frontend/src/App.tsx:268-270` | Medium | Medium | Two-tab conflict test | Refresh/merge UX | Confirmed |
| SignalR missed events | Direct sends at `backend/SupportDesk.Api/Realtime/SignalRTicketNotifier.cs:15-22` | Medium | Medium | Disconnect/reconnect scenario | Outbox/replay | Confirmed |
| Effect reconnect churn | SignalR effect depends on `selectedId` at `frontend/src/App.tsx:58-69` | Low/Medium | Medium | Browser network observation | Own connection in stable hook | Possible risk |
| No rate limiting | No rate limiting in `backend/SupportDesk.Api/Program.cs:51-59` | Medium | Medium | Load/flood create/comment | Add rate limiting middleware | Confirmed |
| No structured logs | No `ILogger` in handlers such as `backend/SupportDesk.Api/Application/Commands/AddComment.cs:10-47` | Medium | High during incidents | Error scenario log test | Add structured logging/correlation | Confirmed |
| Dependency drift | Frontend semver ranges at `frontend/package.json:11-18`; no lockfile found | Medium | Medium | Repro install on clean machine | Commit lockfile | Confirmed |
