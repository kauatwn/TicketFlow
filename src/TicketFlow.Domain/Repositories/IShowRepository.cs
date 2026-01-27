using TicketFlow.Domain.Entities;

namespace TicketFlow.Domain.Repositories;

public interface IShowRepository
{
    void Add(Show show);
    Task<Show?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}