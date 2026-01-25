namespace TicketFlow.Domain.Exceptions;

public class ConcurrencyException(string message) : Exception(message);