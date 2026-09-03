extern alias webapp;

using Cms.Application.DTOs.Websites;
using Cms.Application.Interfaces;
using Cms.Domain.Entities;
using Cms.Infrastructure.Persistence;
using Cms.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Cms.Api.IntegrationTests;

/// <summary>
/// A website's last live address must not be removable, or a school disappears off the internet
/// because someone tidied a list — which is exactly how the console locked its own operator out.
///
/// But the guard was checking only "is this the last one", not "is there anything to lose". A
/// host already switched off is serving nobody, and a website switched off has nobody to strand,
/// so both were refused for a danger that did not exist. That left a school's only domain
/// impossible to remove, impossible to disable, and impossible to get rid of at all.
/// </summary>
public sealed class DomainRemovalTests : IClassFixture<AdminFactory>
{
    private readonly AdminFactory _factory;

    public DomainRemovalTests(AdminFactory factory) => _factory = factory;

    [Fact]
    public async Task TheLastLiveAddressOfALiveWebsite_CannotBeRemoved()
    {
        var (websites, domainId, _) = await ArrangeAsync(hostLive: true, siteLive: true);

        var refusal = await Assert.ThrowsAsync<ValidationAppException>(
            () => websites.DeleteDomainAsync(domainId, default));

        // The refusal has to say the way out, or it is a locked door with no key.
        Assert.Contains("Add another domain", refusal.Message);
        Assert.Contains("switch the website off", refusal.Message);
    }

    [Fact]
    public async Task AHostThatIsAlreadySwitchedOff_CanBeRemoved()
    {
        var (websites, domainId, _) = await ArrangeAsync(hostLive: false, siteLive: true);

        await websites.DeleteDomainAsync(domainId, default);

        Assert.DoesNotContain(await websites.GetDomainsAsync(default), x => x.Id == domainId);
    }

    [Fact]
    public async Task WhenTheWebsiteItselfIsSwitchedOff_ItsAddressCanBeRemoved()
    {
        var (websites, domainId, _) = await ArrangeAsync(hostLive: true, siteLive: false);

        await websites.DeleteDomainAsync(domainId, default);

        Assert.DoesNotContain(await websites.GetDomainsAsync(default), x => x.Id == domainId);
    }

    /// <summary>With a second live address there is nothing to strand, so the first may go.</summary>
    [Fact]
    public async Task WithAnotherLiveAddress_TheFirstCanBeRemoved()
    {
        var (websites, domainId, siteId) = await ArrangeAsync(hostLive: true, siteLive: true);
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
        bool hostLive, bool siteLive)
    {
        var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var tenantId = await db.Tenants.IgnoreQueryFilters().Select(x => x.Id).FirstAsync();
        var site = new Site
        {
            TenantId = tenantId,
            Name = "Test Academy",
            SiteKey = $"t{Guid.NewGuid():N}"[..12],
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
