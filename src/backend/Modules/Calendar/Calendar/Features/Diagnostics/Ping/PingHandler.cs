using BuildingBlocks.SharedKernel;

namespace Calendar.Features.Diagnostics.Ping;

/// <summary>Temporary: proves the module is wired into Host.Core. Removed once the module has a real feature.</summary>
internal sealed class PingHandler
{
#pragma warning disable CA1822 // Handlers are resolved from DI as instances, like every future handler
    public Result<PingResponse> Handle() => new PingResponse("calendar");
#pragma warning restore CA1822
}
