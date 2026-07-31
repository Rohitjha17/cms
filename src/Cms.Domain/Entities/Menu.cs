using Cms.Domain.Common;

namespace Cms.Domain.Entities;

public class Menu : BaseEntity, ITenantEntity, ISiteEntity
{
    public Guid TenantId { get; set; }
    public Guid SiteId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = "header";
    public bool IsActive { get; set; } = true;
    public ICollection<MenuItem> Items { get; set; } = new List<MenuItem>();
}
