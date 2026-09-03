extern alias adminapp;

using Cms.Application.DTOs.Websites;
using Cms.Application.Interfaces;
using Cms.Domain.Entities;
using Cms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Cms.Api.IntegrationTests;

/// <summary>
/// Domains are the operator's to arrange.
///
/// Removing a website's last address, or pointing it at a different website, used to be refused.
/// Every refusal could be walked around by switching the website off, doing the thing, and
/// switching it back on — the same end state, three times the clicks — so nothing was ever
/// prevented. What it did do was read as the screen being broken.
///
/// The Domains page names every website left without a live address, in a warning above the
/// list. That is the whole safety net now, and it is a truer account of what happened than a
/// refusal that could be sidestepped.
/// </summary>
public sealed class DomainRemovalTests : IClassFixture<AdminFactory>
{
    private readonly AdminFactory _factory;

    public DomainRemovalTests(AdminFactory factory) => _factory = factory;

    [Fact]
    public async Task AWebsitesLastAddress_CanBeRemoved()
    {
        var (websites, domainId, _) = await ArrangeAsync(siteKey: null);

        await websites.DeleteDomainAsync(domainId, default);

        Assert.DoesNotContain(await websites.GetDomainsAsync(default), x => x.Id == domainId);
    }

    [Fact]
    public async Task AWebsitesLastAddress_CanBeMovedToAnotherWebsite()
    {
        var (websites, domainId, siteId) = await ArrangeAsync(siteKey: null);
        var target = (await websites.GetWebsitesAsync(default)).First(x => x.Id != siteId).Id;

        var moved = await websites.SaveDomainAsync(domainId, new SaveSiteDomainDto
        {
            DomainName = $"moved-{Guid.NewGuid():N}.test",
            SiteId = target,
            IsActive = true
        }, default);

        Assert.Equal(target, moved.SiteId);
    }

    [Fact]
    public async Task AWebsitesLastAddress_CanBeSwitchedOff()
    {
        var (websites, domainId, siteId) = await ArrangeAsync(siteKey: null);

        var disabled = await websites.SaveDomainAsync(domainId, new SaveSiteDomainDto
        {
            DomainName = $"off-{Guid.NewGuid():N}.test",
            SiteId = siteId,
            IsActive = false
        }, default);

        Assert.False(disabled.IsActive);
    }

    /// <summary>
    /// The console's own address is no different. Losing it is recoverable without touching the
    /// database — the platform seed re-binds Platform__Domain every time the console starts —
    /// so refusing it only stopped the operator doing their own housekeeping.
    /// </summary>
    [Fact]
    public async Task TheConsolesOwnAddress_CanBeRemovedToo()
    {
        var (websites, domainId, _) = await ArrangeAsync(siteKey: "platform");

        await websites.DeleteDomainAsync(domainId, default);

        Assert.DoesNotContain(await websites.GetDomainsAsync(default), x => x.Id == domainId);
    }

    /// <summary>
    /// Stranding a website is allowed, but it must not be silent: the page reads this to warn
    /// the operator that a school is no longer reachable.
    /// </summary>
    [Fact]
    public async Task AWebsiteLeftWithNoAddress_IsStillLiveAndHasNone()
    {
        var (websites, domainId, siteId) = await ArrangeAsync(siteKey: null);

        await websites.DeleteDomainAsync(domainId, default);

        var site = (await websites.GetWebsitesAsync(default)).First(x => x.Id == siteId);
        var domains = await websites.GetDomainsAsync(default);

        Assert.True(site.IsActive);
        Assert.DoesNotContain(domains, d => d.SiteId == siteId && d.IsActive);
    }

    /// <summary>
    /// Builds one website with exactly one live address and hands back a service scoped to that
    /// tenant.
    /// </summary>
    private async Task<(IWebsiteService Websites, Guid DomainId, Guid SiteId)> ArrangeAsync(
        string? siteKey)
    {
        var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var tenantId = await db.Tenants.IgnoreQueryFilters().Select(x => x.Id).FirstAsync();
        var site = new Site
        {
            TenantId = tenantId,
            Name = siteKey == "platform" ? "Platform Console" : "Test Academy",
            SiteKey = siteKey ?? $"t{Guid.NewGuid():N}"[..12],
            IsActive = true
        };
        var domain = new TenantDomain
        {
            TenantId = tenantId,
            SiteId = site.Id,
            DomainName = $"only-{Guid.NewGuid():N}.test",
            IsActive = true
        };
        db.Sites.Add(site);
        db.TenantDomains.Add(domain);
        await db.SaveChangesAsync();

        scope.ServiceProvider.GetRequiredService<ITenantContext>().Set(tenantId, "test", "Test");
        scope.ServiceProvider.GetRequiredService<ISiteContext>().Set(site.Id, site.SiteKey, site.Name);

        return (scope.ServiceProvider.GetRequiredService<IWebsiteService>(), domain.Id, site.Id);
    }
}
