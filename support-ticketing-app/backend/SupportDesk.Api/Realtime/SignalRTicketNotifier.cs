using Microsoft.AspNetCore.SignalR;
using SupportDesk.Api.Application;

namespace SupportDesk.Api.Realtime;

public sealed class SignalRTicketNotifier : IRealtimeTicketNotifier
{
    private readonly IHubContext<TicketHub> _hub;

    public SignalRTicketNotifier(IHubContext<TicketHub> hub)
    {
        _hub = hub;
    }

    public Task TicketCreatedAsync(TicketSummaryDto ticket, CancellationToken cancellationToken) =>
        _hub.Clients.Group("inbox").SendAsync("tickets.changed", cancellationToken);

    public Task TicketUpdatedAsync(TicketSummaryDto ticket, CancellationToken cancellationToken) =>
        _hub.Clients.Group("inbox").SendAsync("tickets.changed", cancellationToken);

    public Task TicketCommentedAsync(TicketSummaryDto ticket, CommentDto comment, CancellationToken cancellationToken) =>
        _hub.Clients.Group("inbox").SendAsync("tickets.changed", cancellationToken);
}
