using SupportDesk.Api.Domain;

namespace SupportDesk.Api.Application;

public sealed record TicketSummaryDto(
    Guid Id,
    string Number,
    string Subject,
    string CustomerName,
    string CustomerEmail,
    TicketStatus Status,
    TicketPriority Priority,
    string? AssigneeId,
    string? TeamId,
    IReadOnlyList<string> Tags,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset FirstResponseDueAt,
    DateTimeOffset ResolutionDueAt,
    DateTimeOffset? FirstRespondedAt,
    DateTimeOffset? ClosedAt,
    bool FirstResponseBreached,
    bool ResolutionBreached,
    int CommentCount,
    int Version);

public sealed record TicketDetailDto(
    Guid Id,
    string Number,
    string Subject,
    string Description,
    string CustomerName,
    string CustomerEmail,
    TicketStatus Status,
    TicketPriority Priority,
    string? AssigneeId,
    string? TeamId,
    IReadOnlyList<string> Tags,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset FirstResponseDueAt,
    DateTimeOffset ResolutionDueAt,
    DateTimeOffset? FirstRespondedAt,
    DateTimeOffset? ClosedAt,
    bool FirstResponseBreached,
    bool ResolutionBreached,
    IReadOnlyList<CommentDto> Comments,
    int Version);

public sealed record CommentDto(
    Guid Id,
    string AuthorId,
    string AuthorName,
    string Body,
    CommentVisibility Visibility,
    DateTimeOffset CreatedAt);

public sealed record CannedReplyDto(
    Guid Id,
    string Title,
    string Body,
    IReadOnlyList<string> Tags,
    TicketPriority? SuggestedPriority);

public sealed record PagedResponse<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize);

public static class TicketMapping
{
    public static TicketSummaryDto ToSummaryDto(this Domain.Ticket ticket, DateTimeOffset now) =>
        new(
            ticket.Id,
            ticket.Number,
            ticket.Subject,
            ticket.CustomerName,
            ticket.CustomerEmail,
            ticket.Status,
            ticket.Priority,
            ticket.AssigneeId,
            ticket.TeamId,
            ticket.Tags,
            ticket.CreatedAt,
            ticket.UpdatedAt,
            ticket.FirstResponseDueAt,
            ticket.ResolutionDueAt,
            ticket.FirstRespondedAt,
            ticket.ClosedAt,
            ticket.IsFirstResponseBreached(now),
            ticket.IsResolutionBreached(now),
            ticket.Comments.Count,
            ticket.Version);

    public static TicketDetailDto ToDetailDto(this Domain.Ticket ticket, DateTimeOffset now, IUserContext user)
    {
        var comments = ticket.Comments
            .Where(comment => comment.Visibility == CommentVisibility.Public || Permissions.CanReadInternalNotes(user, ticket))
            .Select(comment => comment.ToDto())
            .ToList();

        return new TicketDetailDto(
            ticket.Id,
            ticket.Number,
            ticket.Subject,
            ticket.Description,
            ticket.CustomerName,
            ticket.CustomerEmail,
            ticket.Status,
            ticket.Priority,
            ticket.AssigneeId,
            ticket.TeamId,
            ticket.Tags,
            ticket.CreatedAt,
            ticket.UpdatedAt,
            ticket.FirstResponseDueAt,
            ticket.ResolutionDueAt,
            ticket.FirstRespondedAt,
            ticket.ClosedAt,
            ticket.IsFirstResponseBreached(now),
            ticket.IsResolutionBreached(now),
            comments,
            ticket.Version);
    }

    public static CommentDto ToDto(this Domain.TicketComment comment) =>
        new(
            comment.Id,
            comment.AuthorId,
            comment.AuthorName,
            comment.Body,
            comment.Visibility,
            comment.CreatedAt);

    public static CannedReplyDto ToDto(this Domain.CannedReply reply) =>
        new(reply.Id, reply.Title, reply.Body, reply.Tags, reply.SuggestedPriority);
}
