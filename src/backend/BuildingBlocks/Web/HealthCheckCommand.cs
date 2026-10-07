namespace BuildingBlocks.Web;

/// <summary>
/// <c>dotnet Host.dll --healthcheck</c>: calls the running app's /health/live and returns 0 (healthy) or 1.
/// Used as the container healthcheck, because chiseled images have no shell and no curl to do it.
/// Must run before the web host is built, so the check stays a short process with no side effect.
/// </summary>
public static class HealthCheckCommand
{
    public const string Argument = "--healthcheck";

    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(3);

    public static bool IsRequested(string[] args) => args.Contains(Argument, StringComparer.Ordinal);

    /// <summary>Checks the app listening in this container, on the port ASP.NET Core reads from ASPNETCORE_HTTP_PORTS.</summary>
    public static async Task<int> RunAsync()
    {
        using var client = new HttpClient { Timeout = Timeout };
        return await RunAsync(client, LiveEndpoint(Environment.GetEnvironmentVariable("ASPNETCORE_HTTP_PORTS")), CancellationToken.None);
    }

    public static async Task<int> RunAsync(HttpClient client, Uri endpoint, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(client);

        try
        {
            using var response = await client.GetAsync(endpoint, cancellationToken);
            return response.IsSuccessStatusCode ? 0 : 1;
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            return 1;
        }
    }

    /// <summary>
    /// /health/live on the first port of <paramref name="httpPorts"/> ("8080" or "8080;8081"),
    /// 8080 when unset like in the .NET images.
    /// </summary>
    public static Uri LiveEndpoint(string? httpPorts)
    {
        var port = httpPorts?.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault();
        return new Uri($"http://localhost:{(string.IsNullOrEmpty(port) ? "8080" : port)}/health/live");
    }
}
