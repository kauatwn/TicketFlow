using Microsoft.EntityFrameworkCore;
using TicketFlow.Domain.Exceptions;
using TicketFlow.Domain.Repositories;

namespace TicketFlow.Infrastructure.Persistence;

public class UnitOfWork(TicketFlowDbContext context) : IUnitOfWork
{
    public async Task CommitAsync(CancellationToken cancellationToken)
    {
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConcurrencyException("The data was modified by another user while you were trying to save. Please refresh and try again.");
        }
    }
}