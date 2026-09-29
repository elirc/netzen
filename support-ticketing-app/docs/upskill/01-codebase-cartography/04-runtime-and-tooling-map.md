# 04 Runtime And Tooling Map

## .NET

| Area | Evidence | Learning point |
| --- | --- | --- |
| SDK style project | `backend/SupportDesk.Api/SupportDesk.Api.csproj:1` | `Microsoft.NET.Sdk.Web` brings ASP.NET Core hosting defaults. |
| Target framework | `backend/SupportDesk.Api/SupportDesk.Api.csproj:2-7` | `net8.0`, nullable references, implicit usings. |
| Local URL | `backend/SupportDesk.Api/Properties/launchSettings.json:4-10` | `dotnet run` should listen at `http://localhost:5208`. |
| Service registration | `backend/SupportDesk.Api/Program.cs:33-46` | DI builds the object graph for handlers. |
| Middleware order | `backend/SupportDesk.Api/Program.cs:51-59` | Exception handling and CORS run before routes/hub. |

## React

| Area | Evidence | Learning point |
| --- | --- | --- |
| Tooling | `frontend/package.json:6-18` | Vite, TypeScript, React, SignalR client. |
| TypeScript strictness | `frontend/tsconfig.json:1-22` | Strict mode makes API contract drift visible earlier. |
| Render entry | `frontend/src/main.tsx:1-9` | React mounts `App` under `#root`. |
| API base | `frontend/src/api.ts:17` | Browser/server boundary is configurable through `VITE_API_BASE_URL`. |
| SignalR client | `frontend/src/api.ts:124-137` | Browser keeps a live connection for inbox events. |

## Runtime Boundaries

| Boundary | Real path | Failure modes |
| --- | --- | --- |
| Browser to API | `frontend/src/api.ts:26-42` | Network failure, CORS failure, stale API contract, untrusted headers. |
| API to domain | `backend/SupportDesk.Api/Application/Commands/CreateTicket.cs:34-44` | Handler bypasses invariant, missing validation, wrong time source. |
| Domain to store | `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:15-49` | Lost update, shared mutable state, cancellation ignored. |
| API to SignalR | `backend/SupportDesk.Api/Realtime/SignalRTicketNotifier.cs:15-22` | Event shape drift, disconnected clients, no durable outbox. |

## Environment And Secrets

No secrets mechanism is present. Development environment is set in `backend/SupportDesk.Api/Properties/launchSettings.json:9-10`. Demo identity is hardcoded into the client at `frontend/src/api.ts:19-24` and fallback server defaults are in `backend/SupportDesk.Api/Auth/HeaderUserContext.cs:12-20`. Treat this as a teaching stub, not a production authentication design.

## Verification Notes

- `dotnet --info` failed during inspection because `dotnet` was not on PATH.
- `npm --version` succeeded.
- No `global.json`, `Directory.Build.props`, `Directory.Packages.props`, `nuget.config`, Dockerfile, compose file, or CI workflow was found.
