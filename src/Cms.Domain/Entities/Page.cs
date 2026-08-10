using Cms.Domain.Common;
using Cms.Domain.Enums;

namespace Cms.Domain.Entities;

/// <summary>
/// A site page created from the page gallery template or as a custom page.
/// Structured content for typed pages lives in JsonData.
/// </summary>
public class Page : BaseEntity, ITenantEntity, ISiteEntity
{
    public Guid TenantId { get; set; }
    public Guid SiteId { get; set; }
    public PageType PageType { get; set; } = PageType.Custom;
    public string? TemplateKey { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Excerpt { get; set; }
    public string? Content { get; set; }
    public string? JsonData { get; set; }
    public string? FeaturedImageUrl { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public bool ShowInMenu { get; set; } = true;
    public int MenuOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public Site Site { get; set; } = null!;
}
