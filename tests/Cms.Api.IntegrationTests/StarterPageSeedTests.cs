using Cms.Domain.Entities;
using Cms.Domain.Enums;
using Cms.Infrastructure.Persistence;
using Cms.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Cms.Api.IntegrationTests;

/// <summary>
/// Starter pages are given to a website once, when it has none.
///
/// They used to be topped up on every start: any starter slug missing from the list was added
/// back. So a page an administrator deleted came back the next time the application restarted,
/// and its link in the top bar with it — which is what "delete does not work" looked like from
/// the outside, because nothing about the deletion had failed.
/// </summary>
public sealed class StarterPageSeedTests : IClassFixture<PublicWebFactory>
{
    private readonly PublicWebFactory _factory;

    public StarterPageSeedTests(PublicWebFactory factory) => _factory = factory;

    [Fact]
    public async Task AWebsiteWithNoPages_IsGivenTheStarterSet()
    {
        var (db, tenantId, siteId) = await ArrangeAsync();

        await SchoolWebsiteSeed.EnsureAsync(db, tenantId, siteId, HomeVariant.Classic, "Test", "Tagline");

        var pages = await db.Pages.IgnoreQueryFilters().Where(x => x.SiteId == siteId).CountAsync();
        Assert.True(pages > 0, "a brand new website should be given its starter pages");
    }

    [Fact]
    public async Task ADeletedPage_DoesNotComeBackOnTheNextStart()
    {
        var (db, tenantId, siteId) = await ArrangeAsync();

        await SchoolWebsiteSeed.EnsureAsync(db, tenantId, siteId, HomeVariant.Classic, "Test", "Tagline");

        var doomed = await db.Pages.IgnoreQueryFilters().FirstAsync(x => x.SiteId == siteId);
        var slug = doomed.Slug;
        db.Pages.Remove(doomed);
        await db.SaveChangesAsync();

        // The application restarts, and the seeder runs again.
        await SchoolWebsiteSeed.EnsureAsync(db, tenantId, siteId, HomeVariant.Classic, "Test", "Tagline");

        var slugs = await db.Pages.IgnoreQueryFilters()
            .Where(x => x.SiteId == siteId).Select(x => x.Slug).ToListAsync();

        Assert.DoesNotContain(slug, slugs);
    }

    private async Task<(ApplicationDbContext Db, Guid TenantId, Guid SiteId)> ArrangeAsync()
    {
        var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var tenantId = await db.Tenants.IgnoreQueryFilters().Select(x => x.Id).FirstAsync();
        var site = new Site
        {
            TenantId = tenantId,
            Name = "Seed Test School",
            SiteKey = $"s{Guid.NewGuid():N}"[..12],
            IsActive = true
        };
        db.Sites.Add(site);
        await db.SaveChangesAsync();

        return (db, tenantId, site.Id);
    }
}
