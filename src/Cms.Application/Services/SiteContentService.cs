using Cms.Application.DTOs.Content;
using Cms.Application.Interfaces;
using Cms.Domain.Entities;
using Cms.Domain.Enums;
using Cms.Shared.Exceptions;
using FluentValidation;
using Ganss.Xss;

namespace Cms.Application.Services;

public sealed class SiteContentService : ISiteContentService
{
    private static readonly HtmlSanitizer Sanitizer = CreateSanitizer();
    private readonly ISiteContentRepository _repository;
    private readonly ITenantContext _tenantContext;
    private readonly ISiteContext _siteContext;
    private readonly ICurrentUserContext _currentUser;
    private readonly IWebsiteService _websiteService;
    private readonly IValidator<SavePageDto> _pageValidator;
    private readonly IValidator<SaveMenuDto> _menuValidator;
    private readonly IValidator<SeoSettingDto> _seoValidator;
    private readonly IValidator<SaveContentEntryDto> _entryValidator;

    public SiteContentService(
        ISiteContentRepository repository,
        ITenantContext tenantContext,
        ISiteContext siteContext,
        ICurrentUserContext currentUser,
        IWebsiteService websiteService,
        IValidator<SavePageDto> pageValidator,
        IValidator<SaveMenuDto> menuValidator,
        IValidator<SeoSettingDto> seoValidator,
        IValidator<SaveContentEntryDto> entryValidator)
    {
        _repository = repository;
        _tenantContext = tenantContext;
        _siteContext = siteContext;
        _currentUser = currentUser;
        _websiteService = websiteService;
        _pageValidator = pageValidator;
        _menuValidator = menuValidator;
        _seoValidator = seoValidator;
        _entryValidator = entryValidator;
    }

    public async Task<IReadOnlyList<PageDto>> GetPagesAsync(bool includeInactive, CancellationToken cancellationToken)
    {
        var (tenantId, siteId) = RequireContext();
        var pages = await _repository.GetPagesAsync(tenantId, siteId, !includeInactive, cancellationToken);
        return pages.Select(page => ToDto(page)).ToList();
    }

    public async Task<PageDto> GetPageAsync(Guid id, CancellationToken cancellationToken)
    {
        var (tenantId, siteId) = RequireContext();
        var page = await _repository.GetPageAsync(tenantId, siteId, id, cancellationToken)
            ?? throw new NotFoundException("Page was not found.");
        return ToDto(page);
    }

    public async Task<PageDto> GetPageBySlugAsync(string slug, bool includeInactive, CancellationToken cancellationToken)
    {
        var (tenantId, siteId) = RequireContext();
        var normalized = NormalizeKey(slug);
        var page = await _repository.GetPageBySlugAsync(tenantId, siteId, normalized, cancellationToken)
            ?? throw new NotFoundException("Page was not found.");
        if (!includeInactive && !page.IsActive)
        {
            throw new NotFoundException("Page was not found.");
        }
        return ToDto(page);
    }

    public async Task<PageDto> SavePageAsync(Guid? id, SavePageDto dto, CancellationToken cancellationToken)
    {
        await _pageValidator.ValidateAndThrowAsync(dto, cancellationToken);
        var (tenantId, siteId) = RequireContext();
        var slug = NormalizeKey(dto.Slug);
        var duplicate = await _repository.GetPageBySlugAsync(tenantId, siteId, slug, cancellationToken);
        if (duplicate is not null && duplicate.Id != id)
        {
            throw new ValidationAppException($"A page with slug '{slug}' already exists.");
        }

        Page page;
        if (id.HasValue)
        {
            page = await _repository.GetPageAsync(tenantId, siteId, id.Value, cancellationToken)
                ?? throw new NotFoundException("Page was not found.");
            page.UpdatedDate = DateTime.UtcNow;
            page.UpdatedBy = Actor;
        }
        else
        {
            page = new Page
            {
                TenantId = tenantId,
                SiteId = siteId,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = Actor
            };
            await _repository.AddPageAsync(page, cancellationToken);
        }

        page.PageType = Enum.IsDefined(dto.PageType) ? dto.PageType : PageType.Custom;
        page.TemplateKey = dto.TemplateKey?.Trim();
        page.Title = dto.Title.Trim();
        page.Slug = slug;
        page.Excerpt = dto.Excerpt?.Trim();
        page.Content = Sanitize(dto.Content);
        page.JsonData = dto.JsonData;
        page.FeaturedImageUrl = dto.FeaturedImageUrl?.Trim();
        page.MetaTitle = dto.MetaTitle?.Trim();
        page.MetaDescription = dto.MetaDescription?.Trim();
        page.ShowInMenu = dto.ShowInMenu;
        page.MenuOrder = dto.MenuOrder;
        page.IsActive = dto.IsActive;
        await _repository.SaveChangesAsync(cancellationToken);
        await _websiteService.SyncHeaderMenuAsync(cancellationToken);
        return ToDto(page);
    }

    public async Task DeletePageAsync(Guid id, CancellationToken cancellationToken)
    {
        var (tenantId, siteId) = RequireContext();
        var page = await _repository.GetPageAsync(tenantId, siteId, id, cancellationToken)
            ?? throw new NotFoundException("Page was not found.");

        // Read before the row goes: afterwards there is nothing left to say which link was its.
        var slug = page.Slug;

        _repository.DeletePage(page);
        await _repository.SaveChangesAsync(cancellationToken);
        await _websiteService.SyncHeaderMenuAsync(cancellationToken, slug);
    }

    public async Task<IReadOnlyList<MenuDto>> GetMenusAsync(bool includeInactive, CancellationToken cancellationToken)
    {
        var (tenantId, siteId) = RequireContext();
        var menus = await _repository.GetMenusAsync(tenantId, siteId, !includeInactive, cancellationToken);
        return menus.Select(menu => ToDto(menu)).ToList();
    }

    public async Task<MenuDto> GetMenuAsync(Guid id, CancellationToken cancellationToken)
    {
        var (tenantId, siteId) = RequireContext();
        var menu = await _repository.GetMenuAsync(tenantId, siteId, id, cancellationToken)
            ?? throw new NotFoundException("Menu was not found.");
        return ToDto(menu);
    }

    public async Task<MenuDto> GetMenuByLocationAsync(string location, CancellationToken cancellationToken)
    {
        var (tenantId, siteId) = RequireContext();
        var menu = await _repository.GetMenuByLocationAsync(
            tenantId, siteId, NormalizeType(location), cancellationToken)
            ?? throw new NotFoundException("Menu was not found.");
        if (!menu.IsActive)
        {
            throw new NotFoundException("Menu was not found.");
        }
        return ToDto(menu, activeItemsOnly: true);
    }

    public async Task<MenuDto> SaveMenuAsync(Guid? id, SaveMenuDto dto, CancellationToken cancellationToken)
    {
        await _menuValidator.ValidateAndThrowAsync(dto, cancellationToken);
        var (tenantId, siteId) = RequireContext();
        var location = NormalizeType(dto.Location);
        var duplicate = await _repository.GetMenuByLocationAsync(tenantId, siteId, location, cancellationToken);
        if (duplicate is not null && duplicate.Id != id)
        {
            throw new ValidationAppException($"A menu already exists at location '{location}'.");
        }

        Menu menu;
        if (id.HasValue)
        {
            menu = await _repository.GetMenuAsync(tenantId, siteId, id.Value, cancellationToken)
                ?? throw new NotFoundException("Menu was not found.");
            _repository.RemoveMenuItems(menu.Items);
            menu.Items.Clear();
            menu.UpdatedDate = DateTime.UtcNow;
            menu.UpdatedBy = Actor;
        }
        else
        {
            menu = new Menu
            {
                TenantId = tenantId,
                SiteId = siteId,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = Actor
            };
            await _repository.AddMenuAsync(menu, cancellationToken);
        }

        menu.Name = dto.Name.Trim();
        menu.Location = location;
        menu.IsActive = dto.IsActive;
        foreach (var item in dto.Items.OrderBy(x => x.DisplayOrder))
        {
            menu.Items.Add(new MenuItem
            {
                TenantId = tenantId,
                SiteId = siteId,
                Label = item.Label.Trim(),
                Url = item.Url.Trim(),
                Target = item.Target,
                DisplayOrder = item.DisplayOrder,
                IsActive = item.IsActive,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = Actor
            });
        }

        await _repository.SaveChangesAsync(cancellationToken);
        return ToDto(menu);
    }

    public async Task DeleteMenuAsync(Guid id, CancellationToken cancellationToken)
    {
        var (tenantId, siteId) = RequireContext();
        var menu = await _repository.GetMenuAsync(tenantId, siteId, id, cancellationToken)
            ?? throw new NotFoundException("Menu was not found.");
        _repository.DeleteMenu(menu);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task<SeoSettingDto> GetSeoAsync(CancellationToken cancellationToken)
    {
        var (tenantId, siteId) = RequireContext();
        var setting = await _repository.GetSeoAsync(tenantId, siteId, cancellationToken);
        return setting is null ? new SeoSettingDto() : ToDto(setting);
    }

    public async Task<SeoSettingDto> SaveSeoAsync(SeoSettingDto dto, CancellationToken cancellationToken)
    {
        await _seoValidator.ValidateAndThrowAsync(dto, cancellationToken);
        var (tenantId, siteId) = RequireContext();
        var setting = await _repository.GetSeoAsync(tenantId, siteId, cancellationToken);
        if (setting is null)
        {
            setting = new SeoSetting
            {
                TenantId = tenantId,
                SiteId = siteId,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = Actor
            };
            await _repository.AddSeoAsync(setting, cancellationToken);
        }
        else
        {
            setting.UpdatedDate = DateTime.UtcNow;
            setting.UpdatedBy = Actor;
        }

        setting.MetaTitle = dto.MetaTitle?.Trim();
        setting.MetaDescription = dto.MetaDescription?.Trim();
        setting.MetaKeywords = dto.MetaKeywords?.Trim();
        setting.AllowIndexing = dto.AllowIndexing;
        setting.OgImageUrl = dto.OgImageUrl?.Trim();
        setting.CanonicalUrl = dto.CanonicalUrl?.Trim();
        await _repository.SaveChangesAsync(cancellationToken);
        return ToDto(setting);
    }

    public async Task<IReadOnlyList<ContentEntryDto>> GetEntriesAsync(
        string type, bool includeInactive, CancellationToken cancellationToken)
    {
        var (tenantId, siteId) = RequireContext();
        var entries = await _repository.GetEntriesAsync(
            tenantId, siteId, NormalizeType(type), !includeInactive, cancellationToken);
        return entries.Select(entry => ToDto(entry)).ToList();
    }

    public async Task<ContentEntryDto> GetEntryAsync(Guid id, CancellationToken cancellationToken)
    {
        var (tenantId, siteId) = RequireContext();
        var entry = await _repository.GetEntryAsync(tenantId, siteId, id, cancellationToken)
            ?? throw new NotFoundException("Content entry was not found.");
        return ToDto(entry);
    }

    public async Task<ContentEntryDto> GetEntryByKeyAsync(
        string type, string key, bool includeInactive, CancellationToken cancellationToken)
    {
        var (tenantId, siteId) = RequireContext();
        var entry = await _repository.GetEntryByKeyAsync(
            tenantId, siteId, NormalizeType(type), NormalizeKey(key), cancellationToken)
            ?? throw new NotFoundException("Content entry was not found.");
        if (!includeInactive && !entry.IsActive)
        {
            throw new NotFoundException("Content entry was not found.");
        }
        return ToDto(entry);
    }

    public async Task<ContentEntryDto> SaveEntryAsync(
        Guid? id, SaveContentEntryDto dto, CancellationToken cancellationToken)
    {
        dto.ContentType = NormalizeType(dto.ContentType);
        dto.Key = NormalizeKey(dto.Key);
        await _entryValidator.ValidateAndThrowAsync(dto, cancellationToken);
        var (tenantId, siteId) = RequireContext();
        var duplicate = await _repository.GetEntryByKeyAsync(
            tenantId, siteId, dto.ContentType, dto.Key, cancellationToken);
        if (duplicate is not null && duplicate.Id != id)
        {
            throw new ValidationAppException(
                $"A {dto.ContentType} entry with key '{dto.Key}' already exists.");
        }

        ContentEntry entry;
        if (id.HasValue)
        {
            entry = await _repository.GetEntryAsync(tenantId, siteId, id.Value, cancellationToken)
                ?? throw new NotFoundException("Content entry was not found.");
            entry.UpdatedDate = DateTime.UtcNow;
            entry.UpdatedBy = Actor;
        }
        else
        {
            entry = new ContentEntry
            {
                TenantId = tenantId,
                SiteId = siteId,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = Actor
            };
            await _repository.AddEntryAsync(entry, cancellationToken);
        }

        entry.ContentType = dto.ContentType;
        entry.Key = dto.Key;
        entry.Title = dto.Title.Trim();
        entry.Summary = dto.Summary?.Trim();
        entry.Body = Sanitize(dto.Body);
        entry.ImageUrl = dto.ImageUrl?.Trim();
        entry.JsonData = dto.JsonData;
        entry.DisplayOrder = dto.DisplayOrder;
        entry.IsActive = dto.IsActive;
        entry.PublishDate = dto.PublishDate;
        await _repository.SaveChangesAsync(cancellationToken);
        return ToDto(entry);
    }

    public async Task DeleteEntryAsync(Guid id, CancellationToken cancellationToken)
    {
        var (tenantId, siteId) = RequireContext();
        var entry = await _repository.GetEntryAsync(tenantId, siteId, id, cancellationToken)
            ?? throw new NotFoundException("Content entry was not found.");
        _repository.DeleteEntry(entry);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    private string Actor => _currentUser.UserId ?? "system";

    private (Guid TenantId, Guid SiteId) RequireContext()
    {
        if (!_tenantContext.IsResolved || !_siteContext.IsResolved
            || !_tenantContext.TenantId.HasValue || !_siteContext.SiteId.HasValue)
        {
            throw new TenantNotResolvedException();
        }
        return (_tenantContext.TenantId.Value, _siteContext.SiteId.Value);
    }

    private static string NormalizeKey(string value) => value.Trim().ToLowerInvariant().Replace(' ', '-');
    private static string NormalizeType(string value) => value.Trim().ToLowerInvariant().Replace(' ', '_');
    private static string? Sanitize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? value : Sanitizer.Sanitize(value);

    private static HtmlSanitizer CreateSanitizer()
    {
        var sanitizer = new HtmlSanitizer();
        sanitizer.AllowedTags.UnionWith(["h1", "h2", "h3", "h4", "p", "br", "strong", "em", "u", "ul", "ol", "li", "blockquote", "a", "img"]);
        sanitizer.AllowedAttributes.UnionWith(["href", "src", "alt", "title"]);
        sanitizer.AllowedSchemes.UnionWith(["http", "https", "mailto", "tel"]);
        return sanitizer;
    }

    private static PageDto ToDto(Page x) => new()
    {
        Id = x.Id, PageType = x.PageType, TemplateKey = x.TemplateKey,
        Title = x.Title, Slug = x.Slug, Excerpt = x.Excerpt, Content = x.Content,
        JsonData = x.JsonData, FeaturedImageUrl = x.FeaturedImageUrl, MetaTitle = x.MetaTitle,
        MetaDescription = x.MetaDescription, ShowInMenu = x.ShowInMenu, MenuOrder = x.MenuOrder,
        IsActive = x.IsActive, CreatedDate = x.CreatedDate, UpdatedDate = x.UpdatedDate
    };

    private static MenuDto ToDto(Menu x, bool activeItemsOnly = false) => new()
    {
        Id = x.Id, Name = x.Name, Location = x.Location, IsActive = x.IsActive,
        Items = x.Items.Where(i => !activeItemsOnly || i.IsActive)
            .OrderBy(i => i.DisplayOrder)
            .Select(i => new MenuItemDto
            {
                Id = i.Id, Label = i.Label, Url = i.Url, Target = i.Target,
                DisplayOrder = i.DisplayOrder, IsActive = i.IsActive
            }).ToList()
    };

    private static SeoSettingDto ToDto(SeoSetting x) => new()
    {
        MetaTitle = x.MetaTitle, MetaDescription = x.MetaDescription,
        MetaKeywords = x.MetaKeywords, OgImageUrl = x.OgImageUrl, CanonicalUrl = x.CanonicalUrl,
        AllowIndexing = x.AllowIndexing
    };

    private static ContentEntryDto ToDto(ContentEntry x) => new()
    {
        Id = x.Id, ContentType = x.ContentType, Key = x.Key, Title = x.Title,
        Summary = x.Summary, Body = x.Body, ImageUrl = x.ImageUrl, JsonData = x.JsonData,
        DisplayOrder = x.DisplayOrder, IsActive = x.IsActive, PublishDate = x.PublishDate
    };
}
