using Microsoft.EntityFrameworkCore;
using TicketFlow.Application.UseCases.Queries.Shows.GetShowDetails;
using TicketFlow.Domain.Enums;

namespace TicketFlow.Infrastructure.Persistence.Repositories;

public class ShowReadRepository(TicketFlowDbContext context) : IShowReadRepository
{
    public async Task<ShowDetailsResponse?> GetDetailsAsync(Guid showId, CancellationToken cancellationToken)
    {

        return await context.Shows.AsNoTracking()
            .Where(s => s.Id == showId)
            .Select(s => new ShowDetailsResponse(
                s.Id,
                s.Title,
                s.Date,
                s.Status.ToString(),
                context.Tickets.Count(t => t.ShowId == s.Id),
                context.Tickets.Count(t => t.ShowId == s.Id && t.Status == TicketStatus.Available)))
            .FirstOrDefaultAsync(cancellationToken);
    }
}