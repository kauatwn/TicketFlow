using Microsoft.EntityFrameworkCore;
using TicketFlow.Domain.Entities;
using TicketFlow.Domain.Repositories;

namespace TicketFlow.Infrastructure.Persistence.Repositories;

public class TicketRepository(TicketFlowDbContext context) : ITicketRepository
{
    public async Task<Ticket?> GetByIdWithShowAsync(Guid id, CancellationToken cancellationToken)
    {
        return await context.Tickets.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public void Update(Ticket ticket)
    {
        context.Tickets.Update(ticket);
    }
}