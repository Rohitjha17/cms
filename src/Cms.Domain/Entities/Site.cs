using Cms.Domain.Common;
using Cms.Domain.Enums;

namespace Cms.Domain.Entities;

/// <summary>
/// A publishable school/college website. Domains and pages belong to a Site.
/// </summary>
public class Site : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SiteKey { get; set; } = string.Empty;
    public WebsiteType WebsiteType { get; set; }
    public HomeVariant HomeVariant { get; set; } = HomeVariant.Classic;
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;

    // Branding
    public string? LogoUrl { get; set; }
    public string? FaviconUrl { get; set; }
    public string? Tagline { get; set; }
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }
    public string? HeaderImageUrl { get; set; }
    public string? FooterText { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? MapEmbedUrl { get; set; }
    public string? SocialLinksJson { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public ICollection<HomePageSection> HomePageSections { get; set; } = new List<HomePageSection>();
    public ICollection<Page> Pages { get; set; } = new List<Page>();
    public ICollection<TenantDomain> Domains { get; set; } = new List<TenantDomain>();
}
