namespace TicketFlow.Application.UseCases.Queries.Tickets.GetAvailable;

public interface ITicketReadRepository
{
    Task<List<TicketResponse>> GetAvailableForShowAsync(Guid showId, CancellationToken cancellationToken);
}