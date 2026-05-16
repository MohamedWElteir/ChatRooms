using ChatRooms.Domain.Shared;
using ChatRooms.Domain.Users;
using ChatRooms.Domain.Users.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatRooms.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasConversion(
                userId => userId.Value,
                value => UserId.From(value));

        builder.Property(u => u.Name)
            .HasConversion(
                name => name.Value,
                value => ChatRooms.Domain.Users.ValueObjects.Name.From(value))
            .HasMaxLength(15)
            .IsRequired();

        builder.Property(u => u.Email)
            .HasConversion(
                email => email.Value,
                value => Email.From(value))
            .HasMaxLength(254)
            .IsRequired();

        builder.Property(u => u.Gender)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(u => u.BirthDate)
            .HasConversion(
                bd => bd.Value,
                value => BirthDate.From(value))
            .IsRequired();

        builder.Property(u => u.CreatedAt)
            .HasConversion(
                utc => utc.DateTime,
                value => DateTimeUtc.FromUtc(value))
            .IsRequired();

        builder.Property(u => u.UpdatedAt)
            .HasConversion(
                utc => utc.HasValue ? utc.Value.DateTime : (DateTime?)null,
                value => value.HasValue ? DateTimeUtc.FromUtc(value.Value) : null);

        builder.Property(u => u.DeletedAt)
            .HasConversion(
                utc => utc.HasValue ? utc.Value.DateTime : (DateTime?)null,
                value => value.HasValue ? DateTimeUtc.FromUtc(value.Value) : null);

        builder.Property(u => u.Reason)
            .HasConversion<string>();

        builder.Property(u => u.Version)
            .HasColumnType("integer")
            .IsConcurrencyToken();

        builder.Ignore(u => u.Age);

        builder.HasIndex(u => u.Email)
            .IsUnique();
    }
}
