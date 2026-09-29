namespace SupportDesk.Api.Application.Commands;

public sealed record ReplaceTicketTagsRequest(
    IReadOnlyList<string> Tags,
    int ExpectedVersion);

public sealed class ReplaceTicketTagsHandler
{
    private readonly ITicketStore _store;
    private readonly IUserContext _user;
    private readonly IRealtimeTicketNotifier _notifier;

    public ReplaceTicketTagsHandler(ITicketStore store, IUserContext user, IRealtimeTicketNotifier notifier)
    {
        _store = store;
        _user = user;
        _notifier = notifier;
    }

    public async Task<TicketDetailDto> HandleAsync(Guid ticketId, ReplaceTicketTagsRequest request, CancellationToken cancellationToken)
    {
        var updated = await _store.UpdateAsync(
            ticketId,
            request.ExpectedVersion,
            ticket =>
            {
                if (!Permissions.CanTag(_user, ticket))
                {
                    throw new ForbiddenException("You do not have permission to tag this ticket.");
                }

                ticket.ReplaceTags(request.Tags, DateTimeOffset.UtcNow);
            },
            cancellationToken);

        var now = DateTimeOffset.UtcNow;
        await _notifier.TicketUpdatedAsync(updated.ToSummaryDto(now), cancellationToken);
        return updated.ToDetailDto(now, _user);
    }
}
