using Cms.Application.Interfaces;
using Cms.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cms.Admin.ViewComponents;

public sealed class SiteSwitcherViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _db;
    private readonly ITenantContext _tenantContext;
    private readonly ISiteContext _siteContext;

    public SiteSwitcherViewComponent(
        ApplicationDbContext db,
        ITenantContext tenantContext,
        ISiteContext siteContext)
    {
        _db = db;
        _tenantContext = tenantContext;
        _siteContext = siteContext;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        if (_tenantContext.TenantId is null)
        {
            return View(new SiteSwitcherViewModel());
        }

        var sites = await _db.Sites
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.TenantId == _tenantContext.TenantId && x.IsActive)
            .OrderByDescending(x => x.IsDefault)
            .ThenBy(x => x.Name)
            .Select(x => new SiteSwitcherItem(x.SiteKey, x.Name, x.WebsiteType.ToString()))
            .ToListAsync();

        return View(new SiteSwitcherViewModel
        {
            CurrentSiteKey = _siteContext.SiteKey,
            CurrentSiteName = _siteContext.SiteName,
            Sites = sites
        });
    }
}

public sealed class SiteSwitcherViewModel
{
    public string? CurrentSiteKey { get; init; }
    public string? CurrentSiteName { get; init; }
    public IReadOnlyList<SiteSwitcherItem> Sites { get; init; } = [];
}

public sealed record SiteSwitcherItem(string Key, string Name, string Type);
