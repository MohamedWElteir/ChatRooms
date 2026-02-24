using ChatRooms.Domain.Rooms;
using ChatRooms.Domain.Rooms.ValueObjects;
using ChatRooms.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatRooms.Infrastructure.Persistence.Configurations;

public sealed class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.ToTable("Rooms");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasConversion(
                roomId => roomId.Value,
                value => RoomId.From(value));

        builder.Property(r => r.Name)
            .HasConversion(
                name => name.Value,
                value => Name.From(value))
            .HasMaxLength(50)
            .IsRequired();
        builder.Property(r => r.Status)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(r => r.Capacity)
            .HasConversion(
                capacity => capacity.Value,
                value => Capacity.From(value))
            .IsRequired();

        builder.Property(r => r.Code)
            .HasConversion(
                code => code.Value,
                value => RoomCode.From(value))
            .HasMaxLength(8)
            .IsRequired();

        builder.Property(r => r.CreatedAt)
            .HasConversion(
                utc => utc.DateTime,
                value => DateTimeUtc.FromUtc(value))
            .IsRequired();

        builder.Property(r => r.UpdatedAt)
            .HasConversion(
                utc => utc.HasValue ? utc.Value.DateTime : (DateTime?)null,
                value => value.HasValue ? DateTimeUtc.FromUtc(value.Value) : null);

        builder.Property(r => r.IsDeleted)
            .IsRequired();

        builder.HasIndex(r => r.Code)
            .IsUnique();
    }
}
