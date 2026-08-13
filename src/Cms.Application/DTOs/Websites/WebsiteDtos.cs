using Cms.Domain.Enums;

namespace Cms.Application.DTOs.Websites;

public sealed class PageTemplateDto
{
    public Guid Id { get; set; }
    public string TemplateKey { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public PageType PageType { get; set; }
    public string DefaultSlug { get; set; } = string.Empty;
    public string? DefaultTitle { get; set; }
    public string? DefaultContent { get; set; }
    public string? DefaultJsonData { get; set; }
    public bool IsStarter { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }
}

public sealed class SavePageTemplateDto
{
    public string TemplateKey { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public PageType PageType { get; set; } = PageType.Custom;
    public string DefaultSlug { get; set; } = string.Empty;
    public string? DefaultTitle { get; set; }
    public string? DefaultContent { get; set; }
    public string? DefaultJsonData { get; set; }
    public bool IsStarter { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }
}

public sealed class AssignTemplatesDto
{
    public List<string> TemplateKeys { get; set; } = [];
}

public sealed class WebsiteSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SiteKey { get; set; } = string.Empty;
    public WebsiteType WebsiteType { get; set; }
    public HomeVariant HomeVariant { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
    public string? LogoUrl { get; set; }
    public string? Tagline { get; set; }
    public string? PrimaryColor { get; set; }
    public IReadOnlyList<string> Domains { get; set; } = [];
    public int PageCount { get; set; }
}

public sealed class SiteBrandingDto
{
    public string Name { get; set; } = string.Empty;
    public string SiteKey { get; set; } = string.Empty;
    public WebsiteType WebsiteType { get; set; }
    public HomeVariant HomeVariant { get; set; }
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
}

public sealed class ProvisionWebsiteDto
{
    public string Name { get; set; } = string.Empty;
    public string SiteKey { get; set; } = string.Empty;
    public WebsiteType WebsiteType { get; set; } = WebsiteType.School;
    public HomeVariant HomeVariant { get; set; } = HomeVariant.Classic;
    public bool IsDefault { get; set; }
    public string? DomainName { get; set; }
    public string? LogoUrl { get; set; }
    public string? HeaderImageUrl { get; set; }
    public string? Tagline { get; set; }
    public string? PrimaryColor { get; set; } = "#0f2d5c";
    public string? SecondaryColor { get; set; } = "#c9a227";
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public List<string> TemplateKeys { get; set; } = [];
}

public sealed class ContactSubmissionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Subject { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedDate { get; set; }
}

public sealed class SubmitContactDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Subject { get; set; }
    public string Message { get; set; } = string.Empty;
}

public sealed class PublicWebsiteDto
{
    /// <summary>
    /// URL prefix this website is served under: empty on a dedicated domain,
    /// <c>/{siteKey}</c> when one domain hosts several websites.
    /// </summary>
    public string BasePath { get; set; } = string.Empty;

    public SiteBrandingDto Branding { get; set; } = new();
    public SeoPublicDto Seo { get; set; } = new();
    public IReadOnlyList<PublicNavItemDto> Navigation { get; set; } = [];
    public IReadOnlyList<HomeSectionPublicDto> HomeSections { get; set; } = [];
}

public sealed class SeoPublicDto
{
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
    public string? OgImageUrl { get; set; }
    public string? CanonicalUrl { get; set; }
    public bool AllowIndexing { get; set; } = true;
}

public sealed class PublicNavItemDto
{
    public string Label { get; set; } = string.Empty;
    public string Url { get; set; } = "/";
    public string? Target { get; set; }
}

public sealed class HomeSectionPublicDto
{
    public string SectionKey { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? SubTitle { get; set; }
    public string? Description { get; set; }
    public string? ButtonText { get; set; }
    public string? ButtonLink { get; set; }
    public string? ImageUrl { get; set; }
    public string? JsonData { get; set; }
    public int DisplayOrder { get; set; }
}

public sealed class PublicPageDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public PageType PageType { get; set; }
    public string? TemplateKey { get; set; }
    public string? Excerpt { get; set; }
    public string? Content { get; set; }
    public string? JsonData { get; set; }
    public string? FeaturedImageUrl { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
}

/// <summary>
/// A host that points at this tenant. Resolution is host-first: an incoming request is
/// matched against these rows, which is what makes one deployment serve unlimited schools.
/// </summary>
public sealed class SiteDomainDto
{
    public Guid Id { get; set; }
    public string DomainName { get; set; } = string.Empty;
    public Guid? SiteId { get; set; }
    public string? SiteName { get; set; }
    public string? SiteKey { get; set; }
    public bool IsPrimary { get; set; }
    public bool IsActive { get; set; }

    /// <summary>True when the host serves every website of the tenant under a path prefix.</summary>
    public bool IsShared => SiteId is null;

    public string ExampleUrl => IsShared
        ? $"https://{DomainName}/{SiteKey ?? "school"}/about"
        : $"https://{DomainName}/about";
}

public sealed class SaveSiteDomainDto
{
    public string DomainName { get; set; } = string.Empty;
    public Guid? SiteId { get; set; }
    public bool IsPrimary { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>A complete website template as offered in the gallery.</summary>
public sealed class SiteTemplateSummaryDto
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string BestFor { get; set; } = string.Empty;
    public WebsiteType WebsiteType { get; set; }
    public HomeVariant HomeVariant { get; set; }
    public string PrimaryColor { get; set; } = string.Empty;
    public string SecondaryColor { get; set; } = string.Empty;
    public string SampleTagline { get; set; } = string.Empty;
    public IReadOnlyList<string> Highlights { get; set; } = [];
    public int PageCount { get; set; }
}

public sealed class ProvisionFromTemplateDto
{
    public string TemplateKey { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string SiteKey { get; set; } = string.Empty;
    public string? DomainName { get; set; }

    /// <summary>Keep the template's sample staff, notices, events and departments.</summary>
    public bool IncludeSampleContent { get; set; } = true;
}
