using SupportDesk.Api.Application.Queries;

namespace SupportDesk.Api.Endpoints;

public static class CannedReplyEndpoints
{
    public static IEndpointRouteBuilder MapCannedReplyEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/canned-replies", async (GetCannedRepliesHandler handler, CancellationToken cancellationToken) =>
            Results.Ok(await handler.HandleAsync(cancellationToken)))
            .WithTags("Canned Replies");

        return app;
    }
}
