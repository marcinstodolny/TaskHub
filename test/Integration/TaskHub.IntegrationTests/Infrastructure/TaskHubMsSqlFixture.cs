using Microsoft.Data.SqlClient;
using Respawn;
using Respawn.Graph;
using Testcontainers.MsSql;
using Xunit;

namespace TaskHub.IntegrationTests.Infrastructure;

public sealed class TaskHubMsSqlFixture : IAsyncLifetime
{
    private const string DatabaseName = "TaskHubTests";

    private readonly MsSqlContainer _container = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-CU14-ubuntu-22.04")
         .WithPassword("TestPassword123!")
        .Build();

    private string? _connectionString;

    public string ConnectionString => _connectionString ?? throw new InvalidOperationException("Database has not been initialized.");

    private Respawner? Respawner { get; set; }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        var builder = new SqlConnectionStringBuilder(_container.GetConnectionString())
        {
            InitialCatalog = DatabaseName
        };

        _connectionString = builder.ConnectionString;
    }

    public async Task InitializeRespawnerAsync()
    {
        await using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();

        Respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.SqlServer,
            SchemasToInclude = new[] { "dbo" },
            TablesToIgnore = new Table[] { "__EFMigrationsHistory" }
        });
    }

    public async Task ResetDatabaseAsync()
    {
        if (Respawner is null)
        {
            await InitializeRespawnerAsync();
        }

        await using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();

        await Respawner!.ResetAsync(connection);

    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}