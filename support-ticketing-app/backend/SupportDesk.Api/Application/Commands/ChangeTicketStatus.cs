using SupportDesk.Api.Domain;

namespace SupportDesk.Api.Application.Commands;

public sealed record ChangeTicketStatusRequest(
    TicketStatus Status,
    int ExpectedVersion);

public sealed class ChangeTicketStatusHandler
{
    private readonly ITicketStore _store;
    private readonly IUserContext _user;
    private readonly IRealtimeTicketNotifier _notifier;

    public ChangeTicketStatusHandler(ITicketStore store, IUserContext user, IRealtimeTicketNotifier notifier)
    {
        _store = store;
        _user = user;
        _notifier = notifier;
    }

    public async Task<TicketDetailDto> HandleAsync(Guid ticketId, ChangeTicketStatusRequest request, CancellationToken cancellationToken)
    {
        var updated = await _store.UpdateAsync(
            ticketId,
            request.ExpectedVersion,
            ticket =>
            {
                if (!Permissions.CanChangeStatus(_user, ticket))
                {
                    throw new ForbiddenException("You do not have permission to change this ticket status.");
                }

                ticket.ChangeStatus(request.Status, DateTimeOffset.UtcNow);
            },
            cancellationToken);

        var now = DateTimeOffset.UtcNow;
        await _notifier.TicketUpdatedAsync(updated.ToSummaryDto(now), cancellationToken);
        return updated.ToDetailDto(now, _user);
    }
}
