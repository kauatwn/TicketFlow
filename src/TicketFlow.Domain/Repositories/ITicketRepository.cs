using TicketFlow.Domain.Entities;

namespace TicketFlow.Domain.Repositories;

public interface ITicketRepository
{
    Task<Ticket?> GetByIdWithShowAsync(Guid id, CancellationToken cancellationToken);
    void Update(Ticket ticket);
}