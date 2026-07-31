using Cms.Domain.Common;

namespace Cms.Domain.Entities;

public class TenantDomain : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public string DomainName { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public bool IsActive { get; set; } = true;

    public Tenant Tenant { get; set; } = null!;
}
