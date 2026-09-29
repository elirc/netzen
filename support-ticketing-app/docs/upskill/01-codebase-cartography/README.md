# 01 Codebase Cartography

Cartography means building a map before changing code. In this repo, the map is small but realistic: ASP.NET Core composes services and routes in `backend/SupportDesk.Api/Program.cs:10-63`, ticket workflows enter through minimal endpoints in `backend/SupportDesk.Api/Endpoints/TicketEndpoints.cs:10-49`, application handlers coordinate permissions and side effects in `backend/SupportDesk.Api/Application/Commands/CreateTicket.cs:27-49`, domain rules live in `backend/SupportDesk.Api/Domain/Ticket.cs:72-239`, and React owns browser state in `frontend/src/App.tsx:35-426`.

Read this section in order:

1. `01-system-map.md` for project ownership.
2. `02-file-reading-order.md` for a guided tour.
3. `03-domain-glossary.md` for the nouns and contracts.
4. `04-runtime-and-tooling-map.md` for build and runtime boundaries.
5. `05-key-flows.md` for end-to-end traces.

Verification note: EF Core, background worker projects, test projects, and CI workflows were not found in the file inventory. Those gaps are documented as "investigate/add" learning opportunities, not as existing subsystems.
