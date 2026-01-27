namespace TicketFlow.Application.UseCases.Queries.Shows.GetShowDetails;

public record ShowDetailsResponse(
    Guid Id,
    string Title,
    DateTime Date,
    string Status,
    int TotalTickets,
    int AvailableTickets);