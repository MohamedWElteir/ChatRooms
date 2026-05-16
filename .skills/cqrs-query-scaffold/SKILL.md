---
name: cqrs-query-scaffold
description: Scaffold a CQRS Query (record + handler) following ChatRooms conventions. Use when adding new read-side MediatR handlers.
---

# CQRS Query Scaffold

Each query has two files in `ChatRooms.Application.{Entity}.Queries.{Action}`:

## File 1: Query record

```csharp
using ChatRooms.Application.Abstractions.Messaging;
using ChatRooms.DTOs.{Entity}s;

namespace ChatRooms.Application.{Entity}.Queries.{Action};

public sealed record {Action}Query(
    // Primitives only (Guid Id, string Code, etc.)
    ) : IQuery<{TResult}>;
```

## File 2: Handler

```csharp
using ChatRooms.Application.Abstractions.Messaging;
using ChatRooms.Domain.{Entity};
using ChatRooms.DTOs.{Entity}s;

namespace ChatRooms.Application.{Entity}.Queries.{Action};

public sealed class {Action}QueryHandler(I{Entity}Query query)
    : IQueryHandler<{Action}Query, {TResult}>
{
    public async Task<{TResult}> Handle({Action}Query request, CancellationToken cancellationToken)
    {
        var dto = await query.{MethodName}(request.{Param}, cancellationToken)
            ?? throw new KeyNotFoundException(nameof({Entity}));
        return dto;
    }
}
```

## Examples

### GetRoomByIdQuery (from project)
```csharp
// Query
public sealed record GetRoomByIdQuery(Guid Id) : IQuery<RoomDto>;

// Handler
public sealed class GetRoomByIdQueryHandler(IRoomQuery query)
    : IQueryHandler<GetRoomByIdQuery, RoomDto>
{
    public async Task<RoomDto> Handle(GetRoomByIdQuery request, CancellationToken cancellationToken)
    {
        var dto = await query.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException(nameof(Room));
        return dto;
    }
}
```

### GetRoomByCodeQuery (from project)
```csharp
public sealed record GetRoomByCodeQuery(string Code) : IQuery<RoomDto>;

public sealed class GetRoomByCodeQueryHandler(IRoomQuery query)
    : IQueryHandler<GetRoomByCodeQuery, RoomDto>
{
    public async Task<RoomDto> Handle(GetRoomByCodeQuery request, CancellationToken cancellationToken)
    {
        var dto = await query.GetByCodeAsync(request.Code, cancellationToken)
            ?? throw new KeyNotFoundException(nameof(Room));
        return dto;
    }
}
```

## Conventions

| Element | Convention |
|---------|-----------|
| Return type | `RoomDto`, `UserDto`, etc. or `MediatR.Unit` |
| Query interface | `I{Entity}Query` in `ChatRooms.Domain.{Entity}` |
| Method name | `GetByIdAsync`, `GetByCodeAsync`, `GetAllAsync` |
| Query parameters | Primitives only — convert to domain value objects at the infrastructure layer |
| No validators | Queries don't get FluentValidation validators |
