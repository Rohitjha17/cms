using Cms.Domain.Entities;

namespace Cms.Application.Interfaces;

public interface ISiteContentRepository
{
    Task<IReadOnlyList<Page>> GetPagesAsync(Guid tenantId, Guid siteId, bool activeOnly, CancellationToken cancellationToken);
    Task<Page?> GetPageAsync(Guid tenantId, Guid siteId, Guid id, CancellationToken cancellationToken);
    Task<Page?> GetPageBySlugAsync(Guid tenantId, Guid siteId, string slug, CancellationToken cancellationToken);
    Task AddPageAsync(Page page, CancellationToken cancellationToken);
    void DeletePage(Page page);

    Task<IReadOnlyList<Menu>> GetMenusAsync(Guid tenantId, Guid siteId, bool activeOnly, CancellationToken cancellationToken);
    Task<Menu?> GetMenuAsync(Guid tenantId, Guid siteId, Guid id, CancellationToken cancellationToken);
    Task<Menu?> GetMenuByLocationAsync(Guid tenantId, Guid siteId, string location, CancellationToken cancellationToken);
    Task AddMenuAsync(Menu menu, CancellationToken cancellationToken);
    void RemoveMenuItems(IEnumerable<MenuItem> items);
    void DeleteMenu(Menu menu);

    Task<SeoSetting?> GetSeoAsync(Guid tenantId, Guid siteId, CancellationToken cancellationToken);
    Task AddSeoAsync(SeoSetting setting, CancellationToken cancellationToken);

    Task<IReadOnlyList<ContentEntry>> GetEntriesAsync(Guid tenantId, Guid siteId, string type, bool activeOnly, CancellationToken cancellationToken);
    Task<ContentEntry?> GetEntryAsync(Guid tenantId, Guid siteId, Guid id, CancellationToken cancellationToken);
    Task<ContentEntry?> GetEntryByKeyAsync(Guid tenantId, Guid siteId, string type, string key, CancellationToken cancellationToken);
    Task AddEntryAsync(ContentEntry entry, CancellationToken cancellationToken);
    void DeleteEntry(ContentEntry entry);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
