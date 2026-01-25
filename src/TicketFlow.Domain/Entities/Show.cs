using TicketFlow.Domain.Common;
using TicketFlow.Domain.Enums;
using TicketFlow.Domain.Exceptions;

namespace TicketFlow.Domain.Entities;

public sealed class Show : IAggregateRoot
{
    public const string TitleCannotBeEmpty = "Show title cannot be empty.";
    public const string DateIsRequired = "Show date is required.";
    public const string DateMustBeFuture = "Show date must be in the future.";
    public const string MaxTicketsMustBePositive = "Max tickets per user must be greater than zero.";

    public Guid Id { get; private set; }
    public string Title { get; private set; }
    public DateTime Date { get; private set; }
    public int MaxTicketsPerUser { get; private set; }
    public ShowStatus Status { get; private set; }

    public DateTime CreatedAt { get; private set; }

    private Show() { }

    public Show(string title, DateTime date, int maxTicketsPerUser, DateTime currentDate)
    {
        ValidateDomain(title, date, maxTicketsPerUser, currentDate);

        Id = Guid.NewGuid();
        Title = title;
        Date = date;
        MaxTicketsPerUser = maxTicketsPerUser;
        CreatedAt = currentDate;
        Status = ShowStatus.Published;
    }

    public bool CanSellTickets(DateTime currentDate)
    {
        return Status == ShowStatus.Published && Date > currentDate;
    }

    public void Cancel()
    {
        if (Status == ShowStatus.Finished)
        {
            throw new DomainConflictException("Cannot cancel a show that has already finished.");
        }

        Status = ShowStatus.Cancelled;
    }

    private static void ValidateDomain(string title, DateTime date, int maxTicketsPerUser, DateTime currentDate)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainException(TitleCannotBeEmpty);
        }

        if (date == default)
        {
            throw new DomainException(DateIsRequired);
        }

        if (date < currentDate)
        {
            throw new DomainException(DateMustBeFuture);
        }

        if (maxTicketsPerUser <= 0)
        {
            throw new DomainException(MaxTicketsMustBePositive);
        }
    }
}