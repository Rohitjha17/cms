using Cms.Domain.Common;

namespace Cms.Domain.Entities;

public class Tenant : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<TenantDomain> Domains { get; set; } = new List<TenantDomain>();
    public ICollection<Site> Sites { get; set; } = new List<Site>();
}
