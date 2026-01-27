using MediatR;
using Microsoft.AspNetCore.Mvc;
using TicketFlow.Application.UseCases.Queries.Shows.GetShowDetails;
using TicketFlow.Application.UseCases.Queries.Tickets.GetAvailable;

namespace TicketFlow.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ShowsController(IMediator mediator) : ControllerBase
{

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ShowDetailsResponse>> GetShowDetails(Guid id)
    {
        GetShowDetailsQuery query = new(id);
        ShowDetailsResponse? show = await mediator.Send(query);

        if (show is null)
        {
            return NotFound();
        }

        return Ok(show);
    }

    [HttpGet("{id:guid}/tickets")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<TicketResponse>>> GetAvailableTickets(Guid id)
    {
        GetAvailableTicketsQuery query = new(id);
        List<TicketResponse> tickets = await mediator.Send(query);

        return Ok(tickets);
    }
}