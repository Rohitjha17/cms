using Cms.Application.Interfaces;
using Cms.Domain.Common;
using Cms.Domain.Entities;
using Cms.Infrastructure.Persistence;
using Cms.Infrastructure.Tenancy;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Cms.Api.IntegrationTests;

/// <summary>
/// Every entity assigns its own key in its constructor. Unless the model says so, EF assumes a
/// key it did not generate belongs to a row that already exists, and saves a brand new child
/// reached through a loaded parent as an UPDATE against a row that was never inserted:
/// "the database operation was expected to affect 1 row(s), but actually affected 0 row(s)".
///
/// That took out saving a page, saving the navigation, and adding a domain to a tenant — every
/// one of them adds a child to a parent that already exists. It only worked while the parent was
/// new as well, which is why creating a website succeeded and editing it afterwards did not.
/// </summary>
public sealed class EntityKeyTrackingTests
{
    [Fact]
    public void EveryEntityKey_IsApplicationGenerated()
    {
        using var context = NewContext(out var connection);
        using var _ = connection;

        var storeGenerated = context.Model.GetEntityTypes()
            .Where(entity => typeof(BaseEntity).IsAssignableFrom(entity.ClrType))
            .Select(entity => new
            {
                Entity = entity.ClrType.Name,
                Key = entity.FindPrimaryKey()?.Properties.FirstOrDefault(p => p.Name == nameof(BaseEntity.Id))
            })
            .Where(x => x.Key is not null && x.Key.ValueGenerated != ValueGenerated.Never)
            .Select(x => x.Entity)
            .ToList();

        Assert.True(
            storeGenerated.Count == 0,
            "These entities would be saved as updates instead of inserts: " + string.Join(", ", storeGenerated));
    }

    /// <summary>
    /// The failure in the operator's own words: fill a page in, save it, and get a server error
    /// every time. This is that save, reduced to the database work it does.
    /// </summary>
    [Fact]
    public async Task AddingAChildToAnAlreadySavedParent_Inserts()
    {
        using var context = NewContext(out var connection);
        using var _ = connection;
        await context.Database.EnsureCreatedAsync();

        var tenantId = Guid.NewGuid();
        var siteId = Guid.NewGuid();
        var menu = new Menu
        {
            TenantId = tenantId,
            SiteId = siteId,
            Name = "Main navigation",
            Location = "header"
        };
        menu.Items.Add(new MenuItem { TenantId = tenantId, SiteId = siteId, Label = "Home", Url = "/" });

        context.Menus.Add(menu);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Reload, then add a link the way saving a page does.
        var saved = await context.Menus.IgnoreQueryFilters()
            .Include(x => x.Items)
            .FirstAsync(x => x.Id == menu.Id);
        saved.Items.Add(new MenuItem
        {
            TenantId = tenantId,
            SiteId = siteId,
            MenuId = saved.Id,
            Label = "About",
            Url = "/about"
        });

        await context.SaveChangesAsync();

        var labels = await context.MenuItems.IgnoreQueryFilters()
            .Where(x => x.MenuId == menu.Id)
            .Select(x => x.Label)
            .ToListAsync();

        Assert.Equal(2, labels.Count);
        Assert.Contains("About", labels);
    }

    private static ApplicationDbContext NewContext(out SqliteConnection connection)
    {
        // A relational provider is required: the in-memory provider reports no affected row
        // counts, so it cannot see this class of failure at all.
        connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        return new ApplicationDbContext(options, new TenantContext(), new SiteContext());
    }
}
