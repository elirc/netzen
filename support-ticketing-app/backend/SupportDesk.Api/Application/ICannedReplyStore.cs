using SupportDesk.Api.Domain;

namespace SupportDesk.Api.Application;

public interface ICannedReplyStore
{
    Task<IReadOnlyList<CannedReply>> ListAsync(CancellationToken cancellationToken);
}
