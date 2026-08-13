using Cms.Domain.Entities;

namespace Cms.Application.Interfaces;

public interface IWebsiteRepository
{
    Task<IReadOnlyList<PageTemplate>> GetPageTemplatesAsync(CancellationToken cancellationToken);
    Task<PageTemplate?> GetPageTemplateAsync(string templateKey, CancellationToken cancellationToken);
    Task<PageTemplate?> GetPageTemplateByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddPageTemplateAsync(PageTemplate template, CancellationToken cancellationToken);
    Task<IReadOnlyList<Site>> GetSitesAsync(Guid tenantId, CancellationToken cancellationToken);
    Task<Site?> GetSiteAsync(Guid tenantId, Guid siteId, CancellationToken cancellationToken);
    Task<Site?> GetSiteByKeyAsync(Guid tenantId, string siteKey, CancellationToken cancellationToken);
    Task<TenantDomain?> GetDomainAsync(string domainName, CancellationToken cancellationToken);
    Task<IReadOnlyList<TenantDomain>> GetDomainsAsync(Guid tenantId, CancellationToken cancellationToken);
    Task<TenantDomain?> GetDomainByIdAsync(Guid tenantId, Guid id, CancellationToken cancellationToken);
    void RemoveDomain(TenantDomain domain);
    Task AddSiteAsync(Site site, CancellationToken cancellationToken);
    Task AddDomainAsync(TenantDomain domain, CancellationToken cancellationToken);
    Task AddPageAsync(Page page, CancellationToken cancellationToken);
    Task AddMenuAsync(Menu menu, CancellationToken cancellationToken);
    void RemoveMenuItems(IEnumerable<MenuItem> items);
    Task AddSeoAsync(SeoSetting seo, CancellationToken cancellationToken);
    Task AddContactAsync(ContactSubmission submission, CancellationToken cancellationToken);
    Task<IReadOnlyList<Page>> GetPagesAsync(Guid tenantId, Guid siteId, bool activeOnly, CancellationToken cancellationToken);
    Task<Page?> GetPageBySlugAsync(Guid tenantId, Guid siteId, string slug, CancellationToken cancellationToken);
    Task<Menu?> GetMenuByLocationAsync(Guid tenantId, Guid siteId, string location, CancellationToken cancellationToken);
    Task<SeoSetting?> GetSeoAsync(Guid tenantId, Guid siteId, CancellationToken cancellationToken);
    Task<IReadOnlyList<HomePageSection>> GetHomeSectionsAsync(Guid tenantId, Guid siteId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ContactSubmission>> GetContactsAsync(Guid tenantId, Guid siteId, CancellationToken cancellationToken);
    Task<int> CountUnreadContactsAsync(Guid tenantId, Guid siteId, CancellationToken cancellationToken);
    Task<ContactSubmission?> GetContactAsync(Guid tenantId, Guid siteId, Guid id, CancellationToken cancellationToken);
    Task<int> CountPagesAsync(Guid tenantId, Guid siteId, CancellationToken cancellationToken);
    Task EnsureHomeSectionsAsync(Guid tenantId, Guid siteId, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
