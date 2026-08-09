using ChatRooms.Domain.RoomParticipants;
using ChatRooms.Domain.RoomParticipants.ValueObjects;
using ChatRooms.Domain.Rooms.ValueObjects;
using ChatRooms.Domain.Shared;
using ChatRooms.Domain.Users.ValueObjects;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace ChatRooms.Infrastructure.Persistence.Configurations;

public sealed class RoomParticipantConfiguration : IEntityTypeConfiguration<RoomParticipant>
{
    public void Configure(EntityTypeBuilder<RoomParticipant> builder)
    {
        builder.ToTable("RoomParticipants");

        builder.HasKey(rp => rp.Id);

        builder.Property(r => r.Id)
                .HasConversion(
                     id => id.Value,
                            value => RoomParticipantId.From(value));

        builder.Property(rp => rp.RoomId)
            .IsRequired()
            .HasConversion(
                id => id.Value,
                value => RoomId.From(value));

        builder.Property(rp => rp.UserId)
            .IsRequired()
            .HasConversion(
                id => id.Value,
                value => UserId.From(value));

        builder.Property(rp => rp.JoinedAt)
            .HasConversion(
                utc => utc.DateTime,
                value => DateTimeUtc.FromUtc(value))
            .IsRequired();

        builder.Property(rp => rp.LeftAt)
            .HasConversion(
                utc => utc.HasValue ? utc.Value.DateTime : (DateTime?)null,
                value => value.HasValue ? DateTimeUtc.FromUtc(value.Value) : null)
            .IsRequired(false);

        builder.Property(r => r.CreatedAt)
            .HasConversion(
                utc => utc.DateTime,
                value => DateTimeUtc.FromUtc(value))
            .IsRequired();

        builder.Property(r => r.UpdatedAt)
            .HasConversion(
                utc => utc.HasValue ? utc.Value.DateTime : (DateTime?)null,
                value => value.HasValue ? DateTimeUtc.FromUtc(value.Value) : null);


        builder.Property(r => r.DeletedAt)
            .HasConversion(
                utc => utc.HasValue ? utc.Value.DateTime : (DateTime?)null,
                value => value.HasValue ? DateTimeUtc.FromUtc(value.Value) : null);

        builder.Property(r => r.Reason)
            .HasConversion<string>();

        builder.Property(rp => rp.Version)
            .IsConcurrencyToken()
            .IsRequired();

        builder.HasIndex(rp => new { rp.RoomId, rp.UserId })
            .IsUnique();

    }
}
