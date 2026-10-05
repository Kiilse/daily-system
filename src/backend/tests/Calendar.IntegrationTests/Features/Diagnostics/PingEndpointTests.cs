using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Calendar.IntegrationTests.Features.Diagnostics;

public class PingEndpointTests(CoreApiFactory factory) : IClassFixture<CoreApiFactory>
{
    [Fact]
    public async Task GetPing_Returns200_WithTheModuleName()
    {
        using var client = factory.CreateClient();

        using var response = await client.GetAsync(new Uri("/api/calendar/ping", UriKind.Relative), TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
        body.GetProperty("module").GetString().ShouldBe("calendar");
    }
}
