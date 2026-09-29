namespace SupportDesk.Api.Domain;

public sealed class TicketComment
{
    public TicketComment(
        Guid id,
        string authorId,
        string authorName,
        string body,
        CommentVisibility visibility,
        DateTimeOffset createdAt)
    {
        Id = id;
        AuthorId = authorId;
        AuthorName = authorName;
        Body = body;
        Visibility = visibility;
        CreatedAt = createdAt;
    }

    public Guid Id { get; }
    public string AuthorId { get; }
    public string AuthorName { get; }
    public string Body { get; }
    public CommentVisibility Visibility { get; }
    public DateTimeOffset CreatedAt { get; }
}
