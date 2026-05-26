# ChatRooms

## What This Is
Enterprise-grade chat application with CQRS+DDD using Clean Architecture, PostgreSQL for writes, MongoDB for reads, orchestrated via .NET Aspire.

## Stack
- Runtime: .NET 10
- Architecture: Clean Architecture + CQRS (MediatR)
- Database: PostgreSQL (write) + MongoDB (read)
- ORM/DB Access: EF Core (PostgreSQL) + MongoDB.Driver (MongoDB)
- Messaging: MediatR (in-process CQRS) + Outbox Pattern (cross-dB sync)
- Test framework: xUnit + Moq

## Build & Run
- Build: `dotnet build`
- Test: `dotnet test`
- Run: `dotnet run --project ChatRooms.AppHost`
- Format: `dotnet format`

## Project Layout
```
ChatRooms.Domain/        — Entities, Value Objects, Domain Events, Aggregates
ChatRooms.Application/   — Commands, Queries, Handlers, Validators, Abstractions
ChatRooms.Infrastructure/ — EF Core DbContext, Mongo, Repositories, Outbox, Auth
ChatRooms.API/           — REST API controllers, middleware
ChatRooms.Blazor/        — Blazor Web UI
ChatRooms.BFF/           — YARP reverse proxy (Backend-for-Frontend)
ChatRooms.Presentation/  — API endpoints (Minimal API / Carter-style)
ChatRooms.DTOs/          — Shared DTOs
ChatRooms.AppHost/       — .NET Aspire orchestrator
ChatRooms.ServiceDefaults/ — OpenTelemetry, health checks, service discovery
ChatRooms.KeycloakSetup/ — Keycloak realm config bootstrapper
ChatRooms.SharedKernel/  — Future shared kernel (currently empty)
```

## Domain Language
- Room, User (aggregates)
- RoomId, UserId, Capacity, RoomCode, Name, Email, BirthDate, Age, Gender (value objects)
- RoomCreated, RoomRenamed, RoomArchived, RoomDeleted, RoomCapacityChanged, RoomParticipantJoined, RoomParticipantLeft, RoomUnArchived (room domain events)
- UserCreated, UserRenamed, UserDeleted, UserEmailChanged, UserGenderChanged (user domain events)
- RoomStatus, DeletionReason, Gender (enums)
- OutboxMessage, DeadLetterQueue (infrastructure concepts)

## Conventions
- `PascalCase` for classes, methods, properties, namespaces
- `_camelCase` for private fields
- `camelCase` for parameters and locals
- One file per class, filename matches class name
- Event sourcing-ish pattern: `AggregateRoot<TId>` with `Apply(IDomainEvent)` and `Raise()`
- Result pattern: `Result<T>` / `Result` struct with implicit conversions, never exceptions for flow control
- Commands in `{Entity}/Commands/{Action}/` with `{Action}{Entity}Command` naming
- Queries in `{Entity}/Queries/{Action}/` with `Get{Entity}By{Criteria}Query` naming
- Handlers: `{Action}{Entity}CommandHandler` / `Get{Entity}By{Criteria}QueryHandler`
- Tests mirror Application structure: `{Feature}HandlerTests.cs`
- Domain events carry `DateTimeUtc OccurredAt` and use past-tense naming
- Entity configurations in `Infrastructure/Persistence/Configurations/`
- DI registration via `Add{Layer}()` extension methods in each project
- `IUnitOfWork` backed by `WriteDbContext` (EF Core)
- Read side uses separate `ReadDbContext` (MongoDB, singleton)
- `IEventProjector` keyed services for outbox → MongoDB projection

## Hard Rules
- Never modify applied migration files (`Migrations/*.cs`, `Migrations/*.Designer.cs`, `ModelSnapshot.cs`)
- Never run destructive SQL (DROP, TRUNCATE, DELETE without WHERE) without explicit confirmation
- Never add NuGet packages without asking
- Never refactor code outside the scope of the current task
- No business logic in controllers or API endpoints
- No EF Core DbContext references in Domain or Application layers
- No domain entities serialized as API response models — use DTOs

## Obsidian Vault

This project has an Obsidian vault at `C:\Users\dell\obsidian\ChatRooms` for persistent knowledge.

**At the end of every session (and periodically during long sessions), save to the vault:**

1. **`sessions/YYYY-MM-DD-HHmm.md`** — A dated session note summarizing what was done, key decisions, files modified, and next steps. Use blockquote format for the user's questions/conversation.

2. **`bugs/YYYY-MM-DD-title.md`** — When a bug is identified or discussed, create or update a bug entry with severity, reproduction steps, and fix notes. Use the frontmatter format: `status`, `severity`, `date`, `tags: [bug]`.

3. **`decisions/YYYY-MM-DD-title.md`** — When an architectural decision is made, create an ADR-style entry with `status`, `date`, `tags: [decision]`, followed by Context/Decision/Consequences/Alternatives sections.

Also update the index files (`bugs/readme.md`, `decisions/readme.md`) with new bullet-point entries linking to the new notes.

## Known Gotchas
- `ChatRooms.SharedKernel` exists but is empty — likely placeholder for future extraction
- Migrations target both PostgreSQL (write) and MongoDB (read) — two separate DB concerns
- Outbox pattern is central to data consistency; never bypass it for write→read sync
- Room entity uses event-sourcing-style state rebuild via `Apply()`, not property setters
