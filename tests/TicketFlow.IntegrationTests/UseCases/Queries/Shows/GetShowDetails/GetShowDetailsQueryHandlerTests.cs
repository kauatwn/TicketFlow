using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TicketFlow.Application.UseCases.Queries.Shows.GetShowDetails;
using TicketFlow.Domain.Entities;
using TicketFlow.Domain.ValueObjects;
using TicketFlow.Infrastructure.Persistence;
using TicketFlow.IntegrationTests.Abstractions;

namespace TicketFlow.IntegrationTests.UseCases.Queries.Shows.GetShowDetails;

[Collection("IntegrationTests")]
[Trait("Category", "Integration")]
public class GetShowDetailsQueryHandlerTests(IntegrationTestWebAppFactory factory)
{
    private readonly IServiceScopeFactory _scopeFactory = factory.Services.GetRequiredService<IServiceScopeFactory>();

    [Fact(DisplayName = "Should return correct ticket counts and details")]
    public async Task Handle_ShouldReturnCorrectCounts_WhenShowExists()
    {
        // Arrange
        Guid showId;
        DateTime now = DateTime.UtcNow;

        using (IServiceScope scope = _scopeFactory.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<TicketFlowDbContext>();

            Show show = new(title: "Coldplay - World Tour", date: now.AddMonths(3), maxTicketsPerUser: 5, currentDate: now);

            Ticket ticket1 = new(show.Id, new Seat(Sector: "VIP", Row: "A", Number: "1"), price: 500m, createdDate: now);
            Ticket ticket2 = new(show.Id, new Seat(Sector: "VIP", Row: "A", Number: "2"), price: 500m, createdDate: now);
            ticket2.Reserve(Guid.NewGuid(), currentDate: now);

            context.Shows.Add(show);
            context.Tickets.AddRange(ticket1, ticket2);
            await context.SaveChangesAsync();

            showId = show.Id;
        }

        // Act & Assert
        using (IServiceScope scope = _scopeFactory.CreateScope())
        {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            ShowDetailsResponse? result = await mediator.Send(new GetShowDetailsQuery(showId));

            Assert.NotNull(result);
            Assert.Equal("Coldplay - World Tour", result.Title);
            Assert.Equal(2, result.TotalTickets);
            Assert.Equal(1, result.AvailableTickets);
        }
    }
}