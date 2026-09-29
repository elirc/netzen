# SupportDesk Upskill Lab

This curriculum turns this repository into a training lab for a junior engineer growing toward mid-level and senior judgment. SupportDesk is a compact customer support inbox: React renders ticket queues and detail panels, ASP.NET Core exposes minimal APIs, command/query handlers coordinate work, a domain aggregate protects ticket invariants, an in-memory store demonstrates persistence boundaries, and SignalR broadcasts live updates. The runtime is split between the browser client in `frontend/src/App.tsx:35-426`, the API host in `backend/SupportDesk.Api/Program.cs:10-63`, and the backend feature modules registered in `backend/SupportDesk.Api/Program.cs:33-46`. Persistence is not EF Core in this version; the active store is `InMemoryTicketStore`, registered at `backend/SupportDesk.Api/Program.cs:35` and implemented at `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:7-120`. Auth is a learning boundary based on request headers, wired at `backend/SupportDesk.Api/Program.cs:33-34` and parsed at `backend/SupportDesk.Api/Auth/HeaderUserContext.cs:8-32`. Tests and CI are not present; treat that as a deliberate lab gap to design, not as invisible coverage.

Senior mindset: junior asks "how do I make it work?", mid-level asks "is this the right pattern?", senior asks "what does this commit us to, who pays, and how do we reduce risk?"

## How To Use This

| Timeline | Path |
| --- | --- |
| Weekend | Read `00-fast-track.md`, trace create ticket and add comment, then do one good-first ticket. |
| Two weeks | Add cartography, stack mental models, 4 code-reading drills, and 2 debugging scenarios. |
| Eight weeks | Work through architecture, testing, performance, security, contribution tickets, and interview prep. |
| Ongoing | Keep `08-reference/risk-register.md` and `08-reference/verification-log.md` current as the code evolves. |

## Recommended Tracks

| Learner | Start | Then |
| --- | --- | --- |
| Brand-new junior | `00-fast-track.md` | `01-codebase-cartography/02-file-reading-order.md`, then `04-code-reading-gym/01-annotation-drills.md`. |
| Junior with stack familiarity | `01-codebase-cartography/05-key-flows.md` | `02-stack-and-language-mastery/02-framework-mental-models.md`, then good-first tickets. |
| Mid-level new to repo | `03-architecture-and-patterns/01-boundaries-and-layers.md` | `05-quality-engineering/01-testing-strategy.md`, then mid-level feature tickets. |
| Senior architecture reviewer | `03-architecture-and-patterns/06-architecture-critique.md` | `08-reference/risk-register.md`, then senior build projects. |

## Conventions

- Codebase claims point to exact paths and line ranges, for example `backend/SupportDesk.Api/Program.cs:33-46`.
- Absences are based on repository inventory and are called out in `08-reference/verification-log.md`.
- Fake code is always labeled `Illustrative fake code: not from this repo`.
- Drills ask you to annotate inputs, outputs, dependencies, invariants, side effects, failure modes, and tests.
- Self-grading uses Basic, Solid, Strong. Basic follows the code. Solid explains why. Strong predicts risk and proposes a check.

## Core Map

| Concern | Real code |
| --- | --- |
| Host, DI, middleware, routes | `backend/SupportDesk.Api/Program.cs:10-63` |
| Ticket domain aggregate | `backend/SupportDesk.Api/Domain/Ticket.cs:3-239` |
| Minimal API endpoints | `backend/SupportDesk.Api/Endpoints/TicketEndpoints.cs:10-49` |
| Command handlers | `backend/SupportDesk.Api/Application/Commands/CreateTicket.cs:14-51`, `backend/SupportDesk.Api/Application/Commands/AddComment.cs:10-47` |
| Query handlers | `backend/SupportDesk.Api/Application/Queries/SearchTickets.cs:3-30`, `backend/SupportDesk.Api/Application/Queries/GetTicket.cs:1-22` |
| DTO contracts | `backend/SupportDesk.Api/Application/Dto.cs:5-65`, `frontend/src/types.ts:1-63` |
| Permissions | `backend/SupportDesk.Api/Application/Permissions.cs:5-39` |
| Demo auth headers | `backend/SupportDesk.Api/Auth/HeaderUserContext.cs:8-32`, `frontend/src/api.ts:19-24` |
| Live updates | `backend/SupportDesk.Api/Realtime/SignalRTicketNotifier.cs:6-23`, `frontend/src/api.ts:124-137` |
| React inbox | `frontend/src/App.tsx:35-426` |

## Verification Notes

- Verified by reading files with line numbers on 2026-06-09.
- `dotnet` was not available on PATH during verification, so build and test commands are inferred, not executed.
- `npm --version` passed, but dependencies were not installed because the original build request said not to install anything.
- No EF Core, hosted service, test project, Dockerfile, or CI workflow was found in the inspected file inventory. See `08-reference/verification-log.md`.
