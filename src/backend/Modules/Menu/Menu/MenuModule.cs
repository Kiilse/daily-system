using Menu.Features.Diagnostics.Ping;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Menu;

/// <summary>Entry points used by Host.Core, so the host never references the module's internals.</summary>
public static class MenuModule
{
    public static IServiceCollection AddMenuModule(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        services.AddScoped<PingHandler>();
        return services;
    }

    public static IEndpointRouteBuilder MapMenuEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/menu").WithTags("Menu");
        group.MapPing();
        return endpoints;
    }
}
