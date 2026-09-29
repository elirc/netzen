namespace SupportDesk.Api.Application.Queries;

public sealed class GetCannedRepliesHandler
{
    private readonly ICannedReplyStore _store;

    public GetCannedRepliesHandler(ICannedReplyStore store)
    {
        _store = store;
    }

    public async Task<IReadOnlyList<CannedReplyDto>> HandleAsync(CancellationToken cancellationToken)
    {
        var replies = await _store.ListAsync(cancellationToken);
        return replies.Select(reply => reply.ToDto()).ToList();
    }
}
