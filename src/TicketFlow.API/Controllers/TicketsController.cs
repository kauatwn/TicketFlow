using MediatR;
using Microsoft.AspNetCore.Mvc;
using TicketFlow.API.Contracts.Tickets;
using TicketFlow.Application.UseCases.Commands.Tickets.Reserve;

namespace TicketFlow.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TicketsController(IMediator mediator) : ControllerBase
{
    [HttpPost("{id:guid}/reserve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Reserve(Guid id, ReserveTicketRequest request)
    {
        ReserveTicketCommand command = new(id, request.CustomerId);
        await mediator.Send(command);

        return NoContent();
    }
}