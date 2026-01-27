using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TicketFlow.Domain.Entities;
using TicketFlow.Domain.ValueObjects;
using TicketFlow.Infrastructure.Persistence;
using TicketFlow.IntegrationTests.Abstractions;

namespace TicketFlow.IntegrationTests.Persistence;

[Collection("IntegrationTests")]
[Trait("Category", "Integration")]
public class TicketConcurrencyTests(IntegrationTestWebAppFactory factory)
{
    private readonly IServiceScopeFactory _scopeFactory = factory.Services.GetRequiredService<IServiceScopeFactory>();

    [Fact(DisplayName = "Should persist ticket and automatically generate RowVersion")]
    public async Task SaveChangesAsync_ShouldGenerateRowVersion_WhenTicketIsCreated()
    {
        // Arrange
        using IServiceScope scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TicketFlowDbContext>();

        DateTime now = DateTime.UtcNow;

        Show show = new(title: "Tech Conference 2025", date: DateTime.UtcNow.AddDays(30), maxTicketsPerUser: 10, currentDate: now);
        Seat seat = new(Sector: "VIP", Row: "A", Number: "1");
        Ticket ticket = new(show.Id, seat, price: 100m, createdDate: now);

        // Act
        context.Shows.Add(show);
        context.Tickets.Add(ticket);
        await context.SaveChangesAsync();

        // Assert
        Assert.NotEqual(Guid.Empty, ticket.Id);
        Assert.NotNull(ticket.RowVersion);
        Assert.NotEmpty(ticket.RowVersion);
    }

    [Fact(DisplayName = "Should throw DbUpdateConcurrencyException when optimistic locking detects conflict")]
    public async Task SaveChangesAsync_ShouldThrowDbUpdateConcurrencyException_WhenConcurrentUpdatesOccur()
    {
        // Arrange
        Guid ticketId;
        DateTime now = DateTime.UtcNow;

        using (IServiceScope seedScope = _scopeFactory.CreateScope())
        {
            var context = seedScope.ServiceProvider.GetRequiredService<TicketFlowDbContext>();

            Show show = new(title: "Rock Festival", date: DateTime.UtcNow.AddDays(60), maxTicketsPerUser: 5, currentDate: now);
            Ticket ticket = new(show.Id, new Seat(Sector: "General", Row: "B", Number: "10"), price: 500m, createdDate: now);

            context.AddRange(show, ticket);
            await context.SaveChangesAsync();

            ticketId = ticket.Id;
        }

        // Act
        using IServiceScope scopeUserA = _scopeFactory.CreateScope();
        var contextUserA = scopeUserA.ServiceProvider.GetRequiredService<TicketFlowDbContext>();
        Ticket? ticketUserA = await contextUserA.Tickets.FindAsync(ticketId);

        using IServiceScope scopeUserB = _scopeFactory.CreateScope();
        var contextUserB = scopeUserB.ServiceProvider.GetRequiredService<TicketFlowDbContext>();
        Ticket? ticketUserB = await contextUserB.Tickets.FindAsync(ticketId);

        ticketUserA!.Reserve(Guid.NewGuid(), currentDate: now.AddMinutes(1));
        await contextUserA.SaveChangesAsync();

        ticketUserB!.Reserve(Guid.NewGuid(), currentDate: now.AddMinutes(2));

        // Assert
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () =>
        {
            await contextUserB.SaveChangesAsync();
        });
    }
}