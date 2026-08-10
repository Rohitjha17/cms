using Cms.Domain.Common;

namespace Cms.Domain.Entities;

public sealed class ActivityLog : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public Guid? SiteId { get; set; }
    public string ActorId { get; set; } = "system";
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string? ChangedProperties { get; set; }
}
