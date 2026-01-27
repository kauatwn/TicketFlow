using MediatR;
using TicketFlow.Domain.Entities;
using TicketFlow.Domain.Exceptions;
using TicketFlow.Domain.Repositories;

namespace TicketFlow.Application.UseCases.Commands.Tickets.Reserve;

public class ReserveTicketCommandHandler(
    ITicketRepository ticketRepository,
    IShowRepository showRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IRequestHandler<ReserveTicketCommand>
{
    public async Task Handle(ReserveTicketCommand request, CancellationToken cancellationToken)
    {
        Ticket ticket = await ticketRepository.GetByIdWithShowAsync(request.TicketId, cancellationToken)
            ?? throw new NotFoundException("Ticket not found.");

        Show show = await showRepository.GetByIdAsync(ticket.ShowId, cancellationToken)
            ?? throw new NotFoundException($"Show with ID '{ticket.ShowId}' not found.");

        DateTime now = timeProvider.GetUtcNow().DateTime;

        if (!show.CanSellTickets(now))
        {
            throw new DomainConflictException("Cannot reserve ticket. The show is unavailable or finished.");
        }

        ticket.Reserve(request.CustomerId, now);

        ticketRepository.Update(ticket);

        await unitOfWork.CommitAsync(cancellationToken);
    }
}