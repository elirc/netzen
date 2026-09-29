# 03 Domain Glossary

| Term | Meaning in this repo | Evidence | Transferable concept |
| --- | --- | --- | --- |
| Ticket | The aggregate representing one support conversation and workflow state. | Properties at `backend/SupportDesk.Api/Domain/Ticket.cs:50-70`; create logic at `backend/SupportDesk.Api/Domain/Ticket.cs:72-134`. | Aggregate: one consistency boundary for related state. |
| Ticket number | Human-readable sequence like `T-001001`. | Format at `backend/SupportDesk.Api/Domain/Ticket.cs:114-117`; sequence at `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:11-13`. | Public identifier differs from database identity. |
| Status | Workflow state: New, Open, Pending, OnHold, Solved, Closed. | Enum at `backend/SupportDesk.Api/Domain/TicketEnums.cs:3-11`; UI list at `frontend/src/App.tsx:24`. | State machine, even if lightweight. |
| Priority | SLA driver: Low, Normal, High, Urgent. | Enum at `backend/SupportDesk.Api/Domain/TicketEnums.cs:13-19`; SLA switch at `backend/SupportDesk.Api/Domain/Ticket.cs:98-112`. | Business rule input. |
| SLA | Due timestamps for first response and resolution. | Properties at `backend/SupportDesk.Api/Domain/Ticket.cs:62-70`; UI display at `frontend/src/App.tsx:347-350`. | Time-based invariant with operational risk. |
| Comment | Conversation entry with visibility. | `backend/SupportDesk.Api/Domain/TicketComment.cs:3-26`; DTO at `backend/SupportDesk.Api/Application/Dto.cs:50-56`. | Immutable event-like child entity. |
| Internal note | Comment visible to support staff, not customers. | Visibility enum at `backend/SupportDesk.Api/Domain/TicketEnums.cs:21-25`; mapping filter at `backend/SupportDesk.Api/Application/Dto.cs:92-120`. | Authorization on reads, not only writes. |
| Assignee | Agent user id assigned to ticket. | `backend/SupportDesk.Api/Domain/Ticket.cs:58`; assignment mutation at `backend/SupportDesk.Api/Domain/Ticket.cs:158-169`. | Ownership and routing. |
| Team | Routing bucket such as `billing` or `vip`. | Team checks at `backend/SupportDesk.Api/Application/Permissions.cs:7-11`; seed data at `backend/SupportDesk.Api/Infrastructure/SeedData.cs:26-29`. | Resource scoping. |
| Tag | Normalized label for filtering and routing. | Normalization at `backend/SupportDesk.Api/Domain/Ticket.cs:219-238`; filter at `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:90-93`. | Controlled vocabulary drift risk. |
| Canned reply | Reusable response body shown in composer. | Store at `backend/SupportDesk.Api/Infrastructure/InMemoryCannedReplyStore.cs:8-34`; UI select at `frontend/src/App.tsx:307-313`. | Content system / template boundary. |
| Version | Optimistic concurrency token. | Property at `backend/SupportDesk.Api/Domain/Ticket.cs:66`; check at `backend/SupportDesk.Api/Infrastructure/InMemoryTicketStore.cs:39-47`; UI sends it at `frontend/src/App.tsx:116`. | Prevent lost updates. |
| Permission | Resource-level authorization based on role, assignee, team, customer. | `backend/SupportDesk.Api/Application/Permissions.cs:7-39`. | Authorization is a business rule, not just login. |
| Demo user context | Header-derived identity used for learning. | Server parser at `backend/SupportDesk.Api/Auth/HeaderUserContext.cs:8-32`; client headers at `frontend/src/api.ts:19-24`. | Authn boundary; current version trusts client input. |
