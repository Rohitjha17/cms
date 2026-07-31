using Cms.Application.DTOs.Tenancy;
using Cms.Domain.Entities;

namespace Cms.Application.Interfaces;

public interface ITenantManagementService
{
    Task<IReadOnlyList<TenantManagementDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<TenantManagementDto> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<TenantManagementDto> SaveAsync(Guid? id, SaveTenantDto dto, CancellationToken cancellationToken);
}

public interface ITenantManagementRepository
{
    Task<IReadOnlyList<Tenant>> GetAllAsync(CancellationToken cancellationToken);
    Task<Tenant?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<Tenant?> GetByCodeAsync(string code, CancellationToken cancellationToken);
    Task<TenantDomain?> GetDomainAsync(string domain, CancellationToken cancellationToken);
    Task AddAsync(Tenant tenant, CancellationToken cancellationToken);
    void RemoveDomains(IEnumerable<TenantDomain> domains);
    void RemoveSites(IEnumerable<Site> sites);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
