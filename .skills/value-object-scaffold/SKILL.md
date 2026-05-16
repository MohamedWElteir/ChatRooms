---
name: value-object-scaffold
description: Scaffold a new Value Object (readonly record struct) following ChatRooms DDD conventions. Use when creating new domain value objects like RoomId, Capacity, Name, Email, etc.
---

# Value Object Scaffold

Every value object in ChatRooms follows this pattern:

## Template

```csharp
namespace ChatRooms.Domain.{Entity}.ValueObjects;

public readonly record struct {Name}
{
    public {PrimitiveType} Value { get; }

    private {Name}({PrimitiveType} value)
    {
        // Validation guards throwing ArgumentException
        Value = value;
    }

    public static {Name} From({PrimitiveType} value) => new(value);
    public static implicit operator {PrimitiveType}({Name} name) => name.Value;
}
```

## Workflow

1. **Pick the domain folder** — `ChatRooms.Domain.Rooms.ValueObjects` or `ChatRooms.Domain.Users.ValueObjects` (or create a new one).
2. **Define the primitive type** — `Guid`, `string`, `int`, etc.
3. **Add validation guards** in the private constructor. Use `ArgumentException` with `nameof(value)`.
4. **Add constraint constants** if needed (e.g. `MaxLength`, `Min`).
5. **Update EF Core configuration** — add `HasConversion` in the entity's `IEntityTypeConfiguration`.

## Examples

### RoomId (Guid wrapper)
```csharp
namespace ChatRooms.Domain.Rooms.ValueObjects;

public readonly record struct RoomId
{
    public Guid Value { get; }
    private RoomId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("RoomId cannot be empty.", nameof(value));
        Value = value;
    }
    public static RoomId New() => new(Guid.NewGuid());
    public static RoomId From(Guid value) => new(value);
    public static implicit operator Guid(RoomId roomId) => roomId.Value;
}
```

### Capacity (int with range)
```csharp
namespace ChatRooms.Domain.Rooms.ValueObjects;

public readonly record struct Capacity
{
    public int Value { get; }
    public const int Min = 1;
    public const int Max = 100;

    private Capacity(int value)
    {
        if (value < Min)
            throw new ArgumentException($"Capacity must be at least {Min}.", nameof(value));
        if (value > Max)
            throw new ArgumentException($"Capacity cannot exceed {Max}.", nameof(value));
        Value = value;
    }
    public static Capacity From(int value) => new(value);
    public static implicit operator int(Capacity capacity) => capacity.Value;
}
```

### EF Core HasConversion (in RoomConfiguration.cs)
```csharp
builder.Property(r => r.{PropertyName})
    .HasConversion(
        vo => vo.Value,
        value => {Name}.From(value))
    .IsRequired();
```
