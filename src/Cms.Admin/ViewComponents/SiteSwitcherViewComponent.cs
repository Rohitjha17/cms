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

        // A SuperAdmin works across institutions and picks one here; everybody else is fixed to
        // their own and is not shown the others at all.
        var institutions = HttpContext.User.IsInRole(Cms.Domain.Constants.AppRoles.SuperAdmin)
            ? await _db.Tenants
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .Select(x => new InstitutionItem(x.Id, x.Name, x.Code))
                .ToListAsync()
            : [];

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
            CurrentTenantId = _tenantContext.TenantId,
            CurrentTenantName = _tenantContext.TenantName,
            Institutions = institutions,
            Sites = sites
        });
    }
}

public sealed class SiteSwitcherViewModel
{
    public string? CurrentSiteKey { get; init; }
    public string? CurrentSiteName { get; init; }
    public IReadOnlyList<SiteSwitcherItem> Sites { get; init; } = [];
    public Guid? CurrentTenantId { get; init; }
    public string? CurrentTenantName { get; init; }

    /// <summary>Every active institution — filled only for a SuperAdmin.</summary>
    public IReadOnlyList<InstitutionItem> Institutions { get; init; } = [];
}

public sealed record InstitutionItem(Guid Id, string Name, string Code);

public sealed record SiteSwitcherItem(string Key, string Name, string Type);
