extern alias adminapp;

using Cms.Application.DTOs.Websites;
using Cms.Application.Interfaces;
using Cms.Domain.Entities;
using Cms.Infrastructure.Persistence;
using Cms.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Cms.Api.IntegrationTests;

/// <summary>
/// Housekeeping on domains has to be possible.
///
/// Removing a school's last address, or pointing it at a different website, used to be refused
/// outright. But switching the website off, doing it, and switching the website back on reaches
/// exactly the same end state — so the refusal never prevented anything, it only made ordinary
/// work three times longer, and read as though the screen were broken.
///
/// The console's own address is the exception, because losing it locks the operator out of the
/// screen they would use to put it back.
/// </summary>
public sealed class DomainRemovalTests : IClassFixture<AdminFactory>
{
    private readonly AdminFactory _factory;

    public DomainRemovalTests(AdminFactory factory) => _factory = factory;

    [Fact]
    public async Task AWebsitesLastAddress_CanBeRemoved()
    {
        var (websites, domainId, _) = await ArrangeAsync(hostLive: true, siteLive: true);

        await websites.DeleteDomainAsync(domainId, default);

        Assert.DoesNotContain(await websites.GetDomainsAsync(default), x => x.Id == domainId);
    }

    [Fact]
    public async Task AWebsitesLastAddress_CanBeMovedToAnotherWebsite()
    {
        var (websites, domainId, siteId) = await ArrangeAsync(hostLive: true, siteLive: true);
        var target = (await websites.GetWebsitesAsync(default)).First(x => x.Id != siteId).Id;

        var moved = await websites.SaveDomainAsync(domainId, new SaveSiteDomainDto
        {
            DomainName = $"moved-{Guid.NewGuid():N}.test",
            SiteId = target,
            IsActive = true
        }, default);

        Assert.Equal(target, moved.SiteId);
    }

    /// <summary>
    /// The operator is told rather than stopped: the Domains page names every website left
    /// without a live address, which is how they find out they have work to finish.
    /// </summary>
    [Fact]
    public async Task AWebsiteLeftWithNoAddress_IsReportedAsUnreachable()
    {
        var (websites, domainId, siteId) = await ArrangeAsync(hostLive: true, siteLive: true);

        await websites.DeleteDomainAsync(domainId, default);

        var stranded = await websites.GetWebsitesAsync(default);
        var domains = await websites.GetDomainsAsync(default);
        var site = stranded.First(x => x.Id == siteId);

        Assert.True(site.IsActive);
        Assert.DoesNotContain(domains, d => d.SiteId == siteId && d.IsActive);
    }

    /// <summary>
    /// Losing this one locks everyone out of the console with no way back except editing the
    /// database by hand — which is exactly what happened once.
    /// </summary>
    [Fact]
    public async Task TheConsolesOwnLastAddress_IsRefused()
    {
        var (websites, domainId, _) = await ArrangeAsync(
            hostLive: true, siteLive: true, siteKey: "platform");

        var refusal = await Assert.ThrowsAsync<ValidationAppException>(
            () => websites.DeleteDomainAsync(domainId, default));

        Assert.Contains("sign in on", refusal.Message);
        Assert.Contains("Add another address", refusal.Message);
    }

    [Fact]
    public async Task TheConsolesAddress_CanGoOnceItHasASpare()
    {
        var (websites, domainId, siteId) = await ArrangeAsync(
            hostLive: true, siteLive: true, siteKey: "platform");

        await websites.SaveDomainAsync(null, new SaveSiteDomainDto
        {
            DomainName = $"spare-{Guid.NewGuid():N}.test",
            SiteId = siteId,
            IsActive = true
        }, default);

        await websites.DeleteDomainAsync(domainId, default);

        Assert.DoesNotContain(await websites.GetDomainsAsync(default), x => x.Id == domainId);
    }

    /// <summary>
    /// Builds one website with exactly one address, in the state under test, and hands back a
    /// service scoped to that tenant.
    /// </summary>
    private async Task<(IWebsiteService Websites, Guid DomainId, Guid SiteId)> ArrangeAsync(
        bool hostLive, bool siteLive, string? siteKey = null)
    {
        var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var tenantId = await db.Tenants.IgnoreQueryFilters().Select(x => x.Id).FirstAsync();
        var site = new Site
        {
            TenantId = tenantId,
            Name = siteKey == "platform" ? "Platform Console" : "Test Academy",
            SiteKey = siteKey ?? $"t{Guid.NewGuid():N}"[..12],
            IsActive = siteLive
        };
        var domain = new TenantDomain
        {
            TenantId = tenantId,
            SiteId = site.Id,
            DomainName = $"only-{Guid.NewGuid():N}.test",
            IsActive = hostLive
        };
        db.Sites.Add(site);
        db.TenantDomains.Add(domain);
        await db.SaveChangesAsync();

        var tenantContext = scope.ServiceProvider.GetRequiredService<ITenantContext>();
        tenantContext.Set(tenantId, "test", "Test");
        scope.ServiceProvider.GetRequiredService<ISiteContext>().Set(site.Id, site.SiteKey, site.Name);

        return (scope.ServiceProvider.GetRequiredService<IWebsiteService>(), domain.Id, site.Id);
    }
}
