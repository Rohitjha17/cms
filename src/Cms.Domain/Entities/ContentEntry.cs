using Cms.Domain.Common;

namespace Cms.Domain.Entities;

/// <summary>
/// Flexible tenant/site content for news, events, people, departments, settings and themes.
/// </summary>
public sealed class ContentEntry : BaseEntity, ITenantEntity, ISiteEntity
{
    public Guid TenantId { get; set; }
    public Guid SiteId { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? Body { get; set; }
    public string? ImageUrl { get; set; }
    public string? JsonData { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? PublishDate { get; set; }
}
