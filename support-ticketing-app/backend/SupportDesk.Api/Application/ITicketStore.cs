using SupportDesk.Api.Domain;

namespace SupportDesk.Api.Application;

public interface ITicketStore
{
    Task<Ticket> AddAsync(Ticket ticket, CancellationToken cancellationToken);
    Task<Ticket?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<Ticket> UpdateAsync(Guid id, int expectedVersion, Action<Ticket> mutate, CancellationToken cancellationToken);
    Task<PagedResult<Ticket>> SearchAsync(TicketSearchCriteria criteria, CancellationToken cancellationToken);
    long NextTicketNumber();
}
