using Cms.Domain.Common;
using Cms.Domain.Enums;

namespace Cms.Domain.Entities;

public class Site : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SiteKey { get; set; } = string.Empty;
    public WebsiteType WebsiteType { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;

    public Tenant Tenant { get; set; } = null!;
    public ICollection<HomePageSection> HomePageSections { get; set; } = new List<HomePageSection>();
}
