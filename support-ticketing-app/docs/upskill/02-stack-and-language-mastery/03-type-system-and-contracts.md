# 03 Type System And Contracts

## Nullable Reference Types

**Concept:** Nullable annotations document where null is valid and let the compiler warn about unsafe use.

**Real code:** Nullable is enabled at `backend/SupportDesk.Api/SupportDesk.Api.csproj:2-7`; assignment fields are nullable at `backend/SupportDesk.Api/Domain/Ticket.cs:58-59`; create request accepts nullable `TeamId` at `backend/SupportDesk.Api/Application/Commands/CreateTicket.cs:5-12`; TypeScript mirrors nullable assignee/team at `frontend/src/types.ts:13-14`.

**Failure modes:** Forgetting null checks, using empty string as fake null, contract mismatch.

**Drill:** Trace `teamId` from new ticket form default at `frontend/src/App.tsx:379-384` to domain property at `backend/SupportDesk.Api/Domain/Ticket.cs:58-59`.

## Records As Contracts

**Concept:** C# records are good DTOs because they are concise immutable-ish value carriers.

**Real code:** Request records at `backend/SupportDesk.Api/Application/Commands/AddComment.cs:5-8`; response records at `backend/SupportDesk.Api/Application/Dto.cs:5-65`; search criteria record supports `with` at `backend/SupportDesk.Api/Application/Queries/SearchTickets.cs:17`.

**Failure modes:** Treating DTO records as domain objects, adding fields without client updates, exposing internal-only fields.

**Drill:** Add a hypothetical `Channel` field. Name every DTO and TS type that would need review.

## Enums And String JSON

**Concept:** Enums model closed vocabularies. String JSON keeps API payloads human-readable but makes names a contract.

**Real code:** Domain enums at `backend/SupportDesk.Api/Domain/TicketEnums.cs:3-33`; JSON string conversion at `backend/SupportDesk.Api/Program.cs:24-32`; TS union types at `frontend/src/types.ts:1-3`.

**Failure modes:** Renaming enum values breaks clients, unknown enum values fail binding, UI lists drift from backend enum.

**Drill:** What breaks if `OnHold` becomes `WaitingOnCustomer`? Include `frontend/src/App.tsx:24` and `frontend/src/types.ts:1`.

## Generic Wrapper Types

**Concept:** Generics express reusable shapes while preserving item type.

**Real code:** `PagedResult<T>` at `backend/SupportDesk.Api/Application/TicketSearchCriteria.cs:13`; `PagedResponse<T>` at `backend/SupportDesk.Api/Application/Dto.cs:65`; TypeScript equivalent at `frontend/src/types.ts:50-55`.

**Failure modes:** Inconsistent paging semantics, total count after authorization vs before authorization, frontend assuming all pages are loaded.

**Drill:** Explain why search returns `visible.Count` at `backend/SupportDesk.Api/Application/Queries/SearchTickets.cs:24-28`.

## Pattern Matching And LINQ

**Concept:** Pattern matching makes rules readable; LINQ composes transformations.

**Real code:** Priority-to-SLA switch expressions at `backend/SupportDesk.Api/Domain/Ticket.cs:98-112`; status close pattern at `backend/SupportDesk.Api/Domain/Ticket.cs:178-180`; comment filtering uses LINQ at `backend/SupportDesk.Api/Application/Dto.cs:92-97`.

**Failure modes:** Deferred execution surprises, multiple enumeration, rules hidden in clever queries.

**Drill:** Rewrite the SLA switch in plain English, then list one test per priority.
