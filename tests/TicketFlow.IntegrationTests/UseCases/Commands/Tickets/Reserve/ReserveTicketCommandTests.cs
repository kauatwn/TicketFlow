using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TicketFlow.Application.UseCases.Commands.Tickets.Reserve;
using TicketFlow.Domain.Entities;
using TicketFlow.Domain.Enums;
using TicketFlow.Domain.Exceptions;
using TicketFlow.Domain.ValueObjects;
using TicketFlow.Infrastructure.Persistence;
using TicketFlow.IntegrationTests.Abstractions;

namespace TicketFlow.IntegrationTests.UseCases.Commands.Tickets.Reserve;

[Collection("IntegrationTests")]
[Trait("Category", "Integration")]
public class ReserveTicketCommandTests(IntegrationTestWebAppFactory factory)
{
    private readonly IServiceScopeFactory _scopeFactory = factory.Services.GetRequiredService<IServiceScopeFactory>();

    [Fact(DisplayName = "Should reserve ticket successfully when request is valid")]
    public async Task Handle_ShouldReserveTicket_WhenRequestIsValid()
    {
        // Arrage
        Guid ticketId;
        Guid customerId = Guid.NewGuid();

        DateTime now = DateTime.UtcNow;

        using (IServiceScope scope = _scopeFactory.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<TicketFlowDbContext>();

            Show show = new(title: "Coldplay Live", date: now.AddDays(30), maxTicketsPerUser: 2, currentDate: now);
            Ticket ticket = new(show.Id, new Seat(Sector: "VIP", Row: "A", Number: "1"), price: 500m, createdDate: now);

            context.Shows.Add(show);
            context.Tickets.Add(ticket);
            await context.SaveChangesAsync();

            ticketId = ticket.Id;
        }

        // Act
        using (IServiceScope scope = _scopeFactory.CreateScope())
        {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            ReserveTicketCommand command = new(ticketId, customerId);

            await mediator.Send(command);
        }

        // Assert
        using (IServiceScope scope = _scopeFactory.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<TicketFlowDbContext>();
            Ticket? ticket = await context.Tickets.FindAsync(ticketId);

            Assert.NotNull(ticket);
            Assert.Equal(TicketStatus.Reserved, ticket.Status);
            Assert.Equal(customerId, ticket.CustomerId);
            Assert.NotNull(ticket.ReservedAt);
        }
    }

    [Fact(DisplayName = "Should throw NotFoundException when ticket does not exist")]
    public async Task Handle_ShouldThrowNotFoundException_WhenTicketDoesNotExist()
    {
        // Arrange
        Guid nonExistentTicketId = Guid.NewGuid();
        Guid customerId = Guid.NewGuid();

        // Act
        using IServiceScope scope = _scopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var command = new ReserveTicketCommand(nonExistentTicketId, customerId);

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(() => mediator.Send(command));
    }

    [Fact(DisplayName = "Should throw DomainConflictException when ticket is already reserved")]
    public async Task Handle_ShouldThrowDomainConflictException_WhenTicketIsAlreadyReserved()
    {
        // Arrange
        Guid ticketId;
        Guid userA = Guid.NewGuid();
        Guid userB = Guid.NewGuid();

        DateTime now = DateTime.UtcNow;

        using (IServiceScope scope = _scopeFactory.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<TicketFlowDbContext>();

            Show show = new(title: "U2 Tour", date: now.AddMonths(2), maxTicketsPerUser: 5, currentDate: now);
            Ticket ticket = new(show.Id, new Seat(Sector: "VIP", Row: "B", Number: "10"), price: 200m, createdDate: now);

            ticket.Reserve(userA, currentDate: now);

            context.Shows.Add(show);
            context.Tickets.Add(ticket);
            await context.SaveChangesAsync();

            ticketId = ticket.Id;
        }

        using IServiceScope actScope = _scopeFactory.CreateScope();
        var mediator = actScope.ServiceProvider.GetRequiredService<IMediator>();
        var command = new ReserveTicketCommand(ticketId, userB);

        // Act
        async Task Act() => await mediator.Send(command);

        // Assert
        await Assert.ThrowsAsync<DomainConflictException>(Act);
    }
}