using BuildingBlocks.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Calendar.Features.Diagnostics.Ping;

internal static class PingEndpoint
{
    public static RouteHandlerBuilder MapPing(this IEndpointRouteBuilder group) =>
        group.MapGet("/ping", (PingHandler handler) => handler.Handle().ToHttpResult());
}
