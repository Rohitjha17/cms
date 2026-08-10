using Cms.Domain.Common;

namespace Cms.Domain.Entities;

/// <summary>Search metadata defaults for one website.</summary>
public class SeoSetting : BaseEntity, ITenantEntity, ISiteEntity
{
    public Guid TenantId { get; set; }
    public Guid SiteId { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
    public string? OgImageUrl { get; set; }
    public string? CanonicalUrl { get; set; }

    /// <summary>
    /// When false the website asks search engines not to index it. Used while a school's
    /// site is still being prepared.
    /// </summary>
    public bool AllowIndexing { get; set; } = true;
}
