using BusBookingSystem.Infrastructure.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BusBookingSystem.Tests.SqlServer;

/// <summary>
/// Creates a throwaway SQL Server database with all migrations applied, and drops it afterwards.
/// Server comes from the BUSBOOKING_TEST_SQLSERVER environment variable, defaulting to local SQL Express.
/// When no server is reachable, <see cref="IsAvailable"/> is false and tests skip instead of failing.
/// </summary>
public sealed class SqlServerDatabaseFixture : IAsyncLifetime
{
    private const string DefaultServerConnection =
        @"Server=(local)\SQLEXPRESS;Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=3;";

    public string ConnectionString { get; }
    public bool IsAvailable { get; private set; }
    public string? UnavailableReason { get; private set; }

    public SqlServerDatabaseFixture()
    {
        var server = Environment.GetEnvironmentVariable("BUSBOOKING_TEST_SQLSERVER") ?? DefaultServerConnection;
        ConnectionString = new SqlConnectionStringBuilder(server)
        {
            InitialCatalog = $"BusBookingSystem_Tests_{Guid.NewGuid():N}"
        }.ConnectionString;
    }

    public AppDbContext CreateContext(params IInterceptor[] interceptors) =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(ConnectionString)
            .AddInterceptors(interceptors)
            .Options);

    public async Task InitializeAsync()
    {
        try
        {
            await using var context = CreateContext();
            await context.Database.MigrateAsync();
            IsAvailable = true;
        }
        catch (SqlException ex)
        {
            UnavailableReason = $"SQL Server not available: {ex.Message}";
        }
    }

    public async Task DisposeAsync()
    {
        if (!IsAvailable)
        {
            return;
        }

        await using var context = CreateContext();
        await context.Database.EnsureDeletedAsync();
    }
}
