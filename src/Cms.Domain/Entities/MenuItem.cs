using Cms.Domain.Common;

namespace Cms.Domain.Entities;

public sealed class MenuItem : BaseEntity, ITenantEntity, ISiteEntity
{
    public Guid TenantId { get; set; }
    public Guid SiteId { get; set; }
    public Guid MenuId { get; set; }
    public Guid? ParentId { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Url { get; set; } = "/";
    public string? Target { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public Menu Menu { get; set; } = null!;
    public MenuItem? Parent { get; set; }
    public ICollection<MenuItem> Children { get; set; } = new List<MenuItem>();
}
