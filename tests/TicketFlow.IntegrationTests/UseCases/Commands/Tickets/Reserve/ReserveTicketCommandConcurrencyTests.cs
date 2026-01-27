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
public class ReserveTicketCommandConcurrencyTests(IntegrationTestWebAppFactory factory)
{
    private readonly IServiceScopeFactory _scopeFactory = factory.Services.GetRequiredService<IServiceScopeFactory>();

    [Fact(DisplayName = "Should prevent double reservation when two requests occur simultaneously")]
    public async Task Handle_ShouldPreventDoubleReservation_WhenConcurrentRequestsOccur()
    {
        // Arrange
        Guid ticketId;
        Guid userA = Guid.NewGuid();
        Guid userB = Guid.NewGuid();

        DateTime now = DateTime.UtcNow;

        using (IServiceScope scope = _scopeFactory.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<TicketFlowDbContext>();

            Show show = new(title: "Rock in Rio", date: now.AddMonths(1), maxTicketsPerUser: 10, currentDate: now);
            Ticket ticket = new(show.Id, new Seat(Sector: "General", Row: "1", Number: "1"), price: 100m, createdDate: now);

            context.Shows.Add(show);
            context.Tickets.Add(ticket);
            await context.SaveChangesAsync();

            ticketId = ticket.Id;
        }

        // Act
        var taskA = Task.Run<(bool Success, Exception? Exception)>(async () =>
        {
            using IServiceScope scope = _scopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            try
            {
                await mediator.Send(new ReserveTicketCommand(ticketId, userA));

                return (Success: true, Exception: null);
            }
            catch (Exception ex)
            {
                return (Success: false, Exception: ex);
            }
        });

        var taskB = Task.Run<(bool Success, Exception? Exception)>(async () =>
        {
            using IServiceScope scope = _scopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            try
            {
                await mediator.Send(new ReserveTicketCommand(ticketId, userB));
                return (Success: true, Exception: null);
            }
            catch (Exception ex)
            {
                return (Success: false, Exception: ex);
            }
        });

        var results = await Task.WhenAll(taskA, taskB);

        // Assert
        int successCount = results.Count(r => r.Success);
        int failureCount = results.Count(r => !r.Success);

        Assert.Equal(1, successCount);
        Assert.Equal(1, failureCount);

        Exception? exception = results.First(r => !r.Success).Exception;

        Assert.True(exception is ConcurrencyException or DomainConflictException,
            $"Unexpected exception caught: {exception.GetType().Name} - {exception.Message}"
        );

        using (IServiceScope scope = _scopeFactory.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<TicketFlowDbContext>();
            Ticket? ticketFinal = await context.Tickets.FindAsync(ticketId);

            Assert.Equal(TicketStatus.Reserved, ticketFinal!.Status);
            Assert.Contains(ticketFinal.CustomerId!.Value, new[] { userA, userB });
        }
    }
}