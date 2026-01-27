using Microsoft.EntityFrameworkCore;
using TicketFlow.Application.UseCases.Queries.Tickets.GetAvailable;
using TicketFlow.Domain.Enums;

namespace TicketFlow.Infrastructure.Persistence.Repositories;

public class TicketReadRepository(TicketFlowDbContext context) : ITicketReadRepository
{
    public async Task<List<TicketResponse>> GetAvailableForShowAsync(Guid showId, CancellationToken cancellationToken)
    {
        return await context.Tickets.AsNoTracking()
            .Where(t => t.ShowId == showId && t.Status == TicketStatus.Available)
            .Select(t => new TicketResponse(
                t.Id,
                t.Seat.Sector,
                t.Seat.Row,
                t.Seat.Number,
                t.Price))
            .ToListAsync(cancellationToken);
    }
}