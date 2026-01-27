using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TicketFlow.Domain.Entities;

namespace TicketFlow.Infrastructure.Persistence.Configurations;

public class ShowConfiguration : IEntityTypeConfiguration<Show>
{
    public void Configure(EntityTypeBuilder<Show> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Title)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(s => s.Date)
            .IsRequired();

        builder.Property(s => s.MaxTicketsPerUser)
            .IsRequired();

        builder.Property(s => s.CreatedAt)
            .IsRequired();
        
        builder.Property(s => s.Status)
            .IsRequired();

        builder.HasIndex(s => s.Date);
    }
}