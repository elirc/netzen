# 4. Test the safety boundaries

The complete focused fixture is [`TicketSafetyTests.cs`](../support-ticketing-app/backend/SupportDesk.Api.Tests/TicketSafetyTests.cs). It belongs in the `SupportDesk.Api.Tests` namespace and uses the real domain, in-memory store, and search handler. The current source is reproduced here so placement and imports stay concrete:

```csharp
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SupportDesk.Api.Domain;
using SupportDesk.Api.Application;
using SupportDesk.Api.Application.Queries;
using SupportDesk.Api.Infrastructure;
using Xunit;

namespace SupportDesk.Api.Tests;

public sealed class TicketSafetyTests
{
    private static Ticket MakeTicket() => Ticket.Create(1, "Subject", "Description", "Customer", "customer@example.com", TicketPriority.Normal, "team", new[] { "alpha", "beta" }, DateTimeOffset.UtcNow);

    [Fact]
    public void ReplaceTags_DoesNotPartiallyMutate_WhenEnumerationFails()
    {
        var ticket = MakeTicket();
        var version = ticket.Version;
        IEnumerable<string> Broken()
        {
            yield return "changed";
            throw new InvalidOperationException("enumeration failed");
        }

        Assert.Throws<InvalidOperationException>(() => ticket.ReplaceTags(Broken(), DateTimeOffset.UtcNow));
        Assert.Equal(version, ticket.Version);
        Assert.Equal(new[] { "alpha", "beta" }, ticket.Tags);
    }

    [Fact]
    public void DomainMethods_RejectUndefinedEnums()
    {
        var ticket = MakeTicket();
        Assert.Throws<DomainException>(() => ticket.ChangeStatus((TicketStatus)99, DateTimeOffset.UtcNow));
        Assert.Throws<DomainException>(() => ticket.UpdatePriority((TicketPriority)99, DateTimeOffset.UtcNow));
        Assert.Throws<DomainException>(() => ticket.AddComment("agent", "Agent", "body", (CommentVisibility)99, DateTimeOffset.UtcNow));
    }

    [Fact]
    public async Task Search_UsesAuthorizedPopulationAcrossSourcePages_AndHidesInternalMarkers()
    {
        var store = new InMemoryTicketStore();
        var now = DateTimeOffset.UtcNow;
        var customer = Ticket.Create(1, "Visible customer ticket", "Description", "Customer", "customer@example.com", TicketPriority.Normal, null, Array.Empty<string>(), now);
        customer.AddComment("agent", "Agent", "INTERNAL-ONLY-MARKER", CommentVisibility.Internal, now);
        store.LoadSeedTickets(new[] { customer });
        for (var i = 2; i <= 121; i++)
        {
            var ticket = Ticket.Create(i, $"Visible {i}", "Description", "Other", $"other{i}@example.com", TicketPriority.Normal, null, Array.Empty<string>(), now.AddSeconds(i));
            store.LoadSeedTickets(new[] { ticket });
        }

        var result = await new SearchTicketsHandler(store, new FakeUser("customer@example.com", SupportRole.Customer))
            .HandleAsync(new TicketSearchCriteria(null, null, null, null, null, null, null, 1, 50), CancellationToken.None);
        Assert.Equal(1, result.Total);
        Assert.Single(result.Items);
        Assert.Equal("T-000001", result.Items[0].Number);

        var hiddenSearch = await new SearchTicketsHandler(store, new FakeUser("customer@example.com", SupportRole.Customer))
            .HandleAsync(new TicketSearchCriteria("INTERNAL-ONLY-MARKER", null, null, null, null, null, null, 1, 50), CancellationToken.None);
        Assert.Empty(hiddenSearch.Items);
        Assert.Equal(0, hiddenSearch.Total);
    }

    private sealed class FakeUser(string userId, SupportRole role) : IUserContext
    {
        public string UserId { get; } = userId;
        public string DisplayName => "Test user";
        public SupportRole Role { get; } = role;
        public IReadOnlySet<string> TeamIds => new HashSet<string>();
    }
}
```

The first test proves enumeration failure is atomic at the aggregate boundary. The second proves runtime enum values cannot bypass domain checks. The third seeds one customer-readable ticket followed by 120 tickets owned by other users. The customer still receives ticket `T-000001` because the handler gathers source pages before permission filtering; searching for the internal marker returns zero because the store searches public comments only. The test does not prove complete actor policies, a live hub, a database, or an atomic cross-page snapshot.

Run from `support-ticketing-app/backend` with `dotnet restore SupportDesk.Api.Tests/SupportDesk.Api.Tests.csproj`, then set `$env:DOTNET_ROLL_FORWARD='Major'` if needed and run `dotnet test SupportDesk.Api.Tests/SupportDesk.Api.Tests.csproj -c Release --no-restore`. The recorded `root-dotnet-test-01` result is 3 xUnit tests passing with TRX. From `support-ticketing-app/frontend`, `npm ci` and `npm run build` provide the recorded `root-web-build-02` check. There is no browser or mounted SignalR test in this evidence.

**Exercise 4.** Locate the diagnostic assertion in the search test showing that `result.Items[0].Number` is `T-000001`, then explain why checking only `result.Total` would miss a wrong page projection. Name the missing live-system checks.
