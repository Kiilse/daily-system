using BuildingBlocks.SharedKernel;

namespace Menu.Features.Diagnostics.Ping;

/// <summary>Temporary: proves the module is wired into Host.Core. Removed once the module has a real feature.</summary>
internal sealed class PingHandler
{
#pragma warning disable CA1822 // Handlers are resolved from DI as instances, like every future handler
    public Result<PingResponse> Handle() => new PingResponse("menu");
#pragma warning restore CA1822
}
