using Microsoft.EntityFrameworkCore;
using TicketFlow.Domain.Entities;

namespace TicketFlow.Infrastructure.Persistence;

public class TicketFlowDbContext(DbContextOptions<TicketFlowDbContext> options) : DbContext(options)
{
    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<Show> Shows { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TicketFlowDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}