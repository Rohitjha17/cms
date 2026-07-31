using Cms.Domain.Common;

namespace Cms.Domain.Entities;

public class Page : BaseEntity, ITenantEntity, ISiteEntity
{
    public Guid TenantId { get; set; }
    public Guid SiteId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Excerpt { get; set; }
    public string? Content { get; set; }
    public string? FeaturedImageUrl { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public bool IsActive { get; set; } = true;
}
