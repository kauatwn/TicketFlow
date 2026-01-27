using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TicketFlow.Domain.Entities;
using TicketFlow.Domain.ValueObjects;

namespace TicketFlow.Infrastructure.Persistence.Configurations;

public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Price)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(t => t.RowVersion)
            .IsRowVersion();

        builder.Property(t => t.CreatedAt)
            .IsRequired();
        
        builder.Property(t => t.Status)
            .IsRequired();

        builder.Property<string>("SeatSector")
            .HasColumnName("SeatSector")
            .HasMaxLength(Seat.MaxSectorLength)
            .IsRequired();

        builder.Property<string>("SeatRow")
            .HasColumnName("SeatRow")
            .HasMaxLength(Seat.MaxRowLength)
            .IsRequired();

        builder.Property<string>("SeatNumber")
            .HasColumnName("SeatNumber")
            .HasMaxLength(Seat.MaxNumberLength)
            .IsRequired();

        builder.OwnsOne(t => t.Seat, seat =>
        {
            seat.Property(s => s.Sector)
                .HasColumnName("SeatSector")
                .HasMaxLength(Seat.MaxSectorLength)
                .IsRequired();

            seat.Property(s => s.Row)
                .HasColumnName("SeatRow")
                .HasMaxLength(Seat.MaxRowLength)
                .IsRequired();

            seat.Property(s => s.Number)
                .HasColumnName("SeatNumber")
                .HasMaxLength(Seat.MaxNumberLength)
                .IsRequired();
        });

        builder.Navigation(t => t.Seat)
            .IsRequired();

        builder.HasIndex("ShowId", "SeatSector", "SeatRow", "SeatNumber")
            .IsUnique();

        builder.HasOne<Show>()
            .WithMany()
            .HasForeignKey(t => t.ShowId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}