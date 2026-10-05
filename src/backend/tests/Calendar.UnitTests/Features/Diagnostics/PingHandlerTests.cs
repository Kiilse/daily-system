using Calendar.Features.Diagnostics.Ping;

namespace Calendar.UnitTests.Features.Diagnostics;

public class PingHandlerTests
{
    [Fact]
    public void Handle_ReturnsTheModuleName()
    {
        var result = new PingHandler().Handle();

        result.IsSuccess.ShouldBeTrue();
        result.Value.Module.ShouldBe("calendar");
    }
}
