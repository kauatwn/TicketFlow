using TicketFlow.Domain.Entities;
using TicketFlow.Domain.Enums;
using TicketFlow.Domain.Exceptions;

namespace TicketFlow.UnitTests.Domain.Entities;

[Trait("Category", "Unit")]
public class ShowTests
{
    private readonly DateTime _now = new(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    [Fact(DisplayName = "Constructor should initialize correctly when data is valid")]
    public void Constructor_ShouldInitializeCorrectly_WhenDataIsValid()
    {
        // Arrange
        const string title = "Coldplay Live";
        DateTime futureDate = DateTime.UtcNow.AddMonths(6);
        const int maxTickets = 4;

        // Act
        Show show = new(title, futureDate, maxTickets, _now);

        // Assert
        Assert.NotEqual(Guid.Empty, show.Id);
        Assert.Equal(title, show.Title);
        Assert.Equal(futureDate, show.Date);
        Assert.Equal(maxTickets, show.MaxTicketsPerUser);
        Assert.Equal(_now, show.CreatedAt);
    }

    [Fact(DisplayName = "Constructor should throw exception when Title is empty")]
    public void Constructor_ShouldThrowDomainException_WhenTitleIsEmpty()
    {
        // Arrange
        DateTime futureDate = DateTime.UtcNow.AddMonths(1);

        // Act
        void Act() => _ = new Show(title: string.Empty, date: futureDate, maxTicketsPerUser: 5, currentDate: _now);

        // Assert
        var exception = Assert.Throws<DomainException>(Act);
        Assert.Equal(Show.TitleCannotBeEmpty, exception.Message);
    }

    [Fact(DisplayName = "Constructor should throw exception when Date is in the past")]
    public void Constructor_ShouldThrowDomainException_WhenDateIsPast()
    {
        // Arrange
        DateTime pastDate = _now.AddDays(-1);

        // Act
        void Act() => _ = new Show(title: "Rock Festival", date: pastDate, maxTicketsPerUser: 5, currentDate: _now);

        // Assert
        var exception = Assert.Throws<DomainException>(Act);
        Assert.Equal(Show.DateMustBeFuture, exception.Message);
    }

    [Fact(DisplayName = "Constructor should throw exception when Date is default")]
    public void Constructor_ShouldThrowDomainException_WhenDateIsDefault()
    {
        // Arrange
        DateTime defaultDate = default;

        // Act
        void Act() => _ = new Show(title: "Rock Festival", date: defaultDate, maxTicketsPerUser: 5, currentDate: _now);

        // Assert
        var exception = Assert.Throws<DomainException>(Act);
        Assert.Equal(Show.DateIsRequired, exception.Message);
    }

    [Theory(DisplayName = "Constructor should throw exception when MaxTicketsPerUser is zero or negative")]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_ShouldThrowDomainException_WhenMaxTicketsIsInvalid(int invalidMaxTickets)
    {
        // Arrange
        DateTime futureDate = DateTime.UtcNow.AddMonths(1);

        // Act
        void Act() => _ = new Show(title: "Rock Festival", futureDate, invalidMaxTickets, currentDate: _now);

        // Assert
        var exception = Assert.Throws<DomainException>(Act);
        Assert.Equal(Show.MaxTicketsMustBePositive, exception.Message);
    }

    [Fact(DisplayName = "Constructor should set Status to Published by default")]
    public void Constructor_ShouldSetStatusToPublished_WhenCreated()
    {
        // Arrange
        const string title = "Coldplay";
        DateTime futureDate = _now.AddMonths(1);
        const int maxTickets = 10;

        // Act
        Show show = new(title, futureDate, maxTickets, currentDate: _now);

        // Assert
        Assert.Equal(ShowStatus.Published, show.Status);
    }

    [Fact(DisplayName = "CanSellTickets should return True when show is Published and in the Future")]
    public void CanSellTickets_ShouldReturnTrue_WhenConditionsAreMet()
    {
        // Arrange
        DateTime showDate = _now.AddDays(10);
        Show show = new(title: "Valid Show", date: showDate, maxTicketsPerUser: 5, currentDate: _now);

        // Act
        bool result = show.CanSellTickets(_now);

        // Assert
        Assert.True(result);
    }

    [Fact(DisplayName = "CanSellTickets should return False when ReferenceDate is AFTER Show Date (Past Show)")]
    public void CanSellTickets_ShouldReturnFalse_WhenShowDateHasPassed()
    {
        // Arrange
        DateTime showDate = _now.AddDays(10);
        Show show = new(title: "Past Show Simulation", date: showDate, maxTicketsPerUser: 5, currentDate: _now);

        DateTime futureReferenceDate = showDate.AddDays(1);

        // Act
        bool result = show.CanSellTickets(futureReferenceDate);

        // Assert
        Assert.False(result);
    }

    [Fact(DisplayName = "CanSellTickets should return False when show is Cancelled")]
    public void CanSellTickets_ShouldReturnFalse_WhenShowIsCancelled()
    {
        // Arrange
        DateTime showDate = _now.AddDays(10);
        Show show = new(title: "Valid Show", date: showDate, maxTicketsPerUser: 5, currentDate: _now);
        show.Cancel();

        // Act
        bool result = show.CanSellTickets(_now);

        // Assert
        Assert.False(result);
    }

    [Fact(DisplayName = "Cancel should change status to Cancelled")]
    public void Cancel_ShouldChangeStatusToCancelled_WhenShowIsValid()
    {
        // Arrange
        DateTime showDate = _now.AddDays(10);
        Show show = new(title: "Past Show Simulation", date: showDate, maxTicketsPerUser: 5, _now);

        // Act
        show.Cancel();

        // Assert
        Assert.Equal(ShowStatus.Cancelled, show.Status);
    }

    [Fact(DisplayName = "Cancel should throw DomainConflictException when show is Finished")]
    public void Cancel_ShouldThrowException_WhenShowIsFinished()
    {
        // Arrange
        Show show = new(title: "Finished Show", date: _now.AddDays(1), maxTicketsPerUser: 10, currentDate: _now);

        show.Finish(_now.AddDays(2));

        // Act
        void Act() => show.Cancel();

        // Assert
        var exception = Assert.Throws<DomainConflictException>(Act);
        Assert.Equal(Show.CannotCancelFinishedShow, exception.Message);
    }

    [Fact(DisplayName = "Finish should update status to Finished when date is valid")]
    public void Finish_ShouldUpdateStatusToFinished_WhenDateIsValid()
    {
        // Arrange
        DateTime showDate = _now.AddDays(10);
        Show show = new(title: "Future Show", date: showDate, maxTicketsPerUser: 5, currentDate: _now);

        DateTime validFinishDate = showDate.AddDays(1);

        // Act
        show.Finish(validFinishDate);

        // Assert
        Assert.Equal(ShowStatus.Finished, show.Status);
    }

    [Fact(DisplayName = "Finish should throw DomainException when currentDate is before Show Date")]
    public void Finish_ShouldThrowDomainException_WhenDateIsBeforeShowDate()
    {
        // Arrange
        DateTime showDate = _now.AddDays(10);
        Show show = new(title: "Future Show", date: showDate, maxTicketsPerUser: 5, currentDate: _now);

        DateTime invalidFinishDate = showDate.AddDays(-1);

        // Act
        void Act() => show.Finish(invalidFinishDate);

        // Assert
        var exception = Assert.Throws<DomainException>(Act);
        Assert.Equal(Show.CannotFinishBeforeDate, exception.Message);
    }
}