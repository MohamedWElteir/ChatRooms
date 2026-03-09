# ChatRooms

An enterprise-grade, highly scalable chat application built with modern .NET utilizing Clean Architecture, CQRS (Command Query Responsibility Segregation), and Domain-Driven Design (DDD).

## Architecture



This project strictly separates read and write concerns to optimize for both complex business logic validation and high-throughput data retrieval:

* **Command Model (Write):** Uses PostgreSQL and Entity Framework Core. The Domain layer is highly encapsulated, utilizing rich immutable entities, custom Value Objects (e.g., `RoomId`, `DateTimeUtc`, `Capacity`), and strongly typed domain events.
* **Query Model (Read):** Uses MongoDB for blazing-fast, flattened document reads, entirely bypassing relational joins.
* **Event Synchronization:** Implements a robust Outbox Pattern to guarantee at-least-once delivery of Domain Events from the PostgreSQL write-context to the MongoDB read-context.

## Tech Stack

* **Framework:** .NET 10
* **Orchestration:** .NET Aspire
* **Write Database:** PostgreSQL (via EF Core)
* **Read Database:** MongoDB (via native C# Driver)
* **CQRS / Mediator:** MediatR
* **UI:** Blazor Web App

## Project Structure

* `ChatRooms.AppHost`: The .NET Aspire orchestrator. Manages Docker containers, networking, and dynamic connection string injection for local development.
* `ChatRooms.Domain`: The core of the system. Contains enterprise business rules, Aggregates, Entities, Value Objects, and Domain Events. Has no external dependencies.
* `ChatRooms.Application`: The MediatR use-cases (Commands and Queries), DTOs, and interface definitions.
* `ChatRooms.Infrastructure`: Data access implementations (EF Core & Mongo), background workers, and external service integrations.
* `ChatRooms.API`: The RESTful presentation layer exposing the application endpoints.
* `ChatRooms.Blazor`: The front-end user interface.

## Key Features

### The Outbox Pattern & Dead Letter Queue (DLQ)
To prevent dual-write vulnerabilities, domain events are serialized and saved to an `OutboxMessages` table within the exact same database transaction as the business entity changes. A background `OutboxProcessor` worker continuously polls this table and projects the events into MongoDB.

* **Resiliency:** Configurable retry counts and polling intervals via the .NET Options Pattern (`OutboxOptions`).
* **Dead Letter Queue (DLQ):** Messages that exceed the maximum retry count are automatically quarantined (`IsDeadLetter = true`) to prevent infinite loop poison-message processing and database lockups.
* **Strategy Pattern & Keyed Services:** Event projection is dynamically routed using .NET Keyed Dependency Injection (`IEventProjector`), adhering to the Open/Closed Principle. Adding a new event sync requires zero modifications to the background worker loop.

## Getting Started

### Prerequisites
* .NET 10 SDK
* Docker Desktop (required for .NET Aspire to spin up Postgres and Mongo containers automatically)

### Running Locally
The application relies entirely on **.NET Aspire** for local orchestration. You do not need to install PostgreSQL or MongoDB on your host machine.

1. Set `ChatRooms.AppHost` as your startup project in Visual Studio, or navigate to the AppHost directory in your terminal.
2. Run the application (`dotnet run` or **F5**).
3. The .NET Aspire Dashboard will launch in your browser.
4. From the dashboard, you can monitor logs, distributed traces, and access the integrated web UIs for your databases:
   * **pgAdmin:** Automatically wired to the `chatrooms-write-db`.
   * **Mongo Express:** Automatically wired to the `chatrooms-read-db`.

### Database Migrations
When running in the `Development` environment, the API automatically applies pending EF Core migrations to the PostgreSQL container on startup. 

To generate a new migration using the EF Core CLI, run the following command from the solution root:
```bash
dotnet ef migrations add <MigrationName> --project ChatRooms.Infrastructure --startup-project ChatRooms.API
```

## Configuration

The Outbox Background Worker behaves according to values defined in the `ChatRooms.API/appsettings.json` file. These can be overridden via environment variables in production.

```json
"Outbox": {
  "MaxRetryCount": 6,
  "BatchSize": 20,
  "PollingIntervalSeconds": 3
}
```