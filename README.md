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

Set the required environment variables before running Docker Compose:

```bash
export MSSQL_SA_PASSWORD='YourStrong!Passw0rd'
export USER_SECRETS_PATH="$HOME/.microsoft/usersecrets"
export HTTPS_CERTS_PATH="$HOME/.aspnet/https"
```

Then run:

```bash
docker compose up --build
```

Services:
- API: `http://localhost:8080` / `https://localhost:8081`
- SQL Server: `localhost:1433`

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
