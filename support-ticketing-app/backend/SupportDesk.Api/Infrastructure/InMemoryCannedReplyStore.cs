using SupportDesk.Api.Application;
using SupportDesk.Api.Domain;

namespace SupportDesk.Api.Infrastructure;

public sealed class InMemoryCannedReplyStore : ICannedReplyStore
{
    private readonly IReadOnlyList<CannedReply> _replies =
    [
        new CannedReply(
            Guid.Parse("48ad0590-131b-45ac-84c8-d30024a31801"),
            "Refund policy",
            "Thanks for reaching out. I checked your order and can help with the refund. Our refund window is 30 days from purchase, and I am reviewing the transaction details now.",
            ["billing", "refund"],
            TicketPriority.Normal),
        new CannedReply(
            Guid.Parse("3b5193cb-05da-4077-bf2f-cef2e0d2c501"),
            "Login troubleshooting",
            "I am sorry you are running into login trouble. Please try resetting your password, then reply with the timestamp and browser if the issue continues.",
            ["login", "account"],
            TicketPriority.High),
        new CannedReply(
            Guid.Parse("9c3585d8-835f-4476-a049-e1cf9ccf4c67"),
            "Engineering escalation",
            "Internal note: reproduced and ready for escalation. Include affected account, repro steps, expected behavior, actual behavior, and screenshots/logs if available.",
            ["bug", "escalation"],
            TicketPriority.High)
    ];

    public Task<IReadOnlyList<CannedReply>> ListAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_replies);
    }
}
