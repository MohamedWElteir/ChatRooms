---
name: cqrs-command-scaffold
description: Scaffold a CQRS Command (record + handler + FluentValidation validator) following ChatRooms conventions. Use when adding new write-side MediatR handlers.
---

# CQRS Command Scaffold

Each command has three files in `ChatRooms.Application.{Entity}.Commands.{Action}`:

## File 1: Command record

```csharp
using ChatRooms.Application.Abstractions.Messaging;
using ChatRooms.DTOs.{Entity}s;

namespace ChatRooms.Application.{Entity}.Commands.{Action};

public sealed record {Action}Command(
    // Primitives only — no domain value objects
    ) : ICommand<{TResult}>;
```

## File 2: Handler

```csharp
using ChatRooms.Application.Abstractions.Common;
using ChatRooms.Application.Abstractions.Messaging;
using ChatRooms.Application.Abstractions.Persistence;
using ChatRooms.Application.Abstractions.Time;
using ChatRooms.Domain.{Entity};
using ChatRooms.Domain.{Entity}.ValueObjects;
using ChatRooms.Domain.Shared;
using ChatRooms.DTOs.{Entity}s;

namespace ChatRooms.Application.{Entity}.Commands.{Action};

public class {Action}CommandHandler(
    I{Entity}Repository {entity}Repository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<{Action}Command, {TResult}>
{
    public async Task<{TResult}> Handle({Action}Command command, CancellationToken cancellationToken)
    {
        var {entity} = await {entity}Repository.GetById(..., cancellationToken)
            ?? throw new KeyNotFoundException(nameof({Entity}));

        {entity}.{ActionMethod}(..., DateTimeUtc.FromUtc(dateTimeProvider.UtcNow));
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new {Entity}Dto(...);
    }
}
```

## File 3: Validator

```csharp
using ChatRooms.Domain.{Entity}.ValueObjects;
using FluentValidation;

namespace ChatRooms.Application.{Entity}.Commands.{Action};

public sealed class {Action}CommandValidator : AbstractValidator<{Action}Command>
{
    public {Action}CommandValidator()
    {
        RuleFor(x => x.{Property})
            .NotEmpty().WithMessage("{Property} is required.")
            .MaximumLength({Name}.MaxLength)
                .WithMessage($"{{Property}} cannot exceed {Name}.MaxLength characters.");
    }
}
```

## Examples

### CreateRoomCommand (from project)
```csharp
// Command
public sealed record CreateRoomCommand(string Name, int Capacity) : ICommand<RoomDto>;

// Handler
public class CreateRoomCommandHandler(
    IRoomRepository roomRepository,
    IUnitOfWork unitOfWork,
    IGenerator<RoomCode> codeGenerator,
    IDateTimeProvider dateTimeProvider
) : ICommandHandler<CreateRoomCommand, RoomDto>
{
    public async Task<RoomDto> Handle(CreateRoomCommand command, CancellationToken ct)
    {
        var room = Room.Create(
            name: Name.From(command.Name),
            capacity: Capacity.From(command.Capacity),
            roomCode: codeGenerator.Generate(),
            dateTime: DateTimeUtc.FromUtc(dateTimeProvider.UtcNow));

        await roomRepository.Add(room, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return new RoomDto(
            Id: room.Id, Name: room.Name, Code: room.Code,
            Capacity: room.Capacity, CurrentParticipantsCount: room.CurrentParticipantsCount,
            Status: room.Status.ToString(), Version: room.Version);
    }
}

// Validator
public sealed class CreateRoomCommandValidator : AbstractValidator<CreateRoomCommand>
{
    public CreateRoomCommandValidator(IRoomCapacityPolicy roomCapacityPolicy)
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Room name is required.")
            .MaximumLength(Name.MaxLength)
                .WithMessage($"Room name cannot exceed {Name.MaxLength} characters.")
            .Must(name => char.IsLetter(name[0])).WithMessage("Room name must start with a letter.");
        RuleFor(x => x.Capacity)
            .GreaterThanOrEqualTo(roomCapacityPolicy.MinCapacity)
            .LessThanOrEqualTo(roomCapacityPolicy.MaxCapacity);
    }
}
```
