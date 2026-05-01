# TaskHub

## Overview

TaskHub is an in-progress .NET 10 engineering portfolio project for a task management system built as a multi-project solution. The active demo focuses on an ASP.NET Core API, a Blazor web client, SQL Server persistence, shared contracts, automated tests, and a layered backend structure.

The repository also contains a WPF desktop client project, but it is currently a planned scaffold rather than part of the active demo.

## Current demo scope

The current portfolio demo covers:

- ASP.NET Core API for authentication, task lists, task items, and task-list activity.
- Blazor web client as the primary UI.
- SQL Server persistence through EF Core command-side writes and Dapper read-side projections.
- MediatR/CQRS-style application layer with FluentValidation pipeline behavior.
- JWT-based authentication with per-user ownership checks.
- SignalR live refresh for the selected task-list activity feed.
- Docker Compose local environment with SQL Server.
- Unit and integration tests, including API/database flows with Testcontainers.
- Swagger/OpenAPI for manual API inspection.

The active demo does not currently include Snowflake integration or a feature-complete WPF desktop client.

## What this project demonstrates

TaskHub is a focused engineering portfolio project that shows how a task management system can be structured as a layered .NET solution without pretending to be complete. In its current form, the repository demonstrates:

- CQRS with MediatR in the application layer.
- DDD-inspired domain modeling with entities, value objects, enums, and policies.
- Layered architecture across Domain, Application, Infrastructure, API, Web, and test projects.
- Shared request and response contracts via `TaskHub.Contracts`.
- Hybrid persistence with EF Core for transactional writes and Dapper for read-side queries and projections.
- Task-list activity feed that records task/list actions, exposes them through the API and Blazor UI, and refreshes live through SignalR for the selected list.
- Local development with Docker Compose and SQL Server.
- Integration testing with Testcontainers, with Respawn available for database reset support.
- FluentValidation wired into the MediatR pipeline.

## Recruiter summary

TaskHub is intended to show practical .NET engineering skills that are useful in product and business applications:

- Layered .NET architecture with clear project boundaries.
- ASP.NET Core API with MediatR/CQRS use cases.
- Blazor web client with a working task board flow.
- EF Core writes, Dapper read models, SQL Server, and Docker Compose.
- JWT authentication with per-user ownership isolation.
- Unit and integration tests for domain behavior, API flows, auth, ownership, persistence, and activity feed behavior.

## What is currently implemented

- User registration and login with JWT access tokens.
- Demo user and demo task data seeding for local portfolio runs.
- Task list CRUD endpoints and Blazor UI flows.
- Task item CRUD endpoints and Blazor UI flows.
- Task status transitions with domain-level transition rules.
- Task-list activity feed for list and task changes, with SignalR live refresh for the selected list.
- Per-user ownership checks across command and query paths.
- EF Core persistence, migrations, repository abstractions, and unit of work.
- Dapper-based read repositories for paginated list, task, and activity projections.
- Docker Compose setup for Web, API, and SQL Server.
- Unit tests for domain/value-object behavior and selected application handlers.
- Integration tests for API, database, auth, ownership, and activity feed behavior.

## Outside the current demo

- The WPF project is present as a planned desktop client scaffold, not as a feature-complete client.
- Snowflake or external warehouse integration is not implemented.
- Reporting/export is only a possible future extension, not part of the current application.

## Architecture foundations

The solution is organized as a layered architecture with clear project boundaries:

- `src/API/TaskHub.Api` hosts the HTTP API, Swagger configuration, and controllers.
- `src/Application/TaskHub.Application` contains application-layer requests, abstractions, and behaviors.
- `src/Domains/TaskHub.Domain` contains the core domain model, value objects, enums, and policies.
- `src/Infrastructure/TaskHub.Infrastructure` contains persistence, repositories, database configuration, and migrations.
- `src/Contracts/TaskHub.Contracts` provides shared contracts used across project boundaries.
- `src/Web/TaskHub.Web` is the active Blazor web client.
- `src/WPF/TaskHub.WPFClient` is a planned desktop client scaffold and is not part of the current active demo.

The result is a layered solution with a clear API-first direction: backend concerns stay separate from presentation, while shared contracts keep communication between projects explicit.

## Project structure

Key areas of the repository:

- `src/API/TaskHub.Api` — API host and controllers
- `src/Application/TaskHub.Application` — application-layer requests, abstractions, and behaviors
- `src/Domains/TaskHub.Domain` — domain model and business rules foundation
- `src/Infrastructure/TaskHub.Infrastructure` — persistence, repositories, and database setup
- `src/Contracts/TaskHub.Contracts` — shared contracts for requests and responses
- `src/Web/TaskHub.Web` — active Blazor web application
- `src/WPF/TaskHub.WPFClient` — optional/planned WPF desktop client scaffold
- `test/Unit/TaskHub.UnitTests` — unit tests around domain behavior
- `test/Integration/TaskHub.IntegrationTests` — integration tests for API and persistence flow

## Running locally

You can run the project locally in two ways:

- Run the API and Web projects directly from an IDE or via `dotnet run`. The checked-in development appsettings provide local JWT and demo-auth values, and the launch profiles provide the local environment and application URLs.
- Run the full stack with Docker Compose.

The main portfolio demo entry point is the Blazor web app at `http://localhost:5262`. Swagger remains available at `http://localhost:8080/swagger` for API inspection, but it is secondary to the Web demo flow.

Demo auth is intended only for local/demo environments (`Development` and `Testing`). When `DemoAuth:Enabled` is `true`, the API startup path seeds the configured demo user and a small portfolio-friendly task dataset for that user. The demo data seed is idempotent: it can run multiple times without duplicating the seeded lists, tasks, or activities, and it does not delete or overwrite user-created data.

Compatibility with JWTs issued by older local runs is intentionally not supported; after auth-related changes, log in again and start from a fresh local database if needed.

### Demo login

For a local portfolio/demo run, the app can seed a ready-to-use demo account automatically. The checked-in development appsettings and the default `.env.example` values use the same demo credentials:

```text
Username: DefaultUser
Password: DefaultPassword123!
```

In Development, the Blazor login page shows a small demo account helper when Web-side `DemoAuth` is enabled. The helper shows the configured username and provides a `Use demo credentials` button that fills the login form. It does not auto-login.

Before starting Docker Compose, create `.env` from `.env.example`:

```bash
cp .env.example .env
```

On PowerShell:

```powershell
Copy-Item .env.example .env
```

The example file contains local development values for Docker Compose. They are intended only for local development. If you need different credentials or JWT settings, change them in `.env` before starting the stack. Docker Compose maps the same demo credential environment variables into both the API and Web containers so the seeded account and login helper stay aligned.

Start the full stack with Docker Compose:

```bash
docker compose up --build
```

When running with Docker Compose, the default endpoints are:

```text
Web app: http://localhost:5262
API: http://localhost:8080
Swagger/API inspection: http://localhost:8080/swagger
SQL Server: localhost:1433
```

Tests can be run with `dotnet test`, and the integration suite requires Docker.

## Demo flow

A simple way to present the current project:

1. Start the local environment with Docker Compose:

```bash
docker compose up --build
```

2. Open the Blazor web client at the main demo URL:

```text
http://localhost:5262
```

3. On the login page, click `Use demo credentials`, then submit the login form. You can also type the configured demo account manually:

```text
Username: DefaultUser
Password: DefaultPassword123!
```

4. Open the Task Board.
5. Inspect the pre-seeded task lists, including `Product Launch`, `Engineering Improvements`, and `Portfolio Demo`.
6. Select a pre-seeded list and review its tasks across different statuses and priorities.
7. Review the activity feed for the selected task list. It is pre-populated from the demo seed and continues to record new list/task changes.
8. Create or update a task, then move it through the available statuses.
9. To demonstrate SignalR live updates, open the task board in a second browser session, sign in with the same demo account, select the same task list, and create or move a task in the first session. The selected list's activity feed should refresh in the other session.
10. Inspect the API through Swagger at:

```text
http://localhost:8080/swagger
```

11. Run the automated test suite:

```bash
dotnet test
```

## Screenshots

Screenshots are not committed yet. Suggested paths for portfolio screenshots:

- `docs/screenshots/login.png`
- `docs/screenshots/task-board.png`
- `docs/screenshots/activity-feed.png`
- `docs/screenshots/swagger.png`

Suggested capture order:

- Login screen with demo credentials flow.
- Task board with at least one list and several tasks across statuses.
- Activity feed after creating, editing, moving, and deleting tasks.
- Swagger page showing the available TaskHub API endpoints.

## How to present this project

For a recruiter or technical interviewer, focus on the working API + Blazor demo first. A concise walkthrough should show authentication, task-list ownership, task board operations, activity tracking, Swagger, and the integration test suite.

The strongest technical talking points are the layered solution structure, MediatR/CQRS application layer, EF Core plus Dapper persistence strategy, SQL Server Docker setup, ownership isolation, and integration tests against a real database container.

WPF should be presented as a planned desktop client scaffold, not as a completed part of the demo.

## Planned next steps

- Add a small dashboard/statistics view to the Blazor client.
- Improve Swagger response metadata and API documentation.
- Add screenshots or a short GIF for the README demo flow.
- Consider extending live updates beyond the activity feed if the task board needs multi-user collaboration polish.
- Build out the WPF client later only if desktop development becomes a priority.
