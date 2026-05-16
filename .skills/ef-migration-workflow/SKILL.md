---
name: ef-migration-workflow
description: Manage EF Core migrations for the PostgreSQL write database in ChatRooms. Use when adding, removing, or applying migrations during development.
---

# EF Core Migration Workflow

ChatRooms uses EF Core with PostgreSQL for the write side. Migrations are in `ChatRooms.Infrastructure/Migrations/`.

## Prerequisites

```bash
dotnet tool install --global dotnet-ef  # if not already installed
```

## Add a migration

Run from the solution root:

```bash
dotnet ef migrations add <MigrationName> --project ChatRooms.Infrastructure --startup-project ChatRooms.API --output-dir Migrations
```

The `--output-dir Migrations` flag keeps migration files inside `ChatRooms.Infrastructure/Migrations/`.

## Apply migrations (development)

Migrations are applied automatically in `Development` mode via `Program.cs`. To apply manually:

```bash
dotnet ef database update --project ChatRooms.Infrastructure --startup-project ChatRooms.API
```

## Remove last migration (not yet applied)

```bash
dotnet ef migrations remove --project ChatRooms.Infrastructure --startup-project ChatRooms.API
```

## Generate a SQL script

```bash
dotnet ef migrations script --project ChatRooms.Infrastructure --startup-project ChatRooms.API --output init.sql
```

## Common patterns

### Adding a new entity to the write model

1. Add the entity to `ChatRooms.Domain.{Entity}`
2. Register `DbSet<{Entity}>` in `WriteDbContext`
3. Create `{Entity}Configuration : IEntityTypeConfiguration<{Entity}>`
4. Run `dotnet ef migrations add Add{Entity}Entity`
5. Run the app to auto-apply the migration

### Adding a new value object property

1. Add the value object to `ChatRooms.Domain.{Entity}.ValueObjects`
2. Add `HasConversion` in the entity's configuration
3. Create a new migration
