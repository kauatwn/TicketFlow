using MediatR;

namespace TicketFlow.Application.UseCases.Queries.Shows.GetShowDetails;

public class GetShowDetailsQueryHandler(IShowReadRepository readRepository) 
    : IRequestHandler<GetShowDetailsQuery, ShowDetailsResponse?>
{
    public async Task<ShowDetailsResponse?> Handle(GetShowDetailsQuery request, CancellationToken cancellationToken)
    {
        return await readRepository.GetDetailsAsync(request.ShowId, cancellationToken);
    }
}