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
    public void Create_RejectsUndefinedPriority()
    {
        var exception = Assert.Throws<DomainException>(() => Ticket.Create(
            1,
            "Subject",
            "Description",
            "Customer",
            "customer@example.com",
            (TicketPriority)99,
            "team",
            Array.Empty<string>(),
            DateTimeOffset.UtcNow));

        Assert.Equal("Ticket priority is not supported.", exception.Message);
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
