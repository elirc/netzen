namespace SupportDesk.Api.Application.Queries;

public sealed class GetTicketHandler
{
    private readonly ITicketStore _store;
    private readonly IUserContext _user;

    public GetTicketHandler(ITicketStore store, IUserContext user)
    {
        _store = store;
        _user = user;
    }

    public async Task<TicketDetailDto> HandleAsync(Guid ticketId, CancellationToken cancellationToken)
    {
        var ticket = await _store.GetAsync(ticketId, cancellationToken)
            ?? throw new NotFoundException("Ticket was not found.");

        if (!Permissions.CanReadTicket(_user, ticket))
        {
            throw new ForbiddenException("You do not have permission to view this ticket.");
        }

        return ticket.ToDetailDto(DateTimeOffset.UtcNow, _user);
    }
}
