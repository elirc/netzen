using Microsoft.AspNetCore.SignalR;

namespace SupportDesk.Api.Realtime;

public sealed class TicketHub : Hub
{
    public Task JoinInbox() => Groups.AddToGroupAsync(Context.ConnectionId, "inbox");
}
