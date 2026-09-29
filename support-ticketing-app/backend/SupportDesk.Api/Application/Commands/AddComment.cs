using SupportDesk.Api.Domain;

namespace SupportDesk.Api.Application.Commands;

public sealed record AddCommentRequest(
    string Body,
    CommentVisibility Visibility,
    int ExpectedVersion);

public sealed class AddCommentHandler
{
    private readonly ITicketStore _store;
    private readonly IUserContext _user;
    private readonly IRealtimeTicketNotifier _notifier;

    public AddCommentHandler(ITicketStore store, IUserContext user, IRealtimeTicketNotifier notifier)
    {
        _store = store;
        _user = user;
        _notifier = notifier;
    }

    public async Task<TicketDetailDto> HandleAsync(Guid ticketId, AddCommentRequest request, CancellationToken cancellationToken)
    {
        TicketComment? newComment = null;

        var updated = await _store.UpdateAsync(
            ticketId,
            request.ExpectedVersion,
            ticket =>
            {
                if (!Permissions.CanComment(_user, ticket, request.Visibility))
                {
                    throw new ForbiddenException("You do not have permission to comment on this ticket.");
                }

                newComment = ticket.AddComment(_user.UserId, _user.DisplayName, request.Body, request.Visibility, DateTimeOffset.UtcNow);
            },
            cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var commentDto = newComment?.ToDto() ?? throw new InvalidOperationException("Comment was not created.");
        await _notifier.TicketCommentedAsync(updated.ToSummaryDto(now), commentDto, cancellationToken);

        return updated.ToDetailDto(now, _user);
    }
}
