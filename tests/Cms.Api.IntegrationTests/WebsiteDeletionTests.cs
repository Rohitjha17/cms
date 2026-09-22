using Cms.Domain.Entities;
using Cms.Infrastructure.Persistence;
using Cms.Infrastructure.Repositories;
using Cms.Infrastructure.Tenancy;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Cms.Api.IntegrationTests;

/// <summary>
/// Deleting a website for good removes everything that belongs to it, in one transaction, and
/// nothing that does not.
///
/// This runs against SQLite rather than the in-memory provider the other tests use: the delete is
/// a set of bulk statements inside a transaction, and the in-memory provider supports neither, so
/// a test there would prove nothing about the database the code actually runs on.
/// </summary>
public sealed class WebsiteDeletionTests : IDisposable
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    private readonly ApplicationDbContext _db;

    public WebsiteDeletionTests()
    {
        _connection.Open();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(_connection).Options;
        _db = new ApplicationDbContext(options, new TenantContext(), new SiteContext());
        _db.Database.EnsureCreated();
    }

    [Fact]
    public async Task DeletingAWebsite_RemovesEverythingInIt_AndNothingElse()
    {
        var tenant = new Tenant { Name = "Green Valley", Code = "green-valley" };
        var doomed = new Site { TenantId = tenant.Id, Name = "Old Wing", SiteKey = "old-wing" };
        var kept = new Site { TenantId = tenant.Id, Name = "Main School", SiteKey = "school", IsDefault = true };
        _db.Tenants.Add(tenant);
        _db.Sites.AddRange(doomed, kept);
        await _db.SaveChangesAsync();

        foreach (var site in new[] { doomed, kept })
        {
            var page = new Page { TenantId = tenant.Id, SiteId = site.Id, Title = "About", Slug = "about" };
            var menu = new Menu { TenantId = tenant.Id, SiteId = site.Id, Name = "Header" };
            _db.Pages.Add(page);
            _db.Menus.Add(menu);
            _db.MenuItems.Add(new MenuItem { TenantId = tenant.Id, SiteId = site.Id, MenuId = menu.Id, Label = "About", Url = "/about" });
            _db.SeoSettings.Add(new SeoSetting { TenantId = tenant.Id, SiteId = site.Id });
            _db.ContentEntries.Add(new ContentEntry { TenantId = tenant.Id, SiteId = site.Id, ContentType = "news", Key = "k", Title = "News" });
            _db.HomePageSections.Add(new HomePageSection { TenantId = tenant.Id, SiteId = site.Id, SectionKey = "hero" });
            _db.MediaFiles.Add(new MediaFile
            {
                TenantId = tenant.Id, SiteId = site.Id, FileName = "a.png", OriginalFileName = "a.png",
                ContentType = "image/png", Url = $"/uploads/{site.SiteKey}/a.png", StorageKey = $"{site.SiteKey}/a.png"
            });
            _db.ContactSubmissions.Add(new ContactSubmission { TenantId = tenant.Id, SiteId = site.Id, Name = "P", Email = "p@x.in", Message = "Hi" });
            _db.TenantDomains.Add(new TenantDomain { TenantId = tenant.Id, SiteId = site.Id, DomainName = $"{site.SiteKey}.green.in" });
            _db.ActivityLogs.Add(new ActivityLog { TenantId = tenant.Id, SiteId = site.Id, Action = "Created", EntityType = "Page", EntityId = page.Id.ToString() });
        }
        await _db.SaveChangesAsync();
        _db.ChangeTracker.Clear();

        var storageKeys = await new WebsiteRepository(_db).DeleteSiteAsync(tenant.Id, doomed.Id, default);

        Assert.Equal(["old-wing/a.png"], storageKeys);
        Assert.False(await _db.Sites.IgnoreQueryFilters().AnyAsync(x => x.Id == doomed.Id));
        Assert.Equal(0, await CountAsync(doomed.Id));
        // History is kept, no longer pointing at a website that is gone.
        Assert.True(await _db.ActivityLogs.IgnoreQueryFilters().AnyAsync(x => x.TenantId == tenant.Id && x.SiteId == null));

        // The institution's other website is untouched, row for row.
        Assert.True(await _db.Sites.IgnoreQueryFilters().AnyAsync(x => x.Id == kept.Id));
        Assert.Equal(10, await CountAsync(kept.Id));
    }

    private async Task<int> CountAsync(Guid siteId) =>
        await _db.Pages.IgnoreQueryFilters().CountAsync(x => x.SiteId == siteId)
        + await _db.Menus.IgnoreQueryFilters().CountAsync(x => x.SiteId == siteId)
        + await _db.MenuItems.IgnoreQueryFilters().CountAsync(x => x.SiteId == siteId)
        + await _db.SeoSettings.IgnoreQueryFilters().CountAsync(x => x.SiteId == siteId)
        + await _db.ContentEntries.IgnoreQueryFilters().CountAsync(x => x.SiteId == siteId)
        + await _db.HomePageSections.IgnoreQueryFilters().CountAsync(x => x.SiteId == siteId)
        + await _db.MediaFiles.IgnoreQueryFilters().CountAsync(x => x.SiteId == siteId)
        + await _db.ContactSubmissions.IgnoreQueryFilters().CountAsync(x => x.SiteId == siteId)
        + await _db.TenantDomains.IgnoreQueryFilters().CountAsync(x => x.SiteId == siteId)
        + await _db.ActivityLogs.IgnoreQueryFilters().CountAsync(x => x.SiteId == siteId);

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }
}
