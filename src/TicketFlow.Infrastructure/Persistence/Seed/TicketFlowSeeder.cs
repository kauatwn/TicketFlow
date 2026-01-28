using System.Diagnostics.CodeAnalysis;
using TicketFlow.Domain.Entities;
using TicketFlow.Domain.ValueObjects;

namespace TicketFlow.Infrastructure.Persistence.Seed;

[ExcludeFromCodeCoverage(Justification = "Data seeding configuration for testing purposes")]
public static class TicketFlowSeeder
{
    public static readonly Guid FixedShowId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public static async Task SeedAsync(TicketFlowDbContext context)
    {
        if (context.Shows.Any())
        {
            return;
        }

        Show show = new(
            title: "Rock in Rio 2026",
            date: DateTime.UtcNow.AddMonths(6),
            maxTicketsPerUser: 4,
            currentDate: DateTime.UtcNow);

        // Uses Reflection to set the private Id for deterministic seeding
        typeof(Show)
            .GetProperty(nameof(Show.Id))?
            .SetValue(show, FixedShowId);

        context.Shows.Add(show);

        List<Ticket> tickets = [];
        for (int i = 1; i <= 10; i++)
        {
            Seat seat = new(Sector: "VIP", Row: "A", Number: i.ToString());
            tickets.Add(new Ticket(show.Id, seat, price: 500m, createdDate: DateTime.UtcNow));
        }

        context.Tickets.AddRange(tickets);

        await context.SaveChangesAsync();
    }
}