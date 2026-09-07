# Windows / WSL2 development

Use one physical checkout: `G:\Programowanie\2025\TaskHub` on Windows and
`/work/programowanie/2025/TaskHub` in Ubuntu 26.04 WSL2. Use native Linux tools in
WSL; do not invoke Windows `.exe` tools or relocate/duplicate the checkout.

## SDK and build outputs

`global.json` accepts stable .NET 10 SDK feature bands starting at 10.0.100.
Check `dotnet --version` and `dotnet --list-sdks`. The root `Directory.Build.props`
separates every C# project's build outputs and NuGet assets:

| Host | Output root | Intermediate / NuGet root |
| --- | --- | --- |
| Windows / Visual Studio | `bin/windows/` | `obj/windows/` |
| Linux / WSL / Linux containers | `bin/linux/` | `obj/linux/` |

Both `bin/**` and `obj/**` are excluded from SDK item globs and already ignored by
Git. Old `obj/project.assets.json` files may remain; they are not authoritative
after the split. Restore on each OS before its first build. Do not build both OS
variants simultaneously in the same checkout. Dockerfiles copy the root build
policy and SDK policy before their restore layer. Visual Studio keeps the full
solution, WPF `net10.0-windows`, launch profiles and Docker tooling.

## Canonical WSL commands

Run from the repository root. Do not use the full `TaskHub.slnx` as the Linux
build entry point: it also contains Windows-only WPF and `Microsoft.Docker.Sdk`.
Do not add `EnableWindowsTargeting` merely to make that solution build on Linux.

```bash
dotnet build src/API/TaskHub.Api/TaskHub.Api.csproj --disable-build-servers -m:1
dotnet build src/Web/TaskHub.Web/TaskHub.Web.csproj --disable-build-servers -m:1
dotnet test test/Unit/TaskHub.UnitTests/TaskHub.UnitTests.csproj --disable-build-servers -m:1
dotnet test test/Integration/TaskHub.IntegrationTests/TaskHub.IntegrationTests.csproj --disable-build-servers -m:1
```

Focused examples:

```bash
dotnet test test/Unit/TaskHub.UnitTests/TaskHub.UnitTests.csproj --disable-build-servers -m:1 --filter FullyQualifiedName~TaskTitleTests.Create_TaskListTitle_Valid_ShouldSuccess
dotnet test test/Integration/TaskHub.IntegrationTests/TaskHub.IntegrationTests.csproj --disable-build-servers -m:1 --filter FullyQualifiedName~TaskListTests.Create_valid_ShouldSuccess
```

Unit tests do not require Docker. Integration tests require Docker Desktop WSL
Integration and Docker socket/network access. `TaskHubMsSqlFixture` provisions
its own SQL Server 2022 container with Testcontainers.MsSql; migrations and
Respawn initialize/reset the test database; fixture disposal and Ryuk clean up.
Do not start `docker compose up` for Integration and never fall back to a dev DB.
Run a focused test first, then the full integration suite. Zero executed tests
or all-skipped tests are not PASS. After a successful restore/build, use
`--no-restore` / `--no-build` only while the exact current source is unchanged.

## Windows LocalDB and direct WSL hosts

Windows keeps its existing `(localdb)\MSSQLLocalDB` development default and
Visual Studio launch profiles. LocalDB is not a WSL database. For a direct WSL API
process, override `ConnectionStrings__TaskHub` with SQL Server reachable on
`localhost,1433` (or the published port of the selected development DB).

The following uses a hidden prompt so no real credential is tracked or placed
in shell history. Enter a complete connection string, for example with
`Server=localhost,1433;Database=TaskHub;User Id=sa;Password=<local-secret>;Encrypt=True;TrustServerCertificate=True`.

```bash
read -rsp 'WSL SQL Server connection string: ' taskhub_connection
printf '\n'
ConnectionStrings__TaskHub="$taskhub_connection" \
  dotnet run --project src/API/TaskHub.Api/TaskHub.Api.csproj --launch-profile http --no-build
unset taskhub_connection
```

In another shell, run the Web host using its existing HTTP profile, which points
to the API on port 5072:

```bash
dotnet run --project src/Web/TaskHub.Web/TaskHub.Web.csproj --launch-profile http --no-build
```

The ignored `.env` already supplies Compose's SQL password, JWT and optional demo
credentials. Compose can reuse it directly. For a direct API process, the same
values can be read from `docker compose config --format json` into a process-local
environment without printing them; replace the Compose-only DB hostname `db`
with the published localhost address. Keep API/Web demo and JWT settings aligned
if mixing direct hosts with the Compose database. Never print `.env`, resolved
environment blocks, tokens or connection strings, and never copy them into tracked
files. Do not `source .env` as shell code merely to read Compose configuration.

## Full development environment with Compose

The existing `.env` is ignored by Git. A new machine can use `.env.example` as a
template and supply its own values locally. Do not overwrite an existing `.env`.

```bash
docker compose config --quiet
docker compose build
docker compose up -d
```

The canonical stack provides SQL Server 2022 (`db`, localhost:1433), API Swagger
at `http://localhost:8080/swagger/index.html`, and Web at `http://localhost:5262`.
DB has a SQL healthcheck; API/Web have no configured healthchecks, so verify their
HTTP endpoints separately after DB readiness. Compose is the full development
environment, not the integration-test authority.

The canonical config uses the fixed `taskhub-mssql` container name and fixed host
ports. `-p` alone does not isolate them. For temporary validation alongside a
development stack, use a task-owned override file with a unique DB container name,
unique API/Web image tags and replaced port mappings (`!override`), together with
`-p <unique-project>`. Keep that override outside tracked source. Clean up only
that project with the identical `-p` and `-f` arguments:

```bash
docker compose -p <unique-project> -f docker-compose.yml \
  -f docker-compose.override.yml -f <task-override.yml> down --volumes --remove-orphans
```

Do not run this destructive cleanup against a shared development project. Remove
only task-created containers, volumes, networks and image tags; do not prune Docker.

## Codex execution contract

- `codex-taskhub`: normal analysis/implementation and focused Unit work.
- `codex-taskhub-manual`: the same workflow with user-reviewed approvals.
- `codex-taskhub-e2e`: network restores, Integration/Testcontainers, Compose and
  API/Web checks; browser work only if the repository later gains that workflow.
- `codex-taskhub-unattended`: offline/static/unit work that needs no escalation.
- `codex-taskhub-e2e-unattended`: not canonical for Testcontainers until a separate
  unattended smoke proves Docker, network and cache access plus cleanup.

These are local shell shortcuts, not repository runners. When a restricted
sandbox blocks Docker/socket/network or a required cache write, record the exact
command and error, request standard escalation, then rerun the identical command.
The escalated identical run is authoritative. Do not change tests or use a dev DB
to conceal a sandbox failure. Prefer this workflow to broad writes under `$HOME`
or a global sandbox change; do not use `danger-full-access`. If repeated evidence
requires a cache override, propose a narrow TaskHub-specific root first.

Classify failures as environment, repository infrastructure, product, sandbox,
EOL debt or Windows-specific capability. Report actual counts, exact commands,
remaining limits, cleanup and final Git state. Root `AGENTS.md` is deliberately
local-only (Git history records that policy); this versioned guide is the shared
workflow reference. No generated/projection authority for root AGENTS was found.

## EOL debt

Do not run `git add --renormalize .`, mass EOL conversion, reset, clean, checkout,
stash or restoration of user files as part of bootstrap. Inspect `git status`,
`git diff --ignore-cr-at-eol`, `git ls-files --eol` and Git's core EOL settings.
If existing tracked differences have zero semantic diff, classify them as
`REPOSITORY EOL DEBT - NON-BLOCKING`. A future dedicated task can agree an
appropriate `.gitattributes` policy and perform a separately reviewed conversion.

There is currently no canonical browser E2E suite. Do not install Playwright/Node,
invent a browser runner or make browser tooling a prerequisite for these commands.
