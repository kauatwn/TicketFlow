namespace TicketFlow.Domain.Exceptions;

public class DomainConflictException(string message) : DomainException(message);