using MediatR;

namespace TicketFlow.Application.UseCases.Queries.Tickets.GetAvailable;

public class GetAvailableTicketsQueryHandler(ITicketReadRepository readRepository)
    : IRequestHandler<GetAvailableTicketsQuery, List<TicketResponse>>
{
    public async Task<List<TicketResponse>> Handle(
        GetAvailableTicketsQuery request,
        CancellationToken cancellationToken)
    {
        return await readRepository.GetAvailableForShowAsync(request.ShowId, cancellationToken);
    }
}