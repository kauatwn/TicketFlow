using TicketFlow.Domain.Entities;
using TicketFlow.Domain.Enums;
using TicketFlow.Domain.Exceptions;
using TicketFlow.Domain.ValueObjects;

namespace TicketFlow.UnitTests.Domain.Entities;

[Trait("Category", "Unit")]
public class TicketTests
{
    private readonly DateTime _now = new(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    [Fact(DisplayName = "Constructor should initialize correctly when data is valid")]
    public void Constructor_ShouldInitializeCorrectly_WhenDataIsValid()
    {
        // Arrange
        Guid showId = Guid.NewGuid();
        Seat seat = new(Sector: "A", Row: "1", Number: "10");
        decimal price = 150.00m;

        // Act
        Ticket ticket = new(showId, seat, price, _now);

        // Assert
        Assert.NotEqual(Guid.Empty, ticket.Id);
        Assert.Equal(showId, ticket.ShowId);
        Assert.Equal(seat, ticket.Seat);
        Assert.Equal(price, ticket.Price);
        Assert.Equal(TicketStatus.Available, ticket.Status);
        Assert.Equal(_now, ticket.CreatedAt);
        Assert.Null(ticket.CustomerId);
        Assert.Null(ticket.ReservedAt);
    }

    [Fact(DisplayName = "Constructor should throw exception when ShowId is empty")]
    public void Constructor_ShouldThrowDomainException_WhenShowIdIsEmpty()
    {
        // Arrange
        Seat seat = new(Sector: "A", Row: "1", Number: "10");
        decimal price = 150.00m;

        // Act
        void Act() => _ = new Ticket(Guid.Empty, seat, price, _now);

        // Assert
        var exception = Assert.Throws<DomainException>(Act);
        Assert.Equal(Ticket.ShowIdCannotBeEmpty, exception.Message);
    }

    [Fact(DisplayName = "Constructor should throw exception when Seat is null")]
    public void Constructor_ShouldThrowDomainException_WhenSeatIsNull()
    {
        // Arrange
        Guid showId = Guid.NewGuid();
        decimal price = 150.00m;

        // Act
        void Act() => _ = new Ticket(showId, null!, price, _now);

        // Assert
        var exception = Assert.Throws<DomainException>(Act);
        Assert.Equal(Ticket.SeatCannotBeNull, exception.Message);
    }

    [Theory(DisplayName = "Constructor should throw exception when Price is zero or negative")]
    [InlineData(0)]
    [InlineData(-10.50)]
    public void Constructor_ShouldThrowDomainException_WhenPriceIsInvalid(decimal invalidPrice)
    {
        // Arrange
        Guid showId = Guid.NewGuid();
        Seat seat = new(Sector: "A", Row: "1", Number: "10");

        // Act
        void Act() => _ = new Ticket(showId, seat, invalidPrice, _now);

        // Assert
        var exception = Assert.Throws<DomainException>(Act);
        Assert.Equal(Ticket.PriceMustBePositive, exception.Message);
    }

    [Fact(DisplayName = "Reserve should update status and customer when ticket is available")]
    public void Reserve_ShouldUpdateStatus_WhenTicketIsAvailable()
    {
        // Arrange
        Ticket ticket = new(Guid.NewGuid(), new Seat(Sector: "B", Row: "2", Number: "20"), price: 200.00m, _now);
        var customerId = Guid.NewGuid();

        DateTime reservationTime = _now.AddHours(1);

        // Act
        ticket.Reserve(customerId, reservationTime);

        // Assert
        Assert.Equal(TicketStatus.Reserved, ticket.Status);
        Assert.Equal(customerId, ticket.CustomerId);
        Assert.Equal(reservationTime, ticket.ReservedAt);
    }

    [Fact(DisplayName = "Reserve should throw exception when CustomerId is empty")]
    public void Reserve_ShouldThrowDomainException_WhenCustomerIdIsEmpty()
    {
        // Arrange
        Ticket ticket = new(Guid.NewGuid(), new Seat(Sector: "B", Row: "2", Number: "20"), price: 200.00m, _now);

        // Act
        void Act() => ticket.Reserve(Guid.Empty, _now);

        // Assert
        var exception = Assert.Throws<DomainException>(Act);

        string expectedMessage = string.Format(Ticket.CustomerIdRequired, nameof(Ticket.CustomerId));
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact(DisplayName = "Reserve should throw ConflictException when ticket is already Reserved")]
    public void Reserve_ShouldThrowDomainConflictException_WhenTicketIsAlreadyReserved()
    {
        // Arrange
        Seat seat = new(Sector: "C", Row: "5", Number: "50");
        Ticket ticket = new(Guid.NewGuid(), seat, price: 300.00m, _now);

        ticket.Reserve(Guid.NewGuid(), _now);

        // Act
        void Act() => ticket.Reserve(Guid.NewGuid(), _now.AddMinutes(5));

        // Assert
        var exception = Assert.Throws<DomainConflictException>(Act);

        string expectedMessage = string.Format(Ticket.SeatNotAvailableMessage, seat);
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact(DisplayName = "ConfirmPurchase should update status to Sold when ticket is Reserved")]
    public void ConfirmPurchase_ShouldUpdateStatusToSold_WhenTicketIsReserved()
    {
        // Arrange
        Ticket ticket = new(Guid.NewGuid(), new Seat(Sector: "A", Row: "1", Number: "1"), price: 100.00m, _now);
        ticket.Reserve(Guid.NewGuid(), _now);

        // Act
        ticket.ConfirmPurchase();

        // Assert
        Assert.Equal(TicketStatus.Sold, ticket.Status);
    }

    [Fact(DisplayName = "ConfirmPurchase should throw ConflictException when ticket is Available")]
    public void ConfirmPurchase_ShouldThrowDomainConflictException_WhenTicketIsAvailable()
    {
        // Arrange
        Ticket ticket = new(Guid.NewGuid(), new Seat(Sector: "A", Row: "1", Number: "1"), price: 100.00m, _now);

        // Act
        void Act() => ticket.ConfirmPurchase();

        // Assert
        var exception = Assert.Throws<DomainConflictException>(Act);
        Assert.Equal(Ticket.TicketMustBeReserved, exception.Message);
    }

    [Fact(DisplayName = "Reserve should throw ConflictException when ticket is Sold")]
    public void Reserve_ShouldThrowDomainConflictException_WhenTicketIsSold()
    {
        // Arrange
        Ticket ticket = new (Guid.NewGuid(), new Seat(Sector: "A", Row: "1", Number: "1"), price: 100.00m, _now);
        ticket.Reserve(Guid.NewGuid(), _now);
        ticket.ConfirmPurchase();

        // Act
        void Act() => ticket.Reserve(Guid.NewGuid(), _now);

        // Assert
        var exception = Assert.Throws<DomainConflictException>(Act);

        string expectedMessage = string.Format(Ticket.SeatNotAvailableMessage, ticket.Seat);
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact(DisplayName = "Reserve should ignore duplicate request (Idempotency) when called by same customer")]
    public void Reserve_ShouldBeIdempotent_WhenCalledBySameCustomer()
    {
        // Arrange
        Ticket ticket = new(Guid.NewGuid(), new Seat(Sector: "A", Row: "1", Number: "1"), price: 100m, _now);
        Guid customerId = Guid.NewGuid();

        ticket.Reserve(customerId, _now);

        TicketStatus firstStatus = ticket.Status;
        DateTime? firstDate = ticket.ReservedAt;

        // Act
        DateTime laterTime = _now.AddHours(1);
        ticket.Reserve(customerId, laterTime);

        // Assert
        Assert.Equal(firstStatus, ticket.Status);
        Assert.Equal(customerId, ticket.CustomerId);
        Assert.Equal(firstDate, ticket.ReservedAt);
        Assert.NotEqual(laterTime, ticket.ReservedAt);
    }

    [Fact(DisplayName = "Reserve should throw ConflictException when ticket is reserved by another customer")]
    public void Reserve_ShouldThrowDomainConflictException_WhenTicketIsReservedByAnotherCustomer()
    {
        // Arrange
        Seat seat = new(Sector: "C", Row: "5", Number: "50");
        Ticket ticket = new(Guid.NewGuid(), seat, price: 300.00m, _now);

        Guid userA = Guid.NewGuid();
        Guid userB = Guid.NewGuid();

        ticket.Reserve(userA, _now);

        // Act
        void Act() => ticket.Reserve(userB, _now);

        // Assert
        var exception = Assert.Throws<DomainConflictException>(Act);
        string expectedMessage = string.Format(Ticket.SeatNotAvailableMessage, seat);
        Assert.Equal(expectedMessage, exception.Message);
    }
}