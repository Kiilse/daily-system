using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Web;

/// <summary>
/// Liveness and readiness endpoints shared by every host, used by Railway.
/// /health/live answers as soon as the process runs. /health/ready also runs every check tagged
/// <see cref="ReadyTag"/> (database, Key Vault... registered by later tickets).
/// </summary>
public static class HealthCheckExtensions
{
    public const string ReadyTag = "ready";

    public static IHealthChecksBuilder AddDefaultHealthChecks(this IServiceCollection services) =>
        services.AddHealthChecks();

    public static IEndpointRouteBuilder MapDefaultHealthChecks(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
        endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = check => check.Tags.Contains(ReadyTag) });
        return endpoints;
    }
}
