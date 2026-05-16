using ChatRooms.Domain.Shared;
using ChatRooms.Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatRooms.Infrastructure.Persistence.Configurations;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Type)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(o => o.Content)
            .HasColumnType("jsonb")
            .IsRequired();


        builder.Property(o => o.ErrorMessage)
            .HasMaxLength(2000);


        builder.Property(o => o.OccurredOn)
            .HasConversion(
                utc => utc.DateTime,
                value => DateTimeUtc.FromUtc(value))
            .IsRequired();

        builder.Property(o => o.ProcessedOn)
            .HasConversion(
                utc => utc.HasValue ? utc.Value.DateTime : (DateTime?)null,
                value => value.HasValue ? DateTimeUtc.FromUtc(value.Value) : null);

        builder.Property(o => o.RetryCount)
            .IsRequired();

        builder.Property(o => o.IsProcessed)
            .IsRequired();

        builder.HasIndex(o => new { o.IsProcessed, o.IsDeadLetter, o.OccurredOn });
    }
}