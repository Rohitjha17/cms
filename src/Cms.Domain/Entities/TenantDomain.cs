using Cms.Domain.Common;

namespace Cms.Domain.Entities;

/// <summary>
/// Host binding for a website. Prefer SiteId so one domain maps to one school site.
/// </summary>
public class TenantDomain : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public Guid? SiteId { get; set; }
    public string DomainName { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public bool IsActive { get; set; } = true;

    public Tenant Tenant { get; set; } = null!;
    public Site? Site { get; set; }
}
