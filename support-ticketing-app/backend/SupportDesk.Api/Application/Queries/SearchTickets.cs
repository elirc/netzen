using SupportDesk.Api.Domain;

namespace SupportDesk.Api.Application.Queries;

public sealed class SearchTicketsHandler
{
    private readonly ITicketStore _store;
    private readonly IUserContext _user;

    public SearchTicketsHandler(ITicketStore store, IUserContext user)
    {
        _store = store;
        _user = user;
    }

    public async Task<PagedResponse<TicketSummaryDto>> HandleAsync(TicketSearchCriteria criteria, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var visible = new List<TicketSummaryDto>();
        var sourcePage = 1;
        PagedResult<Ticket> searchResult;
        do
        {
            searchResult = await _store.SearchAsync(criteria with { Page = sourcePage, PageSize = 100 }, cancellationToken);
            visible.AddRange(searchResult.Items.Where(ticket => Permissions.CanReadTicket(_user, ticket))
                .Select(ticket => ticket.ToSummaryDto(now)));
            sourcePage++;
        } while (sourcePage <= Math.Max(1, (int)Math.Ceiling(searchResult.Total / 100d)));

        var page = Math.Max(criteria.Page, 1);
        var pageSize = Math.Clamp(criteria.PageSize, 1, 100);
        var items = visible.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return new PagedResponse<TicketSummaryDto>(items, visible.Count, page, pageSize);
    }
}
