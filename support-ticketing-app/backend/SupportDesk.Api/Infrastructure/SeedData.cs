using SupportDesk.Api.Domain;
using SupportDesk.Api.Application;

namespace SupportDesk.Api.Infrastructure;

public static class SeedData
{
    public static void Load(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<ITicketStore>();

        if (store is not InMemoryTicketStore memoryStore)
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        var first = Ticket.Create(
            1001,
            "Refund still missing after invoice correction",
            "The invoice was corrected last week, but the card has not received the refund yet.",
            "Mina Patel",
            "mina@example.com",
            TicketPriority.High,
            "billing",
            ["billing", "refund", "vip"],
            now.AddHours(-8));
        first.Assign("agent-1", "billing", now.AddHours(-7));
        first.AddComment("agent-1", "Avery Stone", "I confirmed the credit memo exists and am checking the processor trace.", CommentVisibility.Internal, now.AddHours(-6));

        var second = Ticket.Create(
            1002,
            "Cannot log in after SSO migration",
            "Our team moved to SSO and three users now loop back to the login screen.",
            "Noah Rivera",
            "noah@example.com",
            TicketPriority.Urgent,
            "vip",
            ["login", "sso", "vip"],
            now.AddHours(-3));
        second.Assign("agent-2", "vip", now.AddHours(-2));
        second.AddComment("agent-2", "Jordan Lee", "Thanks for the detail. We are checking the SAML attributes for your workspace.", CommentVisibility.Public, now.AddHours(-1));

        var third = Ticket.Create(
            1003,
            "Feature request: export conversation transcript",
            "We need a way to export the full support transcript for audit reviews.",
            "Priya Shah",
            "priya@example.com",
            TicketPriority.Low,
            "product",
            ["feature-request", "export"],
            now.AddDays(-2));
        third.ChangeStatus(TicketStatus.Pending, now.AddDays(-1));

        memoryStore.LoadSeedTickets([first, second, third]);
    }
}
