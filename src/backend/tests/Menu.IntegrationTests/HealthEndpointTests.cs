using System.Net;

namespace Menu.IntegrationTests;

/// <summary>Health endpoints belong to Host.Core, not to a module. They are tested once, here.</summary>
public class HealthEndpointTests(CoreApiFactory factory) : IClassFixture<CoreApiFactory>
{
    [Theory]
    [InlineData("/health/live")]
    [InlineData("/health/ready")]
    public async Task HealthEndpoint_Returns200(string path)
    {
        using var client = factory.CreateClient();

        using var response = await client.GetAsync(new Uri(path, UriKind.Relative), TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
