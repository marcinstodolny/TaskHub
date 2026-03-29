# TaskHub

## Overview

TaskHub is an in-progress .NET 10 engineering portfolio project for a task management system built as a multi-project solution. It brings together an ASP.NET Core API, a Blazor web client, a WPF desktop client, shared contracts, and a layered backend structure to show how the same business domain can be exposed through multiple clients and clear project boundaries.

## What this project demonstrates

TaskHub is a focused engineering portfolio project that shows how a task management system can be structured as a layered .NET solution without pretending to be complete. In its current form, the repository demonstrates:

- CQRS with MediatR in the application layer
- DDD-inspired domain modeling with entities, value objects, enums, and policies
- Layered architecture across Domain, Application, Infrastructure, API, Web, and WPF projects
- Shared request and response contracts via `TaskHub.Contracts`
- Hybrid persistence with EF Core for transactional writes and Dapper for read-side queries and projections
- Local development with Docker Compose and SQL Server
- Integration testing with Testcontainers, with Respawn available for database reset support
- FluentValidation wired into the MediatR pipeline

## Current state

TaskHub currently includes working task list and task item endpoints, shared request and response contracts, domain entities and value objects, infrastructure for persistence, a Blazor UI with working pages, and unit and integration test projects. The backend structure is already in place and the web client is usable for demonstrating the flow through the system. The WPF client is intentionally at an earlier stage and should be read as a scaffold for further desktop development rather than a feature-complete application.

## Architecture foundations

The solution is organized as a layered architecture with clear project boundaries:

- `src/API/TaskHub.Api` hosts the HTTP API, Swagger configuration, and controllers.
- `src/Application/TaskHub.Application` contains application-layer requests, abstractions, and behaviors.
- `src/Domains/TaskHub.Domain` contains the core domain model, value objects, enums, and policies.
- `src/Infrastructure/TaskHub.Infrastructure` contains persistence, repositories, database configuration, and migrations.
- `src/Contracts/TaskHub.Contracts` provides shared contracts used across project boundaries.
- `src/Web/TaskHub.Web` is the current web client.
- `src/WPF/TaskHub.WPFClient` is the desktop client foundation.

The result is a layered solution with a clear multi-client direction: API and backend concerns stay separate from presentation, while shared contracts keep communication between projects explicit.

## Project structure

Key areas of the repository:

- `src/API/TaskHub.Api` — API host and controllers
- `src/Application/TaskHub.Application` — application-layer requests, abstractions, and behaviors
- `src/Domains/TaskHub.Domain` — domain model and business rules foundation
- `src/Infrastructure/TaskHub.Infrastructure` — persistence, repositories, and database setup
- `src/Contracts/TaskHub.Contracts` — shared contracts for requests and responses
- `src/Web/TaskHub.Web` — Blazor web application
- `src/WPF/TaskHub.WPFClient` — WPF desktop client scaffold
- `test/Unit/TaskHub.UnitTests` — unit tests around domain behavior
- `test/Integration/TaskHub.IntegrationTests` — integration tests for API and persistence flow

## Running locally

The easiest local start is through the helper script. It creates `.env` from `.env.example` if needed and then starts Docker Compose.

If you run the API or Web project directly from an IDE or from `dotnet run`, the launch profiles also include matching local development values for JWT and demo auth.

On Windows PowerShell:

```powershell
.\scripts\dev-up.ps1
```

On macOS or Linux:

```bash
sh ./scripts/dev-up.sh
```

If you prefer to do it manually, the repository includes a default local environment template in `.env.example`.
Create your own `.env` file from it:

```bash
cp .env.example .env
```

On PowerShell:

```powershell
Copy-Item .env.example .env
```

The example file contains local development values for Docker Compose. They are intended only for local development. If you need different credentials or JWT settings, change them in `.env` before starting the stack.

Start the full stack with Docker Compose:

```bash
docker compose up --build
```

Default endpoints:

```text
Web: http://localhost:5262
API: http://localhost:8080
Swagger: http://localhost:8080/swagger
SQL Server: localhost:1433
```

Tests can be run with `dotnet test`, and the integration suite requires Docker.

## Planned next steps

- Extend the API and application layer with additional task management use cases.
- Continue shaping the domain and infrastructure layers as the workflow becomes more complete.
- Evolve the Blazor client beyond the current working pages into a more polished front end.
- Build out the WPF client so the desktop path moves beyond its current scaffold.
- Increase automated test coverage as more behavior is added to the solution.
