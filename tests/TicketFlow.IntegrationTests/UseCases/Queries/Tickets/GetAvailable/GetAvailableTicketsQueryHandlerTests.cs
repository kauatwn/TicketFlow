using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TicketFlow.Application.UseCases.Queries.Tickets.GetAvailable;
using TicketFlow.Domain.Entities;
using TicketFlow.Domain.ValueObjects;
using TicketFlow.Infrastructure.Persistence;
using TicketFlow.IntegrationTests.Abstractions;

namespace TicketFlow.IntegrationTests.UseCases.Queries.Tickets.GetAvailable;

[Collection("IntegrationTests")]
[Trait("Category", "Integration")]
public class GetAvailableTicketsQueryHandlerTests(IntegrationTestWebAppFactory factory)
{
    private readonly IServiceScopeFactory _scopeFactory = factory.Services.GetRequiredService<IServiceScopeFactory>();

    [Fact(DisplayName = "Should return only available tickets for show")]
    public async Task Handle_ShouldReturnOnlyAvailableTickets_WhenShowHasMixedTickets()
    {
        // Arrange
        Guid showId;
        DateTime now = DateTime.UtcNow;

        using (IServiceScope scope = _scopeFactory.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<TicketFlowDbContext>();

            Show show = new(title: "Mixed Tickets Concert", date: now.AddDays(30), maxTicketsPerUser: 5, currentDate: now);

            Ticket availableTicket1 = new(show.Id, new Seat(Sector: "VIP", Row: "A", Number: "1"), price: 500m, createdDate: now);
            Ticket availableTicket2 = new(show.Id, new Seat(Sector: "General", Row: "B", Number: "1"), price: 200m, createdDate: now);

            Ticket reservedTicket = new(show.Id, new Seat(Sector: "VIP", Row: "A", Number: "2"), price: 500m, createdDate: now);
            reservedTicket.Reserve(Guid.NewGuid(), now);

            context.Shows.Add(show);
            context.Tickets.AddRange(availableTicket1, availableTicket2, reservedTicket);
            await context.SaveChangesAsync();

            showId = show.Id;
        }

        // Act & Assert
        using (IServiceScope scope = _scopeFactory.CreateScope())
        {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            GetAvailableTicketsQuery query = new(showId);

            List<TicketResponse> result = await mediator.Send(query);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);

            Assert.All(result, ticket =>
            {
                Assert.NotEqual(Guid.Empty, ticket.Id);
                Assert.NotEmpty(ticket.Sector);
                Assert.NotEmpty(ticket.Row);
                Assert.NotEmpty(ticket.Number);
                Assert.True(ticket.Price > 0);
            });
        }
    }

    [Fact(DisplayName = "Should return empty list when show has no available tickets")]
    public async Task Handle_ShouldReturnEmptyList_WhenShowHasNoAvailableTickets()
    {
        // Arrange
        Guid showId;
        DateTime now = DateTime.UtcNow;

        using (IServiceScope scope = _scopeFactory.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<TicketFlowDbContext>();

            Show show = new(title: "Sold Out Concert", date: now.AddDays(30), maxTicketsPerUser: 5, currentDate: now);

            Ticket reservedTicket1 = new(show.Id, new Seat(Sector: "VIP", Row: "A", Number: "1"), price: 500m, createdDate: now);
            reservedTicket1.Reserve(Guid.NewGuid(), now);

            Ticket reservedTicket2 = new(show.Id, new Seat(Sector: "VIP", Row: "A", Number: "2"), price: 500m, createdDate: now);
            reservedTicket2.Reserve(Guid.NewGuid(), now);

            context.Shows.Add(show);
            context.Tickets.AddRange(reservedTicket1, reservedTicket2);
            await context.SaveChangesAsync();

            showId = show.Id;
        }

        // Act & Assert
        using (IServiceScope scope = _scopeFactory.CreateScope())
        {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            GetAvailableTicketsQuery query = new(showId);

            List<TicketResponse> result = await mediator.Send(query);

            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }

    [Fact(DisplayName = "Should return empty list when show does not exist")]
    public async Task Handle_ShouldReturnEmptyList_WhenShowDoesNotExist()
    {
        // Arrange
        Guid nonExistentShowId = Guid.NewGuid();

        using IServiceScope scope = _scopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        GetAvailableTicketsQuery query = new(nonExistentShowId);

        // Act
        List<TicketResponse> result = await mediator.Send(query);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
}