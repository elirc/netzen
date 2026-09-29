namespace SupportDesk.Api.Application.Commands;

public sealed record AssignTicketRequest(
    string? AssigneeId,
    string? TeamId,
    int ExpectedVersion);

public sealed class AssignTicketHandler
{
    private readonly ITicketStore _store;
    private readonly IUserContext _user;
    private readonly IRealtimeTicketNotifier _notifier;

    public AssignTicketHandler(ITicketStore store, IUserContext user, IRealtimeTicketNotifier notifier)
    {
        _store = store;
        _user = user;
        _notifier = notifier;
    }

    public async Task<TicketDetailDto> HandleAsync(Guid ticketId, AssignTicketRequest request, CancellationToken cancellationToken)
    {
        var updated = await _store.UpdateAsync(
            ticketId,
            request.ExpectedVersion,
            ticket =>
            {
                if (!Permissions.CanAssign(_user, ticket))
                {
                    throw new ForbiddenException("You do not have permission to assign this ticket.");
                }

                ticket.Assign(request.AssigneeId, request.TeamId, DateTimeOffset.UtcNow);
            },
            cancellationToken);

        var now = DateTimeOffset.UtcNow;
        await _notifier.TicketUpdatedAsync(updated.ToSummaryDto(now), cancellationToken);
        return updated.ToDetailDto(now, _user);
    }
}
