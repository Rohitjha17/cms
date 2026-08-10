using Cms.Domain.Common;
using Cms.Domain.Enums;

namespace Cms.Domain.Entities;

/// <summary>
/// Reusable page definition in the CMS page gallery.
/// New school websites are provisioned by assigning templates from this catalog.
/// </summary>
public class PageTemplate : BaseEntity
{
    public string TemplateKey { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public PageType PageType { get; set; }
    public string DefaultSlug { get; set; } = string.Empty;
    public string? DefaultTitle { get; set; }
    public string? DefaultContent { get; set; }
    public string? DefaultJsonData { get; set; }
    public bool IsStarter { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }
}
