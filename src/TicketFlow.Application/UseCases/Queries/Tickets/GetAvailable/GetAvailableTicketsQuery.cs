using MediatR;

namespace TicketFlow.Application.UseCases.Queries.Tickets.GetAvailable;

public record GetAvailableTicketsQuery(Guid ShowId) : IRequest<List<TicketResponse>>;