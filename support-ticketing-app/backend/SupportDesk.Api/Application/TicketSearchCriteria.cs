using SupportDesk.Api.Domain;

namespace SupportDesk.Api.Application;

public sealed record TicketSearchCriteria(
    string? Query,
    TicketStatus? Status,
    TicketPriority? Priority,
    string? AssigneeId,
    string? TeamId,
    string? Tag,
    bool? Breached,
    int Page,
    int PageSize);

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize);
