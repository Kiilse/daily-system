# Backend

.NET 10 modular monolith ([ADR-001](../../docs/adr/adr-001-modular-monolith-with-co-hosted-modules-in-v1.md)). Each module is organized in vertical slices ([ADR-007](../../docs/adr/adr-007-internal-architecture-per-module.md)).

## Commands

Run from `src/backend/`.

| What | Command |
|---|---|
| Build (0 warnings expected, warnings are errors) | `dotnet build Ecosystem.slnx -c Release` |
| Test | `dotnet test --solution Ecosystem.slnx -c Release` |
| Test with coverage (Cobertura) | `dotnet test --solution Ecosystem.slnx -c Release -- --coverlet --coverlet-output-format cobertura` |
| Check formatting | `dotnet format Ecosystem.slnx --verify-no-changes` |
| Restore exactly the locked versions | `dotnet restore Ecosystem.slnx --locked-mode` |
| Run Host.Core | `dotnet run --project Host.Core` then `curl localhost:5100/health/live` |
| Run Gateway.Bff | `dotnet run --project Gateway.Bff` then `curl localhost:5000/health/live` |

Local ports come from each host's `Properties/launchSettings.json`. In a container, both hosts listen on 8080 (`ASPNETCORE_HTTP_PORTS`, the .NET image default). To try the container path locally: `ASPNETCORE_HTTP_PORTS=8080 dotnet run --project Host.Core --no-launch-profile`.

Package versions live in `Directory.Packages.props` (central package management). After changing one, run `dotnet restore` and commit the updated `packages.lock.json` files.

## Project map

| Path | Role |
|---|---|
| `BuildingBlocks/SharedKernel` | `Error`, `ErrorType`, `Result`, `Result<T>`. No framework dependency. |
| `BuildingBlocks/Web` | `Result` to HTTP mapping (RFC 9457 Problem Details), health check endpoints. |
| `BuildingBlocks/Security`, `BuildingBlocks/Persistence` | Empty for now, filled by #18 and #4. |
| `Modules/<Name>/<Name>.Contracts` | The only public surface of a module. |
| `Modules/<Name>/<Name>` | Module implementation: `<Name>Module.cs` (registration and endpoints) plus `Features/<Area>/<Slice>/`. |
| `Host.Core` | Hosts the Account, Menu and Calendar modules. |
| `Gateway.Bff` | Backend for frontend. Health endpoints only until #14/#15. |
| `tests/` | One unit and one integration project per module, `BuildingBlocks.UnitTests`, `Architecture.Tests`. |

Health endpoints, on both hosts:
- `/health/live`: the process answers. Runs no check.
- `/health/ready`: runs the checks tagged `ready` (none yet, the database check comes with #4).

## Module boundary rule

A module may reference another module **only through its `*.Contracts` project**, never its implementation project.

Today this rule is not enforced: adding a reference from `Menu` to `Calendar` still builds. Ticket #3 adds the ArchUnitNET tests in `tests/Architecture.Tests` that make such a reference fail the build.
