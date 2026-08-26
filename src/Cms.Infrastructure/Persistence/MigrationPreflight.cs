using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Cms.Infrastructure.Persistence;

/// <summary>
/// Checks that the database can actually be migrated before trying.
///
/// A database holding tables that these migrations did not create cannot be brought forward:
/// the first migration tries to create what is already there and the request dies on
/// "There is already an object named 'ActivityLogs'" — which says nothing about the cause or the
/// cure. Worse is the near miss, where the tables exist but predate a column: the site starts
/// and then fails mid-page with "Invalid column name 'SiteId'".
///
/// Both mean the same thing, and both have the same answer, so say it plainly instead.
/// </summary>
public static class MigrationPreflight
{
    public static async Task EnsureCanMigrateAsync(DbContext db, CancellationToken cancellationToken = default)
    {
        var applied = await db.Database.GetAppliedMigrationsAsync(cancellationToken);
        if (applied.Any())
        {
            // Migrated before: EF can take it from wherever it left off.
            return;
        }

        var pending = await db.Database.GetPendingMigrationsAsync(cancellationToken);
        if (!pending.Any())
        {
            return;
        }

        var creator = db.Database.GetService<IRelationalDatabaseCreator>();
        if (!await creator.HasTablesAsync(cancellationToken))
        {
            // Empty database: exactly what the migrations expect.
            return;
        }

        throw new InvalidOperationException(
            "This database already contains tables, but no record of these migrations having "
            + "created them. That happens when it was built by an earlier, incomplete attempt. "
            + "Migrating on top of it cannot work — the first migration would try to create "
            + "tables that are already there."
            + Environment.NewLine + Environment.NewLine
            + "Point the application at an empty database, or drop this one and let it be "
            + "recreated:"
            + Environment.NewLine
            + "    USE master;"
            + Environment.NewLine
            + "    ALTER DATABASE [YourDatabase] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;"
            + Environment.NewLine
            + "    DROP DATABASE [YourDatabase];"
            + Environment.NewLine + Environment.NewLine
            + "If it holds content worth keeping, back it up first — this discards everything.");
    }
}
