using SupportDesk.Api.Domain;

namespace SupportDesk.Api.Application.Commands;

public sealed record CreateTicketRequest(
    string Subject,
    string Description,
    string CustomerName,
    string CustomerEmail,
    TicketPriority Priority,
    string? TeamId,
    IReadOnlyList<string> Tags);

public sealed class CreateTicketHandler
{
    private readonly ITicketStore _store;
    private readonly IUserContext _user;
    private readonly IRealtimeTicketNotifier _notifier;

    public CreateTicketHandler(ITicketStore store, IUserContext user, IRealtimeTicketNotifier notifier)
    {
        _store = store;
        _user = user;
        _notifier = notifier;
    }

    public async Task<TicketDetailDto> HandleAsync(CreateTicketRequest request, CancellationToken cancellationToken)
    {
        if (!Permissions.CanCreateTicket(_user))
        {
            throw new ForbiddenException("You do not have permission to create tickets.");
        }

        var now = DateTimeOffset.UtcNow;
        var ticket = Ticket.Create(
            _store.NextTicketNumber(),
            request.Subject,
            request.Description,
            request.CustomerName,
            request.CustomerEmail,
            request.Priority,
            request.TeamId,
            request.Tags,
            now);

        var saved = await _store.AddAsync(ticket, cancellationToken);
        await _notifier.TicketCreatedAsync(saved.ToSummaryDto(now), cancellationToken);

        return saved.ToDetailDto(now, _user);
    }
}
