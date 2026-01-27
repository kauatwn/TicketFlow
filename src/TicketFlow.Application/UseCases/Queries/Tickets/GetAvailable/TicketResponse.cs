namespace TicketFlow.Application.UseCases.Queries.Tickets.GetAvailable;

public record TicketResponse(Guid Id, string Sector, string Row, string Number, decimal Price);