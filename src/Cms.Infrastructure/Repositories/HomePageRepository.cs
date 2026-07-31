using Cms.Application.Interfaces;
using Cms.Domain.Entities;
using Cms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Cms.Infrastructure.Repositories;

public class HomePageRepository : IHomePageRepository
{
    private readonly ApplicationDbContext _db;

    public HomePageRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<HomePageSection>> GetAllAsync(Guid tenantId, Guid siteId, bool activeOnly, CancellationToken cancellationToken = default)
    {
        var query = _db.HomePageSections
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.SiteId == siteId);

        if (activeOnly)
        {
            query = query.Where(x => x.IsActive);
        }

        return await query.OrderBy(x => x.DisplayOrder).ToListAsync(cancellationToken);
    }

    public Task<HomePageSection?> GetByKeyAsync(Guid tenantId, Guid siteId, string sectionKey, CancellationToken cancellationToken = default)
    {
        return _db.HomePageSections
            .FirstOrDefaultAsync(x =>
                x.TenantId == tenantId &&
                x.SiteId == siteId &&
                x.SectionKey == sectionKey, cancellationToken);
    }

    public Task<bool> ExistsAsync(Guid tenantId, Guid siteId, string sectionKey, CancellationToken cancellationToken = default)
    {
        return _db.HomePageSections.AnyAsync(x =>
            x.TenantId == tenantId &&
            x.SiteId == siteId &&
            x.SectionKey == sectionKey, cancellationToken);
    }

    public async Task AddAsync(HomePageSection section, CancellationToken cancellationToken = default)
    {
        await _db.HomePageSections.AddAsync(section, cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<HomePageSection> sections, CancellationToken cancellationToken = default)
    {
        await _db.HomePageSections.AddRangeAsync(sections, cancellationToken);
    }

    public Task UpdateAsync(HomePageSection section, CancellationToken cancellationToken = default)
    {
        _db.HomePageSections.Update(section);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(HomePageSection section, CancellationToken cancellationToken = default)
    {
        _db.HomePageSections.Remove(section);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _db.SaveChangesAsync(cancellationToken);
}
