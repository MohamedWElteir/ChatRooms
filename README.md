# ChatRooms

[![wakatime](https://wakatime.com/badge/user/8a84d55b-e339-4aae-af7e-e53592801c34/project/7e973a6e-40e4-4a30-a84e-e046741bac2e.svg)](https://wakatime.com/badge/user/8a84d55b-e339-4aae-af7e-e53592801c34/project/7e973a6e-40e4-4a30-a84e-e046741bac2e)

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
* **IAM / SSO:** Keycloak (OIDC)
* **Reverse Proxy (BFF):** YARP

## Project Structure

* `ChatRooms.AppHost`: The .NET Aspire orchestrator. Manages Docker containers, networking, and dynamic connection string injection for local development.
* `ChatRooms.Domain`: The core of the system. Contains enterprise business rules, Aggregates, Entities, Value Objects, and Domain Events. Has no external dependencies.
* `ChatRooms.Application`: The MediatR use-cases (Commands and Queries), abstract interfaces, and application-level validations.
* `ChatRooms.Infrastructure`: Data access implementations (EF Core & Mongo), background workers (Outbox), and external service integrations.
* `ChatRooms.Presentation`: Includes the presentation-layer API endpoints (minimal APIs) alongside `ChatRooms.API`, which serves as the host.
* `ChatRooms.DTOs`: Shared read-model Data Transfer Objects used by the API and the Blazor client.
* `ChatRooms.BFF`: The YARP reverse proxy (Backend-for-Frontend layer for the Blazor app).
* `ChatRooms.Blazor`: The front-end user interface (Razor components), including Keycloak token handling.
* `ChatRooms.ServiceDefaults`: Shared configuration, service discovery, and observability defaults for all services.
* `ChatRooms.KeycloakSetup`: Realm/Client bootstrap for the local Keycloak instance.
* `ChatRooms.Application.Tests`, `ChatRooms.Domain.Tests`, `ChatRooms.Infrastructure.Tests`, `ChatRooms.Presentation.Tests`, `ChatRooms.Blazor.Tests`: Unit test suites.

## Key Features

### Authentication (Keycloak / OIDC)
* **Code Flow + PKCE**: The server-side Blazor app authenticates users against Keycloak using the Authorization Code flow with PKCE.
* **Refresh-Token Rotation:** Access tokens are refreshed per circuit using a confidential client (`chatrooms-bff`); returned refresh tokens are always rotated.
* **Concurrency-Safe Single-Flight Refresh:** When multiple requests in the same circuit observe an expired access token, exactly one refresh is issued; the others reuse the refreshed token after acquiring the in-memory semaphore.
* **No Token Caching:** Tokens are scoped to the circuit/`HttpContext` (cookie properties) — never stored statically or in a process-wide cache.
* **Session Expiry:** Failed or unavailable refresh sessions explicitly expire the circuit session instead of silently falling back to anonymous calls.

### The Outbox Pattern & Dead Letter Queue (DLQ)
To prevent dual-write vulnerabilities, domain events are serialized and saved to an `OutboxMessages` table within the exact same database transaction as the business entity changes. A background `OutboxProcessor` worker continuously polls this table and projects the events into MongoDB.

* **Claim & Leases:** Each poll claims a batch of up to `BatchSize` rows (SQL `FOR UPDATE SKIP LOCKED`) and stamps each with `ProcessingBy` + a lease (`ProcessingLeaseUntil`, default 2 minutes).
* **Lease Heartbeat:** For every message, the worker renews the lease of the entire remaining batch (guarded by worker ownership AND lease-not-expired); if the renew count is less than requested, the worker abandons the batch rather than processing a message it no longer owns.
* **Expired Leases Are Never Renewed:** a lease must be actively extended by its owner; an expired record returns to the claim queue even if it is still marked `ProcessingBy` the old worker.
* **Crash Recovery:** If a worker crashes, its lease expires and another worker claims the records — the new worker starts where the old stopped.
* **Dead Letter Queue (DLQ):** Messages that exceed the maximum retry count are automatically quarantined (`IsDeadLetter = true`) to prevent poison-message infinite loops and database lockups.
* **Idempotent Projections:** every domain-event projection against Mongo is idempotent (upserts/version-guarded replaces) so at-least-once delivery converges to a correct read state even with concurrent workers.
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

The Outbox Background Worker behaves according to values defined in the `ChatRooms.API/appsettings.json` file under the `OutboxOptions` section. These can be overridden via environment variables in production.

```json
"OutboxOptions": {
  "MaxRetryCount": 6,
  "BatchSize": 20,
  "PollingIntervalSeconds": 3,
  "ProcessingLeaseDurationMinutes": 2
}
```