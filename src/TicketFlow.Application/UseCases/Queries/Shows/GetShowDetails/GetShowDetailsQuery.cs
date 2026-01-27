using MediatR;

namespace TicketFlow.Application.UseCases.Queries.Shows.GetShowDetails;

public record GetShowDetailsQuery(Guid ShowId) : IRequest<ShowDetailsResponse?>;