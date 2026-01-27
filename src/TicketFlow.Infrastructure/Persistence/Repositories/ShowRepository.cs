using Microsoft.EntityFrameworkCore;
using TicketFlow.Domain.Entities;
using TicketFlow.Domain.Repositories;

namespace TicketFlow.Infrastructure.Persistence.Repositories;

public class ShowRepository(TicketFlowDbContext context) : IShowRepository
{
    public void Add(Show show)
    {
        context.Shows.Add(show);
    }

    public async Task<Show?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await context.Shows.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }
}