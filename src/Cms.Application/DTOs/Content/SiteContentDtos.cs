namespace Cms.Application.DTOs.Content;

public sealed class PageDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Excerpt { get; set; }
    public string? Content { get; set; }
    public string? FeaturedImageUrl { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
}

public sealed class SavePageDto
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Excerpt { get; set; }
    public string? Content { get; set; }
    public string? FeaturedImageUrl { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class MenuDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = "header";
    public bool IsActive { get; set; }
    public IReadOnlyList<MenuItemDto> Items { get; set; } = [];
}

public sealed class MenuItemDto
{
    public Guid? Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Url { get; set; } = "/";
    public string? Target { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class SaveMenuDto
{
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = "header";
    public bool IsActive { get; set; } = true;
    public List<MenuItemDto> Items { get; set; } = [];
}

public sealed class SeoSettingDto
{
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
    public string? OgImageUrl { get; set; }
    public string? CanonicalUrl { get; set; }
}

public sealed class ContentEntryDto
{
    public Guid Id { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? Body { get; set; }
    public string? ImageUrl { get; set; }
    public string? JsonData { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTime? PublishDate { get; set; }
}

public sealed class SaveContentEntryDto
{
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
