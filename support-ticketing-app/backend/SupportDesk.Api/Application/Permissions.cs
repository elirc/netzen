using SupportDesk.Api.Domain;

namespace SupportDesk.Api.Application;

public static class Permissions
{
    public static bool CanReadTicket(IUserContext user, Ticket ticket) =>
        user.Role is SupportRole.Admin or SupportRole.Manager ||
        user.UserId == ticket.AssigneeId ||
        (ticket.TeamId is not null && user.TeamIds.Contains(ticket.TeamId)) ||
        user.UserId.Equals(ticket.CustomerEmail, StringComparison.OrdinalIgnoreCase);

    public static bool CanCreateTicket(IUserContext user) =>
        user.Role is SupportRole.Admin or SupportRole.Manager or SupportRole.Agent or SupportRole.Customer;

    public static bool CanComment(IUserContext user, Ticket ticket, CommentVisibility visibility) =>
        visibility switch
        {
            CommentVisibility.Internal => (user.Role is SupportRole.Admin or SupportRole.Manager or SupportRole.Agent) && CanReadTicket(user, ticket),
            _ => (user.Role is SupportRole.Admin or SupportRole.Manager or SupportRole.Agent) && CanReadTicket(user, ticket)
                || user.UserId.Equals(ticket.CustomerEmail, StringComparison.OrdinalIgnoreCase)
        };

    public static bool CanAssign(IUserContext user, Ticket ticket) =>
        user.Role is SupportRole.Admin or SupportRole.Manager ||
        user.Role == SupportRole.Agent && CanReadTicket(user, ticket);

    public static bool CanChangeStatus(IUserContext user, Ticket ticket) =>
        user.Role is SupportRole.Admin or SupportRole.Manager ||
        user.Role == SupportRole.Agent && CanReadTicket(user, ticket);

    public static bool CanTag(IUserContext user, Ticket ticket) =>
        user.Role is SupportRole.Admin or SupportRole.Manager ||
        user.Role == SupportRole.Agent && CanReadTicket(user, ticket);

    public static bool CanReadInternalNotes(IUserContext user, Ticket ticket) =>
        user.Role is SupportRole.Admin or SupportRole.Manager ||
        user.Role == SupportRole.Agent && CanReadTicket(user, ticket);
}
