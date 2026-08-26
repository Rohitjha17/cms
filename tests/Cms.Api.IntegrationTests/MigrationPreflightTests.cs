using Cms.Application.Interfaces;
using Cms.Infrastructure.Persistence;
using Cms.Infrastructure.Tenancy;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Cms.Api.IntegrationTests;

/// <summary>
/// A database built by an earlier, incomplete attempt cannot be migrated forward. Left to
/// itself the attempt dies on "There is already an object named 'ActivityLogs'", which names
/// neither the cause nor the cure — and the near miss is worse still, because the site starts
/// and only fails once somebody opens a page.
/// </summary>
public sealed class MigrationPreflightTests
{
    [Fact]
    public async Task AnEmptyDatabase_IsAcceptedForMigration()
    {
        using var connection = Open();
        using var db = NewContext(connection);

        var exception = await Record.ExceptionAsync(() => MigrationPreflight.EnsureCanMigrateAsync(db));

        Assert.Null(exception);
    }

    [Fact]
    public async Task ADatabaseWithTablesButNoMigrationHistory_IsRefusedWithAnExplanation()
    {
        using var connection = Open();

        // Exactly the shape an interrupted attempt leaves: tables, no record of the migrations.
        using (var command = connection.CreateCommand())
        {
            command.CommandText = "CREATE TABLE ActivityLogs (Id TEXT PRIMARY KEY);";
            command.ExecuteNonQuery();
        }

        using var db = NewContext(connection);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => MigrationPreflight.EnsureCanMigrateAsync(db));

        Assert.Contains("already contains tables", exception.Message);
        Assert.Contains("DROP DATABASE", exception.Message);
    }

    private static SqliteConnection Open()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        return connection;
    }

    private static ApplicationDbContext NewContext(SqliteConnection connection) =>
        new(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options,
            new TenantContext(),
            new SiteContext());
}
