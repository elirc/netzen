using SupportDesk.Api.Application;
using SupportDesk.Api.Application.Commands;
using SupportDesk.Api.Application.Queries;
using SupportDesk.Api.Domain;

namespace SupportDesk.Api.Endpoints;

public static class TicketEndpoints
{
    public static IEndpointRouteBuilder MapTicketEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tickets").WithTags("Tickets");

        group.MapGet("/", async (
            SearchTicketsHandler handler,
            CancellationToken cancellationToken,
            string? query = null,
            TicketStatus? status = null,
            TicketPriority? priority = null,
            string? assigneeId = null,
            string? teamId = null,
            string? tag = null,
            bool? breached = null,
            int page = 1,
            int pageSize = 25) =>
        {
            var criteria = new TicketSearchCriteria(query, status, priority, assigneeId, teamId, tag, breached, page, pageSize);
            return Results.Ok(await handler.HandleAsync(criteria, cancellationToken));
        });

        group.MapGet("/{id:guid}", async (Guid id, GetTicketHandler handler, CancellationToken cancellationToken) =>
            Results.Ok(await handler.HandleAsync(id, cancellationToken)));

        group.MapPost("/", async (CreateTicketRequest request, CreateTicketHandler handler, CancellationToken cancellationToken) =>
            Results.Created($"/api/tickets", await handler.HandleAsync(request, cancellationToken)));

        group.MapPost("/{id:guid}/comments", async (Guid id, AddCommentRequest request, AddCommentHandler handler, CancellationToken cancellationToken) =>
            Results.Ok(await handler.HandleAsync(id, request, cancellationToken)));

        group.MapPut("/{id:guid}/assignment", async (Guid id, AssignTicketRequest request, AssignTicketHandler handler, CancellationToken cancellationToken) =>
            Results.Ok(await handler.HandleAsync(id, request, cancellationToken)));

        group.MapPut("/{id:guid}/status", async (Guid id, ChangeTicketStatusRequest request, ChangeTicketStatusHandler handler, CancellationToken cancellationToken) =>
            Results.Ok(await handler.HandleAsync(id, request, cancellationToken)));

        group.MapPut("/{id:guid}/tags", async (Guid id, ReplaceTicketTagsRequest request, ReplaceTicketTagsHandler handler, CancellationToken cancellationToken) =>
            Results.Ok(await handler.HandleAsync(id, request, cancellationToken)));

        return app;
    }
}
