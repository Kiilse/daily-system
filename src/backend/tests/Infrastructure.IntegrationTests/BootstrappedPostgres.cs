using DotNet.Testcontainers.Builders;
using Npgsql;
using Testcontainers.PostgreSql;

namespace Infrastructure.IntegrationTests;

/// <summary>
/// A throwaway PostgreSQL bootstrapped by infra/postgres/init/01-databases.sql, the same way
/// <c>docker compose up</c> does it: the script is mounted in /docker-entrypoint-initdb.d/ and reads
/// the role passwords from environment variables. Started once and shared by every test of a class.
/// </summary>
public sealed class BootstrappedPostgres : IAsyncLifetime
{
    public static readonly IReadOnlyList<string> Databases = ["bff_db", "account_db", "menu_db", "calendar_db"];

    public static readonly IReadOnlyList<string> RoleSuffixes = ["app", "migrator"];

    private readonly PostgreSqlContainer _container;

    public BootstrappedPostgres()
    {
        var builder = new PostgreSqlBuilder(InfraFiles.PostgresImage())
            .WithResourceMapping(InfraFiles.InitScript(), "/docker-entrypoint-initdb.d/")
            // The entrypoint runs init scripts on a temporary server that only listens on the Unix socket.
            // Probing over TCP waits for the real server, started once every script has run.
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilCommandIsCompleted("pg_isready", "--host=127.0.0.1", "--username=postgres"));

        foreach (var role in Databases.SelectMany(database => RoleSuffixes.Select(suffix => $"{database}_{suffix}")))
        {
            builder = builder.WithEnvironment(PasswordVariable(role), Password(role));
        }

        _container = builder.Build();
    }

    /// <summary>Opens a connection to <paramref name="database"/> as one of the roles the bootstrap script creates.</summary>
    public Task<NpgsqlConnection> OpenAsync(string database, string role) => OpenAsync(database, role, Password(role));

    public async Task<NpgsqlConnection> OpenAsync(string database, string role, string password)
    {
        var connectionString = new NpgsqlConnectionStringBuilder
        {
            Host = _container.Hostname,
            Port = _container.GetMappedPublicPort(PostgreSqlBuilder.PostgreSqlPort),
            Database = database,
            Username = role,
            Password = password,
            Pooling = false,
        }.ConnectionString;

        var connection = new NpgsqlConnection(connectionString);
        try
        {
            await connection.OpenAsync(TestContext.Current.CancellationToken);
            return connection;
        }
        catch
        {
            await connection.DisposeAsync();
            throw;
        }
    }

    /// <summary>Runs SQL as the superuser, to set up a case the bootstrap script does not cover.</summary>
    public async Task ExecuteAsSuperuserAsync(string sql)
    {
        var result = await _container.ExecScriptAsync(sql, TestContext.Current.CancellationToken);
        if (result.ExitCode != 0)
        {
            throw new InvalidOperationException($"psql failed: {result.Stderr}");
        }
    }

    public async ValueTask InitializeAsync() => await _container.StartAsync();

    public async ValueTask DisposeAsync() => await _container.DisposeAsync();

    /// <summary>menu_db_app reads its password from MENU_DB_APP_PASSWORD, as in infra/.env.example.</summary>
    private static string PasswordVariable(string role) => $"{role.ToUpperInvariant()}_PASSWORD";

    private static string Password(string role) => $"{role}-test-password";
}
