using System.Net;
using BuildingBlocks.Web;

namespace BuildingBlocks.UnitTests.Web;

public class HealthCheckCommandTests
{
    [Theory]
    [InlineData(new[] { "--healthcheck" }, true)]
    [InlineData(new string[0], false)]
    [InlineData(new[] { "--urls", "http://+:8080" }, false)]
    public void IsRequested_OnlyWithTheHealthcheckArgument(string[] args, bool expected) =>
        HealthCheckCommand.IsRequested(args).ShouldBe(expected);

    [Theory]
    [InlineData(null, "http://localhost:8080/health/live")]
    [InlineData("", "http://localhost:8080/health/live")]
    [InlineData("5100", "http://localhost:5100/health/live")]
    [InlineData("8080;8081", "http://localhost:8080/health/live")]
    public void LiveEndpoint_UsesTheFirstConfiguredHttpPort(string? httpPorts, string expected) =>
        HealthCheckCommand.LiveEndpoint(httpPorts).ShouldBe(new Uri(expected));

    [Fact]
    public async Task RunAsync_Returns0_WhenTheLiveEndpointAnswers200()
    {
        using var client = new HttpClient(new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)));

        var exitCode = await HealthCheckCommand.RunAsync(client, new Uri("http://localhost:8080/health/live"), TestContext.Current.CancellationToken);

        exitCode.ShouldBe(0);
    }

    [Fact]
    public async Task RunAsync_Returns1_WhenTheLiveEndpointAnswersAnError()
    {
        using var client = new HttpClient(new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)));

        var exitCode = await HealthCheckCommand.RunAsync(client, new Uri("http://localhost:8080/health/live"), TestContext.Current.CancellationToken);

        exitCode.ShouldBe(1);
    }

    [Fact]
    public async Task RunAsync_Returns1_WhenNothingListens()
    {
        using var client = new HttpClient(new StubHandler(_ => throw new HttpRequestException("Connection refused")));

        var exitCode = await HealthCheckCommand.RunAsync(client, new Uri("http://localhost:8080/health/live"), TestContext.Current.CancellationToken);

        exitCode.ShouldBe(1);
    }

    private sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(respond(request));
    }
}
