using Cms.Application.Interfaces;
using Cms.Domain.Entities;
using Cms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Cms.Infrastructure.Repositories;

public sealed class SiteContentRepository : ISiteContentRepository
{
    private readonly ApplicationDbContext _db;

    public SiteContentRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<Page>> GetPagesAsync(Guid tenantId, Guid siteId, bool activeOnly, CancellationToken cancellationToken) =>
        await _db.Pages.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.SiteId == siteId && (!activeOnly || x.IsActive))
            .OrderBy(x => x.Title)
            .ToListAsync(cancellationToken);

    public Task<Page?> GetPageAsync(Guid tenantId, Guid siteId, Guid id, CancellationToken cancellationToken) =>
        _db.Pages.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && x.SiteId == siteId, cancellationToken);

    public Task<Page?> GetPageBySlugAsync(Guid tenantId, Guid siteId, string slug, CancellationToken cancellationToken) =>
        _db.Pages.AsNoTracking().FirstOrDefaultAsync(
            x => x.Slug == slug && x.TenantId == tenantId && x.SiteId == siteId,
            cancellationToken);

    public Task AddPageAsync(Page page, CancellationToken cancellationToken) =>
        _db.Pages.AddAsync(page, cancellationToken).AsTask();

    public void DeletePage(Page page) => _db.Pages.Remove(page);

    public async Task<IReadOnlyList<Menu>> GetMenusAsync(Guid tenantId, Guid siteId, bool activeOnly, CancellationToken cancellationToken) =>
        await _db.Menus.AsNoTracking().Include(x => x.Items)
            .Where(x => x.TenantId == tenantId && x.SiteId == siteId && (!activeOnly || x.IsActive))
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

    public Task<Menu?> GetMenuAsync(Guid tenantId, Guid siteId, Guid id, CancellationToken cancellationToken) =>
        _db.Menus.Include(x => x.Items).FirstOrDefaultAsync(
            x => x.Id == id && x.TenantId == tenantId && x.SiteId == siteId,
            cancellationToken);

    public Task<Menu?> GetMenuByLocationAsync(Guid tenantId, Guid siteId, string location, CancellationToken cancellationToken) =>
        _db.Menus.AsNoTracking().Include(x => x.Items).FirstOrDefaultAsync(
            x => x.Location == location && x.TenantId == tenantId && x.SiteId == siteId,
            cancellationToken);

    public Task AddMenuAsync(Menu menu, CancellationToken cancellationToken) =>
        _db.Menus.AddAsync(menu, cancellationToken).AsTask();

    public void RemoveMenuItems(IEnumerable<MenuItem> items) => _db.MenuItems.RemoveRange(items);
    public void DeleteMenu(Menu menu) => _db.Menus.Remove(menu);

    public Task<SeoSetting?> GetSeoAsync(Guid tenantId, Guid siteId, CancellationToken cancellationToken) =>
        _db.SeoSettings.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.SiteId == siteId,
            cancellationToken);

    public Task AddSeoAsync(SeoSetting setting, CancellationToken cancellationToken) =>
        _db.SeoSettings.AddAsync(setting, cancellationToken).AsTask();

    public async Task<IReadOnlyList<ContentEntry>> GetEntriesAsync(
        Guid tenantId, Guid siteId, string type, bool activeOnly, CancellationToken cancellationToken) =>
        await _db.ContentEntries.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.SiteId == siteId
                && x.ContentType == type && (!activeOnly || x.IsActive))
            .OrderBy(x => x.DisplayOrder).ThenByDescending(x => x.PublishDate)
            .ToListAsync(cancellationToken);

    public Task<ContentEntry?> GetEntryAsync(Guid tenantId, Guid siteId, Guid id, CancellationToken cancellationToken) =>
        _db.ContentEntries.FirstOrDefaultAsync(
            x => x.Id == id && x.TenantId == tenantId && x.SiteId == siteId,
            cancellationToken);

    public Task<ContentEntry?> GetEntryByKeyAsync(
        Guid tenantId, Guid siteId, string type, string key, CancellationToken cancellationToken) =>
        _db.ContentEntries.AsNoTracking().FirstOrDefaultAsync(
            x => x.ContentType == type && x.Key == key && x.TenantId == tenantId && x.SiteId == siteId,
            cancellationToken);

    public Task AddEntryAsync(ContentEntry entry, CancellationToken cancellationToken) =>
        _db.ContentEntries.AddAsync(entry, cancellationToken).AsTask();

    public void DeleteEntry(ContentEntry entry) => _db.ContentEntries.Remove(entry);

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        _db.SaveChangesAsync(cancellationToken);
}
