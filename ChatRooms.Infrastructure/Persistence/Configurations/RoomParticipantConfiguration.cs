using ChatRooms.Domain.RoomParticipants;
using ChatRooms.Domain.Rooms.ValueObjects;
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
            .IsRequired();

        builder.Property(rp => rp.LeftAt)
            .IsRequired(false);


        builder.HasIndex(rp => new { rp.RoomId, rp.UserId })
            .IsUnique();

    }
}
