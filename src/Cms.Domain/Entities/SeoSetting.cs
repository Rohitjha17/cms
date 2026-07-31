using Cms.Domain.Common;

namespace Cms.Domain.Entities;

/// <summary>Schema placeholder for future SEO module.</summary>
public class SeoSetting : BaseEntity, ITenantEntity, ISiteEntity
{
    public Guid TenantId { get; set; }
    public Guid SiteId { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
    public string? OgImageUrl { get; set; }
    public string? CanonicalUrl { get; set; }
}
