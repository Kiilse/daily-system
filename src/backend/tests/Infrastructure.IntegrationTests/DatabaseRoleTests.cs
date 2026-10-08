using Npgsql;

namespace Infrastructure.IntegrationTests;

/// <summary>
/// Least privilege on every product database (ADR-006): {db}_migrator owns the schema, {db}_app reads and
/// writes data only, and no role reaches a database it does not belong to.
/// </summary>
public class DatabaseRoleTests(BootstrappedPostgres postgres) : IClassFixture<BootstrappedPostgres>
{
    public static TheoryData<string> Databases => [.. BootstrappedPostgres.Databases];

    public static TheoryData<string, string> RolesAndForeignDatabases
    {
        get
        {
            var data = new TheoryData<string, string>();
            foreach (var database in BootstrappedPostgres.Databases)
            {
                foreach (var suffix in BootstrappedPostgres.RoleSuffixes)
                {
                    foreach (var foreign in BootstrappedPostgres.Databases.Where(other => other != database))
                    {
                        data.Add($"{database}_{suffix}", foreign);
                    }
                }
            }

            return data;
        }
    }

    [Theory]
    [MemberData(nameof(Databases))]
    public async Task AppRole_CannotCreateTable(string database)
    {
        await using var app = await postgres.OpenAsync(database, $"{database}_app");

        var exception = await Should.ThrowAsync<PostgresException>(() => ExecuteAsync(app, "CREATE TABLE intruder (id int)"));

        exception.SqlState.ShouldBe(PostgresErrorCodes.InsufficientPrivilege);
    }

    [Theory]
    [MemberData(nameof(RolesAndForeignDatabases))]
    public async Task Role_CannotConnectToAnotherProductDatabase(string role, string foreignDatabase)
    {
        var exception = await Should.ThrowAsync<PostgresException>(() => postgres.OpenAsync(foreignDatabase, role));

        exception.SqlState.ShouldBe(PostgresErrorCodes.InsufficientPrivilege);
    }

    [Theory]
    [MemberData(nameof(Databases))]
    public async Task AppRole_ReadsAndWritesTablesCreatedByTheMigrator(string database)
    {
        var table = await CreateTableAsMigratorAsync(database);
        await using var app = await postgres.OpenAsync(database, $"{database}_app");

        await ExecuteAsync(app, $"INSERT INTO {table} (name) VALUES ('first')");
        await ExecuteAsync(app, $"UPDATE {table} SET name = 'renamed'");
        var rows = await ScalarAsync(app, $"SELECT count(*) FROM {table} WHERE name = 'renamed'");
        await ExecuteAsync(app, $"DELETE FROM {table}");

        rows.ShouldBe(1L);
    }

    [Theory]
    [MemberData(nameof(Databases))]
    public async Task AppRole_CannotDropTablesCreatedByTheMigrator(string database)
    {
        var table = await CreateTableAsMigratorAsync(database);
        await using var app = await postgres.OpenAsync(database, $"{database}_app");

        var exception = await Should.ThrowAsync<PostgresException>(() => ExecuteAsync(app, $"DROP TABLE {table}"));

        exception.SqlState.ShouldBe(PostgresErrorCodes.InsufficientPrivilege);
    }

    [Fact]
    public async Task RoleWithoutGrant_CannotConnectToAnyProductDatabase()
    {
        await postgres.ExecuteAsSuperuserAsync("CREATE ROLE outsider LOGIN PASSWORD 'outsider-test-password'");

        foreach (var database in BootstrappedPostgres.Databases)
        {
            var exception = await Should.ThrowAsync<PostgresException>(() => postgres.OpenAsync(database, "outsider", "outsider-test-password"));

            exception.SqlState.ShouldBe(PostgresErrorCodes.InsufficientPrivilege, $"outsider reached {database}");
        }
    }

    /// <summary>Creates a uniquely named table as {database}_migrator, the way a migration would.</summary>
    private async Task<string> CreateTableAsMigratorAsync(string database)
    {
        var table = $"probe_{Guid.NewGuid():N}";
        await using var migrator = await postgres.OpenAsync(database, $"{database}_migrator");
        await ExecuteAsync(migrator, $"CREATE TABLE {table} (id int GENERATED ALWAYS AS IDENTITY PRIMARY KEY, name text NOT NULL)");
        return table;
    }

    private static async Task ExecuteAsync(NpgsqlConnection connection, string sql)
    {
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync(TestContext.Current.CancellationToken);
    }

    private static async Task<object?> ScalarAsync(NpgsqlConnection connection, string sql)
    {
        await using var command = new NpgsqlCommand(sql, connection);
        return await command.ExecuteScalarAsync(TestContext.Current.CancellationToken);
    }
}
