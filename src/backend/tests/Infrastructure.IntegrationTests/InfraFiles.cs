using System.Text.RegularExpressions;

namespace Infrastructure.IntegrationTests;

/// <summary>The files under infra/ that these tests exercise, read from the repository, never copied.</summary>
internal static partial class InfraFiles
{
    public static FileInfo InitScript() => new(Path.Combine(InfraRoot(), "postgres", "init", "01-databases.sql"));

    /// <summary>
    /// The PostgreSQL image pinned in docker-compose.yml, so the tests always run the same image as the
    /// local environment (Dependabot updates the digest in one place).
    /// </summary>
    public static string PostgresImage()
    {
        var compose = File.ReadAllText(Path.Combine(InfraRoot(), "docker-compose.yml"));
        var match = PostgresImageLine().Match(compose);
        return match.Success
            ? match.Groups["image"].Value
            : throw new InvalidOperationException("No 'image: postgres:...' line found in infra/docker-compose.yml.");
    }

    /// <summary>infra/, found by walking up from the test binaries to src/backend/Ecosystem.slnx.</summary>
    private static string InfraRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Ecosystem.slnx")))
        {
            directory = directory.Parent;
        }

        var backendRoot = directory ?? throw new InvalidOperationException("Ecosystem.slnx not found above the test binaries.");
        return Path.GetFullPath(Path.Combine(backendRoot.FullName, "..", "..", "infra"));
    }

    [GeneratedRegex(@"^\s*image:\s*(?<image>postgres:\S+)\s*$", RegexOptions.Multiline)]
    private static partial Regex PostgresImageLine();
}
