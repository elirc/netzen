# 03 Architecture And Patterns

Architecture here is about ownership and blast radius. The repo uses a single ASP.NET Core project, but still separates host setup (`backend/SupportDesk.Api/Program.cs:10-63`), endpoints (`backend/SupportDesk.Api/Endpoints/TicketEndpoints.cs:10-49`), command/query handlers (`backend/SupportDesk.Api/Application/Commands/CreateTicket.cs:14-51`, `backend/SupportDesk.Api/Application/Queries/SearchTickets.cs:3-30`), domain rules (`backend/SupportDesk.Api/Domain/Ticket.cs:3-239`), and infrastructure (`backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:7-120`).

Read:

1. `01-boundaries-and-layers.md`
2. `02-data-model-and-persistence.md`
3. `03-validation-auth-and-permissions.md`
4. `04-side-effects-async-and-reliability.md`
5. `05-pattern-catalog.md`
6. `06-architecture-critique.md`
