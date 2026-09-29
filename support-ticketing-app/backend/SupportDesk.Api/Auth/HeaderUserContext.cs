using SupportDesk.Api.Application;
using SupportDesk.Api.Domain;

namespace SupportDesk.Api.Auth;

public sealed class HeaderUserContext : IUserContext
{
    public HeaderUserContext(IHttpContextAccessor accessor)
    {
        var headers = accessor.HttpContext?.Request.Headers;

        UserId = Read(headers, "X-User-Id", "agent-1");
        DisplayName = Read(headers, "X-User-Name", "Avery Stone");
        Role = Enum.TryParse<SupportRole>(Read(headers, "X-User-Role", "Agent"), ignoreCase: true, out var role)
            ? role
            : SupportRole.Agent;

        TeamIds = Read(headers, "X-Team-Ids", "billing,vip")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    public string UserId { get; }
    public string DisplayName { get; }
    public SupportRole Role { get; }
    public IReadOnlySet<string> TeamIds { get; }

    private static string Read(IHeaderDictionary? headers, string key, string fallback) =>
        headers is not null && headers.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value.ToString()
            : fallback;
}
