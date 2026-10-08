# Backend

.NET 10 modular monolith ([ADR-001](../../docs/adr/adr-001-modular-monolith-with-co-hosted-modules-in-v1.md)). Each module is organized in vertical slices ([ADR-007](../../docs/adr/adr-007-internal-architecture-per-module.md)).

## Commands

Run from `src/backend/`.

| What | Command |
|---|---|
| Build (0 warnings expected, warnings are errors) | `dotnet build Ecosystem.slnx -c Release` |
| Test | `dotnet test --solution Ecosystem.slnx -c Release` |
| Test with coverage (Cobertura) | `dotnet test --solution Ecosystem.slnx -c Release -- --coverlet --coverlet-output-format cobertura` |
| Coverage gate, as in CI (after the line above) | `dotnet tool restore` once, then `../../tools/backend-coverage.sh` (80 % on `*.Domain.*` and `*.Features.*`, endpoints excluded) |
| Check formatting | `dotnet format Ecosystem.slnx --verify-no-changes` |
| Restore exactly the locked versions | `dotnet restore Ecosystem.slnx --locked-mode` |
| Run Host.Core | `dotnet run --project Host.Core` then `curl localhost:5100/health/live` |
| Run Gateway.Bff | `dotnet run --project Gateway.Bff` then `curl localhost:5000/health/live` |
| Run everything in containers (PostgreSQL, core, gateway-bff) | from `infra/`: see [docs/runbooks/local-setup.md](../../docs/runbooks/local-setup.md) |

Local ports come from each host's `Properties/launchSettings.json`. In a container, both hosts listen on 8080 (`ASPNETCORE_HTTP_PORTS`, the .NET image default). To try the container path locally: `ASPNETCORE_HTTP_PORTS=8080 dotnet run --project Host.Core --no-launch-profile`.

CI runs the same commands on every PR (`.github/workflows/ci-backend.yml`); its `backend` job is required to merge into `main`. `dotnet-tools.json` pins the .NET tools it uses (ReportGenerator).

Package versions live in `Directory.Packages.props` (central package management). After changing one, run `dotnet restore` and commit the updated `packages.lock.json` files.

## Project map

| Path | Role |
|---|---|
| `BuildingBlocks/SharedKernel` | `Error`, `ErrorType`, `Result`, `Result<T>`. No framework dependency. |
| `BuildingBlocks/Web` | `Result` to HTTP mapping (RFC 9457 Problem Details), health check endpoints. |
| `BuildingBlocks/Security`, `BuildingBlocks/Persistence` | Empty for now, filled by #18 and by the first module that stores data. |
| `Modules/<Name>/<Name>.Contracts` | The only public surface of a module. |
| `Modules/<Name>/<Name>` | Module implementation: `<Name>Module.cs` (registration and endpoints) plus `Features/<Area>/<Slice>/`. |
| `Host.Core` | Hosts the Account, Menu and Calendar modules. |
| `Gateway.Bff` | Backend for frontend. Health endpoints only until #14/#15. |
| `tests/` | One unit and one integration project per module, `BuildingBlocks.UnitTests`, `Architecture.Tests`, `Infrastructure.IntegrationTests` (the `infra/` database bootstrap against a real PostgreSQL, needs Docker). |
| `Host.Core/Dockerfile`, `Gateway.Bff/Dockerfile` | Production images: SDK build stage, chiseled runtime running as the non-root `app` user. Build context is `src/backend`. |

Health endpoints, on both hosts:
- `/health/live`: the process answers. Runs no check.
- `/health/ready`: runs the checks tagged `ready` (none yet, the database check comes with the first module that uses its database).

In a container, `dotnet <Host>.dll --healthcheck` calls `/health/live` and exits with 0 or 1 (`BuildingBlocks.Web.HealthCheckCommand`). Compose uses it as the healthcheck, because the chiseled images have no shell and no curl.

## Architecture rules

A module may reference another module **only through its `*.Contracts` project**, never its implementation project.

`tests/Architecture.Tests` enforces this and the other structural rules, so breaking one fails the test run. Most rules use ArchUnitNET on the compiled code. `ProjectReferenceTests` checks the `.csproj` files too, because the compiler drops a project reference no code uses yet, which ArchUnitNET cannot see.

| # | Rule | Tests |
|---|---|---|
| 1 | A module uses another module only through its `*.Contracts` | `ModuleBoundaryTests`, `ProjectReferenceTests` |
| 2 | A `*.Contracts` project depends only on `BuildingBlocks.SharedKernel` | `ModuleBoundaryTests`, `ProjectReferenceTests` |
| 3 | Types in a `*.Domain` namespace do not use EF Core, ASP.NET Core or `System.Net.Http` | `LayeringTests` |
| 4 | `Gateway.Bff` uses no module, implementation or contracts | `ModuleBoundaryTests`, `ProjectReferenceTests` |
| 5 | In a module implementation project, only `{Name}Module` is public | `VisibilityTests` |
| 6 | Only `BuildingBlocks.Security` uses `AesGcm` | `CryptographyTests` |

Adding a module: add its folder under `Modules/`, then add it to `ProductionCode.Modules` in `tests/Architecture.Tests`. `ModuleListTests` fails until both match.
