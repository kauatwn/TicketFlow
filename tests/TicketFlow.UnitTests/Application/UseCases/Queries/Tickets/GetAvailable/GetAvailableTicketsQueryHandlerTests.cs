using Moq;
using TicketFlow.Application.UseCases.Queries.Tickets.GetAvailable;

namespace TicketFlow.UnitTests.Application.UseCases.Queries.Tickets.GetAvailable;

[Trait("Category", "Unit")]
public class GetAvailableTicketsQueryHandlerTests
{
    private readonly Mock<ITicketReadRepository> _readRepositoryMock = new();
    private readonly GetAvailableTicketsQueryHandler _sut;

    public GetAvailableTicketsQueryHandlerTests()
    {
        _sut = new GetAvailableTicketsQueryHandler(_readRepositoryMock.Object);
    }

    [Fact(DisplayName = "Should return available tickets for show")]
    public async Task Handle_ShouldReturnAvailableTickets_WhenShowExists()
    {
        // Arrange
        Guid showId = Guid.NewGuid();
        List<TicketResponse> expectedTickets =
        [
            new(Id: Guid.NewGuid(), Sector: "VIP", Row: "A", Number: "1", Price: 500m),
            new(Id: Guid.NewGuid(), Sector: "General", Row: "B", Number: "10", Price: 200m)
        ];

        _readRepositoryMock
            .Setup(r => r.GetAvailableForShowAsync(showId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTickets);

        GetAvailableTicketsQuery query = new(showId);

        // Act
        List<TicketResponse> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedTickets.Count, result.Count);
        Assert.Equal(expectedTickets, result);
        
        _readRepositoryMock.Verify(r => r.GetAvailableForShowAsync(showId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "Should return empty list when no tickets available")]
    public async Task Handle_ShouldReturnEmptyList_WhenNoTicketsAvailable()
    {
        // Arrange
        Guid showId = Guid.NewGuid();
        List<TicketResponse> emptyList = [];

        _readRepositoryMock
            .Setup(r => r.GetAvailableForShowAsync(showId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyList);

        GetAvailableTicketsQuery query = new GetAvailableTicketsQuery(showId);

        // Act
        List<TicketResponse> result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        
        _readRepositoryMock.Verify(r => r.GetAvailableForShowAsync(showId, It.IsAny<CancellationToken>()), Times.Once);
    }
}