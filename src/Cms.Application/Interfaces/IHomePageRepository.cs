using Cms.Domain.Entities;

namespace Cms.Application.Interfaces;

public interface IHomePageRepository
{
    Task<IReadOnlyList<HomePageSection>> GetAllAsync(Guid tenantId, Guid siteId, bool activeOnly, CancellationToken cancellationToken = default);
    Task<HomePageSection?> GetByKeyAsync(Guid tenantId, Guid siteId, string sectionKey, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid tenantId, Guid siteId, string sectionKey, CancellationToken cancellationToken = default);
    Task AddAsync(HomePageSection section, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<HomePageSection> sections, CancellationToken cancellationToken = default);
    Task UpdateAsync(HomePageSection section, CancellationToken cancellationToken = default);
    Task DeleteAsync(HomePageSection section, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
