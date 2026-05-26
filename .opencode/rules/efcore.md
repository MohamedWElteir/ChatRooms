---
paths:
  - "**/Migrations/**"
  - "**/*Context*.cs"
  - "**/*Configuration*.cs"
---
# EF Core Rules
- Migrations are append-only. Never edit an applied migration.
- New migration: `dotnet ef migrations add [Name] --project ChatRooms.Infrastructure --startup-project ChatRooms.API`
- Use `HasConversion` for value objects, not raw primitives on the entity.
- Lazy loading is OFF by default. Use explicit `.Include()`.
- No raw SQL in queries unless performance requires it — and document why.
- DbContext lifetime: scoped (never singleton, never transient).
