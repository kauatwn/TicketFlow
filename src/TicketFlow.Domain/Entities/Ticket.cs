using TicketFlow.Domain.Common;
using TicketFlow.Domain.Enums;
using TicketFlow.Domain.Exceptions;
using TicketFlow.Domain.ValueObjects;

namespace TicketFlow.Domain.Entities;

public sealed class Ticket : IAggregateRoot
{
    public const string CustomerIdRequired = "A valid '{0}' is required for reservation.";
    public const string TicketMustBeReserved = "Ticket must be reserved before confirmation.";
    public const string ShowIdCannotBeEmpty = "ShowId cannot be empty.";
    public const string SeatCannotBeNull = "Seat cannot be null.";
    public const string PriceMustBePositive = "Price must be greater than zero.";
    public const string SeatNotAvailableMessage = "Seat '{0}' is already reserved or sold.";

    public Guid Id { get; private set; }
    public Guid ShowId { get; private set; }
    public Seat Seat { get; private set; }
    public decimal Price { get; private set; }
    public TicketStatus Status { get; private set; }
    public Guid? CustomerId { get; private set; }
    public DateTime? ReservedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public byte[] RowVersion { get; private set; }

    private Ticket() { }

    public Ticket(Guid showId, Seat seat, decimal price, DateTime createdDate)
    {
        ValidateDomain(showId, seat, price);

        Id = Guid.NewGuid();
        ShowId = showId;
        Seat = seat;
        Price = price;
        Status = TicketStatus.Available;
        CreatedAt = createdDate;
    }

    public void Reserve(Guid customerId, DateTime currentDate)
    {
        if (customerId == Guid.Empty)
        {
            throw new DomainException(string.Format(CustomerIdRequired, nameof(CustomerId)));
        }

        if (Status == TicketStatus.Reserved && CustomerId == customerId)
        {
            return;
        }

        if (Status != TicketStatus.Available)
        {
            throw new DomainConflictException(string.Format(SeatNotAvailableMessage, Seat));
        }

        Status = TicketStatus.Reserved;
        CustomerId = customerId;
        ReservedAt = currentDate;
    }

    public void ConfirmPurchase()
    {
        if (Status != TicketStatus.Reserved)
        {
            throw new DomainConflictException(TicketMustBeReserved);
        }

        Status = TicketStatus.Sold;
    }

    private static void ValidateDomain(Guid showId, Seat seat, decimal price)
    {
        if (showId == Guid.Empty)
        {
            throw new DomainException(ShowIdCannotBeEmpty);
        }

        if (seat is null)
        {
            throw new DomainException(SeatCannotBeNull);
        }

        if (price <= 0)
        {
            throw new DomainException(PriceMustBePositive);
        }
    }
}