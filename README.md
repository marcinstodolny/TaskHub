# TaskHub

TaskHub is a sample .NET solution showcasing **CQRS**, **DDD-style layering**, **Dockerized infrastructure**, **hybrid EF Core/Dapper data access**, and **integration testing** against a real SQL Server database.

## What it demonstrates

- CQRS with MediatR (commands/queries)
- Layered structure (Domain / Application / Infrastructure)
- Hybrid persistence (EF Core for writes, Dapper for read projections)
- Local environment via Docker Compose (SQL Server + app)
- Integration tests using Testcontainers (optionally with Respawn for DB cleanup)

## Tech stack

- .NET 10
- ASP.NET Core
- Entity Framework Core
- Dapper (query/read projections)
- MediatR (CQRS)
- SQL Server (Docker)
- xUnit + Testcontainers (integration tests)
- (Optional) Respawn for integration test DB reset

## Prerequisites

- Docker (required for integration tests and local SQL Server)
- .NET SDK 10 (preview)

## Configuration

### Docker Compose

1. Copy sample environment variables and adjust values if needed:

```bash
cp .env.example .env
```

2. Start the stack:

```bash
docker compose up --build
```

> `docker-compose.override.yml` is used for local development (ports, development environment variables, and optional local certs/secrets mounts).

Services:
- Web: `http://localhost:5262`
- API: `http://localhost:8080` / `https://localhost:8081`
- SQL Server: `localhost:1433`

## Blazor Task Board

The Blazor web app now includes a minimal Kanban-style **Task Board** page.

1. Start the stack:

```bash
docker compose up --build
```

2. Open the web client at `http://localhost:5262`.
3. Navigate to **Task Board** in the left menu.
4. Select a task list and move cards between statuses using the **Move to** dropdown on each card.

Notes:
- Columns map directly to domain statuses: `Todo`, `InProgress`, `Done`, `Cancelled`.
- Status updates use API rules for allowed transitions (invalid transitions return API errors shown in the UI).

## Testing

### Unit tests

```bash
dotnet test test/Unit/TaskHub.UnitTests/TaskHub.UnitTests.csproj
```

### Integration tests

Integration tests use Testcontainers and require Docker to be running:

```bash
dotnet test test/Integration/TaskHub.IntegrationTests/TaskHub.IntegrationTests.csproj
```

## CI

GitHub Actions runs `dotnet test` on the solution and verifies Docker access for Testcontainers.

## Project structure

```
src/                     Application code
  API/                   Web API host
  Application/           CQRS commands/queries
  Domain/                DDD domain model
  Infrastructure/        EF Core + persistence

test/
  Unit/                  Unit tests
  Integration/           Integration tests with Testcontainers
```

## Roadmap

- Expand API surface (endpoints, commands, queries)
- Additional integration tests and API contract tests
- Explore WPF or Blazor client UI (longer term)
