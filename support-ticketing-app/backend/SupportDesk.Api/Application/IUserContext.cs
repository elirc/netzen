using SupportDesk.Api.Domain;

namespace SupportDesk.Api.Application;

public interface IUserContext
{
    string UserId { get; }
    string DisplayName { get; }
    SupportRole Role { get; }
    IReadOnlySet<string> TeamIds { get; }
}
