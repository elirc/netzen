namespace SupportDesk.Api.Application;

public interface IRealtimeTicketNotifier
{
    Task TicketCreatedAsync(TicketSummaryDto ticket, CancellationToken cancellationToken);
    Task TicketUpdatedAsync(TicketSummaryDto ticket, CancellationToken cancellationToken);
    Task TicketCommentedAsync(TicketSummaryDto ticket, CommentDto comment, CancellationToken cancellationToken);
}
