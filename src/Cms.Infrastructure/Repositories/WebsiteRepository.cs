using Cms.Application.Interfaces;
using Cms.Domain.Entities;
using Cms.Infrastructure.Persistence;
using Cms.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;

namespace Cms.Infrastructure.Repositories;

public sealed class WebsiteRepository : IWebsiteRepository
{
    private readonly ApplicationDbContext _db;

    public WebsiteRepository(ApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<PageTemplate>> GetPageTemplatesAsync(CancellationToken cancellationToken) =>
        await _db.PageTemplates
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

    public Task<PageTemplate?> GetPageTemplateAsync(string templateKey, CancellationToken cancellationToken) =>
        _db.PageTemplates
            .FirstOrDefaultAsync(x => x.TemplateKey == templateKey, cancellationToken);

    public Task<PageTemplate?> GetPageTemplateByIdAsync(Guid id, CancellationToken cancellationToken) =>
        _db.PageTemplates.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task AddPageTemplateAsync(PageTemplate template, CancellationToken cancellationToken) =>
        _db.PageTemplates.AddAsync(template, cancellationToken).AsTask();

    public async Task<IReadOnlyList<Site>> GetSitesAsync(Guid tenantId, CancellationToken cancellationToken) =>
        await _db.Sites.IgnoreQueryFilters()
            .Include(x => x.Domains)
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.IsDefault)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

    public Task<Site?> GetSiteAsync(Guid tenantId, Guid siteId, CancellationToken cancellationToken) =>
        _db.Sites.IgnoreQueryFilters()
            .Include(x => x.Domains)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == siteId, cancellationToken);

    public async Task<IReadOnlyList<string>> DeleteSiteAsync(
        Guid tenantId, Guid siteId, CancellationToken cancellationToken)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

        var storageKeys = await _db.MediaFiles.IgnoreQueryFilters()
            .Where(x => x.TenantId == tenantId && x.SiteId == siteId)
            .Select(x => x.StorageKey)
            .ToListAsync(cancellationToken);

        // Children before parents: menu items point at menus and pages.
        await _db.MenuItems.IgnoreQueryFilters()
            .Where(x => x.TenantId == tenantId && x.SiteId == siteId).ExecuteDeleteAsync(cancellationToken);
        await _db.Menus.IgnoreQueryFilters()
            .Where(x => x.TenantId == tenantId && x.SiteId == siteId).ExecuteDeleteAsync(cancellationToken);
        await _db.SeoSettings.IgnoreQueryFilters()
            .Where(x => x.TenantId == tenantId && x.SiteId == siteId).ExecuteDeleteAsync(cancellationToken);
        await _db.ContentEntries.IgnoreQueryFilters()
            .Where(x => x.TenantId == tenantId && x.SiteId == siteId).ExecuteDeleteAsync(cancellationToken);
        await _db.HomePageSections.IgnoreQueryFilters()
            .Where(x => x.TenantId == tenantId && x.SiteId == siteId).ExecuteDeleteAsync(cancellationToken);
        await _db.Pages.IgnoreQueryFilters()
            .Where(x => x.TenantId == tenantId && x.SiteId == siteId).ExecuteDeleteAsync(cancellationToken);
        await _db.ContactSubmissions.IgnoreQueryFilters()
            .Where(x => x.TenantId == tenantId && x.SiteId == siteId).ExecuteDeleteAsync(cancellationToken);
        await _db.MediaFiles.IgnoreQueryFilters()
            .Where(x => x.TenantId == tenantId && x.SiteId == siteId).ExecuteDeleteAsync(cancellationToken);
        await _db.TenantDomains.IgnoreQueryFilters()
            .Where(x => x.TenantId == tenantId && x.SiteId == siteId).ExecuteDeleteAsync(cancellationToken);
        await _db.ActivityLogs.IgnoreQueryFilters()
            .Where(x => x.TenantId == tenantId && x.SiteId == siteId)
            .ExecuteUpdateAsync(x => x.SetProperty(log => log.SiteId, (Guid?)null), cancellationToken);
        await _db.Sites.IgnoreQueryFilters()
            .Where(x => x.TenantId == tenantId && x.Id == siteId).ExecuteDeleteAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);
        return storageKeys;
    }

    public Task<Site?> GetSiteByKeyAsync(Guid tenantId, string siteKey, CancellationToken cancellationToken) =>
        _db.Sites.IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.SiteKey == siteKey, cancellationToken);

    public Task<TenantDomain?> GetDomainAsync(string domainName, CancellationToken cancellationToken) =>
        _db.TenantDomains.IgnoreQueryFilters().AsNoTracking()
            .FirstOrDefaultAsync(x => x.DomainName == domainName, cancellationToken);

    public async Task<IReadOnlyList<TenantDomain>> GetDomainsAsync(
        Guid tenantId, CancellationToken cancellationToken) =>
        await _db.TenantDomains.IgnoreQueryFilters()
            .Include(x => x.Site)
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.IsPrimary)
            .ThenBy(x => x.DomainName)
            .ToListAsync(cancellationToken);

    public Task<TenantDomain?> GetDomainByIdAsync(
        Guid tenantId, Guid id, CancellationToken cancellationToken) =>
        _db.TenantDomains.IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id, cancellationToken);

    public void RemoveDomain(TenantDomain domain) => _db.TenantDomains.Remove(domain);

    public Task AddSiteAsync(Site site, CancellationToken cancellationToken) =>
        _db.Sites.AddAsync(site, cancellationToken).AsTask();

    public Task AddDomainAsync(TenantDomain domain, CancellationToken cancellationToken) =>
        _db.TenantDomains.AddAsync(domain, cancellationToken).AsTask();

    public Task AddPageAsync(Page page, CancellationToken cancellationToken) =>
        _db.Pages.AddAsync(page, cancellationToken).AsTask();

    public Task AddMenuAsync(Menu menu, CancellationToken cancellationToken) =>
        _db.Menus.AddAsync(menu, cancellationToken).AsTask();

    public void RemoveMenuItems(IEnumerable<MenuItem> items) =>
        _db.MenuItems.RemoveRange(items);

    public Task AddSeoAsync(SeoSetting seo, CancellationToken cancellationToken) =>
        _db.SeoSettings.AddAsync(seo, cancellationToken).AsTask();

    public Task AddContactAsync(ContactSubmission submission, CancellationToken cancellationToken) =>
        _db.ContactSubmissions.AddAsync(submission, cancellationToken).AsTask();

    public async Task<IReadOnlyList<Page>> GetPagesAsync(
        Guid tenantId, Guid siteId, bool activeOnly, CancellationToken cancellationToken)
    {
        var query = _db.Pages.IgnoreQueryFilters()
            .Where(x => x.TenantId == tenantId && x.SiteId == siteId);
        if (activeOnly)
        {
            query = query.Where(x => x.IsActive);
        }

        return await query.OrderBy(x => x.MenuOrder).ThenBy(x => x.Title).ToListAsync(cancellationToken);
    }

    public Task<Page?> GetPageBySlugAsync(
        Guid tenantId, Guid siteId, string slug, CancellationToken cancellationToken) =>
        _db.Pages.IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.SiteId == siteId && x.Slug == slug, cancellationToken);

    public Task<Menu?> GetMenuByLocationAsync(
        Guid tenantId, Guid siteId, string location, CancellationToken cancellationToken) =>
        _db.Menus.IgnoreQueryFilters()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.SiteId == siteId && x.Location == location, cancellationToken);

    public Task<SeoSetting?> GetSeoAsync(Guid tenantId, Guid siteId, CancellationToken cancellationToken) =>
        _db.SeoSettings.IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.SiteId == siteId, cancellationToken);

    public async Task<IReadOnlyList<HomePageSection>> GetHomeSectionsAsync(
        Guid tenantId, Guid siteId, CancellationToken cancellationToken) =>
        await _db.HomePageSections.IgnoreQueryFilters()
            .Where(x => x.TenantId == tenantId && x.SiteId == siteId && x.IsActive)
            .OrderBy(x => x.DisplayOrder)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ContactSubmission>> GetContactsAsync(
        Guid tenantId, Guid siteId, CancellationToken cancellationToken) =>
        await _db.ContactSubmissions.IgnoreQueryFilters()
            .Where(x => x.TenantId == tenantId && x.SiteId == siteId)
            .OrderByDescending(x => x.CreatedDate)
            .ToListAsync(cancellationToken);

    public Task<int> CountUnreadContactsAsync(
        Guid tenantId, Guid siteId, CancellationToken cancellationToken) =>
        _db.ContactSubmissions.IgnoreQueryFilters()
            .CountAsync(x => x.TenantId == tenantId && x.SiteId == siteId && !x.IsRead, cancellationToken);

    public Task<ContactSubmission?> GetContactAsync(
        Guid tenantId, Guid siteId, Guid id, CancellationToken cancellationToken) =>
        _db.ContactSubmissions.IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.SiteId == siteId && x.Id == id, cancellationToken);

    public Task<int> CountPagesAsync(Guid tenantId, Guid siteId, CancellationToken cancellationToken) =>
        _db.Pages.IgnoreQueryFilters()
            .CountAsync(x => x.TenantId == tenantId && x.SiteId == siteId, cancellationToken);

    public Task EnsureHomeSectionsAsync(Guid tenantId, Guid siteId, CancellationToken cancellationToken) =>
        HomePageSeed.EnsureSectionsAsync(_db, tenantId, siteId, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        _db.SaveChangesAsync(cancellationToken);
}
