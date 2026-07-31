using Cms.Application.Interfaces;
using Cms.Domain.Entities;
using Cms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Cms.Infrastructure.Repositories;

public sealed class TenantManagementRepository : ITenantManagementRepository
{
    private readonly ApplicationDbContext _db;

    public TenantManagementRepository(ApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<Tenant>> GetAllAsync(CancellationToken cancellationToken) =>
        await _db.Tenants.IgnoreQueryFilters().AsNoTracking()
            .Include(x => x.Domains).Include(x => x.Sites)
            .OrderBy(x => x.Name).ToListAsync(cancellationToken);

    public Task<Tenant?> GetAsync(Guid id, CancellationToken cancellationToken) =>
        _db.Tenants.IgnoreQueryFilters().Include(x => x.Domains).Include(x => x.Sites)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<Tenant?> GetByCodeAsync(string code, CancellationToken cancellationToken) =>
        _db.Tenants.IgnoreQueryFilters().AsNoTracking()
            .FirstOrDefaultAsync(x => x.Code == code, cancellationToken);

    public Task<TenantDomain?> GetDomainAsync(string domain, CancellationToken cancellationToken) =>
        _db.TenantDomains.IgnoreQueryFilters().AsNoTracking()
            .FirstOrDefaultAsync(x => x.DomainName == domain, cancellationToken);

    public Task AddAsync(Tenant tenant, CancellationToken cancellationToken) =>
        _db.Tenants.AddAsync(tenant, cancellationToken).AsTask();

    public void RemoveDomains(IEnumerable<TenantDomain> domains) => _db.TenantDomains.RemoveRange(domains);
    public void RemoveSites(IEnumerable<Site> sites) => _db.Sites.RemoveRange(sites);
    public Task SaveChangesAsync(CancellationToken cancellationToken) => _db.SaveChangesAsync(cancellationToken);
}
