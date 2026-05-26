---
paths:
  - "**/*.cs"
---
# Clean Architecture Rules

## Dependency Direction (strictly enforced)
Domain → nothing
Application → Domain only
Infrastructure → Application + Domain
API/Presentation → Application only

## Layer Responsibilities
- Domain: Entities, Value Objects, Domain Events, Repository interfaces, Domain Services
- Application: Use cases (Commands/Queries), DTOs, Application Services, Interfaces
- Infrastructure: EF Core, MongoDB, external APIs, outbox processing, auth implementation
- API: Controllers/Endpoints, Middleware, DI registration only

## CQRS via MediatR
- Commands return `Result<T>` or `Result` — never raw entities
- Queries return DTOs — never domain entities
- Handlers are in Application layer, one handler per file
- Validators (FluentValidation) live next to their Command/Query

## What NOT to do
- No business logic in controllers
- No EF Core DbContext in Domain or Application layer
- No domain entities as API response models
- No static classes in Domain layer
