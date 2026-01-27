using MediatR;

namespace TicketFlow.Application.UseCases.Commands.Tickets.Reserve;

public record ReserveTicketCommand(Guid TicketId, Guid CustomerId) : IRequest;