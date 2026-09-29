namespace SupportDesk.Api.Domain;

public sealed record CannedReply(
    Guid Id,
    string Title,
    string Body,
    IReadOnlyList<string> Tags,
    TicketPriority? SuggestedPriority);
