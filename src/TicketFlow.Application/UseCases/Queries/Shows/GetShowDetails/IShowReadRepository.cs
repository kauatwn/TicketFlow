namespace TicketFlow.Application.UseCases.Queries.Shows.GetShowDetails;

public interface IShowReadRepository
{
    Task<ShowDetailsResponse?> GetDetailsAsync(Guid showId, CancellationToken cancellationToken);
}