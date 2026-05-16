---
name: aggregate-tdd
description: Red-green-refactor workflow for building a new DDD aggregate with value objects, domain events, and xUnit tests following ChatRooms conventions. Use when adding a new aggregate root to the domain model.
---

# Aggregate TDD

Build a new aggregate in 6 phases following red-green-refactor.

## Phase 1: Value Objects (red-green-refactor per VO)

Write tests first for each value object:

```csharp
public sealed class {Name}ValueObjectTests
{
    [Fact]
    public void {Name}_Create_ShouldInitializeCorrectly_ForValidValues() { /* Arrange + Act + Assert */ }

    [Fact]
    public void {Name}_Create_ShouldThrowException_ForInvalidValues() { /* Assert.Throws */ }

    [Fact]
    public void {Name}_Equality_ShouldWorkCorrectly() { /* Assert.Equal */ }
}
```

Then implement the `readonly record struct` with validation.

## Phase 2: Domain Events

Define event records — one per state change:

```csharp
public sealed record {Name}CreatedDomainEvent(
    {Name}Id Id,
    // Value object properties
    DateTimeUtc OccurredAt
    ) : DomainEvent(OccurredAt);

public sealed record {Name}DeletedDomainEvent(
    {Name}Id Id,
    DeletionReason DeletionReason,
    DateTimeUtc OccurredAt
    ) : DomainEvent(OccurredAt);
```

## Phase 3: Aggregate + Apply switch (TDD)

Implement the aggregate following the existing pattern. Write tests for:

1. **Creation** — factory method raises event, properties initialized
2. **State transitions** — each behavior method raises correct event
3. **Guard clauses** — invalid state transitions throw `InvalidOperationException`
4. **No-op detection** — no event raised when state wouldn't change
5. **Event replay** — `Apply()` dispatches correctly
6. **Equality** — same entity type + same ID → equal

Test pattern from the project:

```csharp
public sealed class {Name}Tests
{
    [Fact]
    public void Create{Name}_ShouldRaise{Name}CreatedDomainEvent()
    {
        var name = {Name}Name.From("TestName");
        var {entity} = {Name}.Create(name, DateTimeUtc.FromUtc(DateTime.UtcNow));

        var domainEvents = {entity}.DomainEvents;
        Assert.Single(domainEvents);
        var createdEvent = Assert.IsType<{Name}CreatedDomainEvent>(domainEvents.First());
        Assert.Equal(name, createdEvent.Name);
    }

    [Fact]
    public void {Name}_{Method}_ShouldThrowError_When{State}()
    {
        // Arrange
        var {entity} = {Name}.Create(...)
        {entity}.{TransitionToInvalidState}();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => {entity}.{Method}(...));
    }
}
```

## Phase 4: EF Core Configuration

```csharp
public sealed class {Name}Configuration : IEntityTypeConfiguration<{Name}>
{
    public void Configure(EntityTypeBuilder<{Name}> builder)
    {
        builder.ToTable("{Names}");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasConversion(id => id.Value, value => {Name}Id.From(value));
        // Value object conversions...
        builder.Property(e => e.Version).IsConcurrencyToken();
    }
}
```

## Phase 5: Event Projector

Add insert projector in `ChatRooms.Infrastructure.BackgroundJobs.Projectors` + keyed DI registration.

## Phase 6: Domain Tests

Verify final coverage covers: creation, all state transitions, guard clauses, no-ops, equality, unsupported event rejection.

Run tests:
```bash
dotnet test ChatRooms.Domain.Tests
```
