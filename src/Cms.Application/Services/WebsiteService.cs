using Cms.Application.DTOs.Websites;
using Cms.Application.Interfaces;
using Cms.Application.Templates;
using System.Text.Json.Nodes;
using Cms.Domain.Constants;
using Cms.Domain.Entities;
using Cms.Domain.Enums;
using Cms.Shared.Exceptions;
using FluentValidation;
using Ganss.Xss;

namespace Cms.Application.Services;

public sealed class WebsiteService : IWebsiteService
{
    private static readonly HtmlSanitizer TemplateSanitizer = CreateTemplateSanitizer();
    private readonly IWebsiteRepository _repository;
    private readonly ISiteContentRepository _contentRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ISiteContext _siteContext;
    private readonly ICurrentUserContext _currentUser;
    private readonly IValidator<ProvisionWebsiteDto> _provisionValidator;
    private readonly IValidator<SiteBrandingDto> _brandingValidator;
    private readonly IValidator<SubmitContactDto> _contactValidator;
    private readonly IValidator<SaveSiteDomainDto> _domainValidator;
    private readonly ITenantHostCache _hostCache;

    public WebsiteService(
        IWebsiteRepository repository,
        ISiteContentRepository contentRepository,
        ITenantContext tenantContext,
        ISiteContext siteContext,
        ICurrentUserContext currentUser,
        IValidator<ProvisionWebsiteDto> provisionValidator,
        IValidator<SiteBrandingDto> brandingValidator,
        IValidator<SubmitContactDto> contactValidator,
        IValidator<SaveSiteDomainDto> domainValidator,
        ITenantHostCache hostCache)
    {
        _repository = repository;
        _contentRepository = contentRepository;
        _tenantContext = tenantContext;
        _siteContext = siteContext;
        _currentUser = currentUser;
        _provisionValidator = provisionValidator;
        _brandingValidator = brandingValidator;
        _contactValidator = contactValidator;
        _domainValidator = domainValidator;
        _hostCache = hostCache;
    }

    public async Task<IReadOnlyList<PageTemplateDto>> GetPageTemplatesAsync(CancellationToken cancellationToken)
    {
        var templates = await _repository.GetPageTemplatesAsync(cancellationToken);
        return templates.Select(ToDto).ToList();
    }

    public async Task<PageTemplateDto> SavePageTemplateAsync(
        Guid? id, SavePageTemplateDto dto, CancellationToken cancellationToken)
    {
        dto.TemplateKey = dto.TemplateKey.Trim().ToLowerInvariant().Replace(' ', '-');
        dto.DefaultSlug = dto.DefaultSlug.Trim().ToLowerInvariant().Replace(' ', '-');
        if (string.IsNullOrWhiteSpace(dto.DefaultTitle))
        {
            dto.DefaultTitle = dto.Name.Trim();
        }

        if (string.IsNullOrWhiteSpace(dto.TemplateKey) || string.IsNullOrWhiteSpace(dto.Name)
            || string.IsNullOrWhiteSpace(dto.DefaultSlug))
        {
            throw new ValidationAppException("Template key, name and default slug are required.");
        }

        PageTemplate template;
        if (id.HasValue)
        {
            template = await _repository.GetPageTemplateByIdAsync(id.Value, cancellationToken)
                ?? throw new NotFoundException("Page template was not found.");
            template.UpdatedDate = DateTime.UtcNow;
            template.UpdatedBy = Actor;
        }
        else
        {
            var existing = await _repository.GetPageTemplateAsync(dto.TemplateKey, cancellationToken);
            if (existing is not null)
            {
                throw new ValidationAppException($"Template key '{dto.TemplateKey}' already exists.");
            }

            template = new PageTemplate
            {
                CreatedDate = DateTime.UtcNow,
                CreatedBy = Actor
            };
            await _repository.AddPageTemplateAsync(template, cancellationToken);
        }

        template.TemplateKey = dto.TemplateKey;
        template.Name = dto.Name.Trim();
        template.Description = dto.Description?.Trim();
        template.PageType = dto.PageType;
        template.DefaultSlug = dto.DefaultSlug;
        template.DefaultTitle = dto.DefaultTitle?.Trim();
        template.DefaultContent = string.IsNullOrWhiteSpace(dto.DefaultContent)
            ? dto.DefaultContent
            : TemplateSanitizer.Sanitize(dto.DefaultContent);
        template.DefaultJsonData = dto.DefaultJsonData;
        template.IsStarter = dto.IsStarter;
        template.IsActive = dto.IsActive;
        template.DisplayOrder = dto.DisplayOrder;
        await _repository.SaveChangesAsync(cancellationToken);
        return ToDto(template);
    }

    public async Task<IReadOnlyList<PublicPageDto>> AssignTemplatesAsync(
        AssignTemplatesDto dto, CancellationToken cancellationToken)
    {
        var (tenantId, siteId) = RequireContext();
        if (dto.TemplateKeys.Count == 0)
        {
            throw new ValidationAppException("Select at least one page template to assign.");
        }

        var existingPages = await _repository.GetPagesAsync(tenantId, siteId, activeOnly: false, cancellationToken);
        var existingSlugs = existingPages.Select(x => x.Slug).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var created = new List<PublicPageDto>();
        var order = existingPages.Count == 0 ? 1 : existingPages.Max(x => x.MenuOrder) + 1;

        foreach (var key in dto.TemplateKeys.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var template = await _repository.GetPageTemplateAsync(key, cancellationToken)
                ?? throw new ValidationAppException($"Unknown page template '{key}'.");
            if (!template.IsActive)
            {
                throw new ValidationAppException($"Page template '{key}' is inactive.");
            }

            if (existingSlugs.Contains(template.DefaultSlug))
            {
                continue;
            }

            var page = new Page
            {
                TenantId = tenantId,
                SiteId = siteId,
                PageType = template.PageType,
                TemplateKey = template.TemplateKey,
                Title = template.DefaultTitle ?? template.Name,
                Slug = template.DefaultSlug,
                Content = template.DefaultContent,
                JsonData = template.DefaultJsonData,
                ShowInMenu = template.PageType != PageType.Home,
                MenuOrder = order++,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = Actor
            };
            await _repository.AddPageAsync(page, cancellationToken);
            created.Add(new PublicPageDto
            {
                Id = page.Id,
                Title = page.Title,
                Slug = page.Slug,
                PageType = page.PageType,
                TemplateKey = page.TemplateKey,
                Content = page.Content,
                JsonData = page.JsonData
            });
        }

        await _repository.SaveChangesAsync(cancellationToken);
        await SyncHeaderMenuAsync(cancellationToken);
        return created;
    }

    /// <summary>
    /// Keeps the header menu in step with the pages, without taking the menu over.
    ///
    /// It adds a link for a page that has none and drops the link for a page that has been
    /// hidden or deactivated. It deliberately leaves everything else alone: renamed labels,
    /// hand-set order, link targets and links to addresses that are not pages at all (/news,
    /// /events, an external site). Rebuilding the whole menu from the page list — which is what
    /// this did — silently discarded every edit made on the Navigation screen the next time
    /// anyone saved any page, which made that screen pointless.
    /// </summary>
    public async Task SyncHeaderMenuAsync(CancellationToken cancellationToken)
    {
        var (tenantId, siteId) = RequireContext();
        var allPages = await _repository.GetPagesAsync(tenantId, siteId, activeOnly: false, cancellationToken);
        var linkable = allPages
            .Where(p => p.IsActive && p.ShowInMenu)
            .OrderBy(p => p.MenuOrder)
            .ThenBy(p => p.Title)
            .ToList();

        var menu = await _repository.GetMenuByLocationAsync(tenantId, siteId, "header", cancellationToken);
        if (menu is null)
        {
            menu = new Menu
            {
                TenantId = tenantId,
                SiteId = siteId,
                Name = "Main navigation",
                Location = "header",
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = Actor
            };
            await _repository.AddMenuAsync(menu, cancellationToken);
        }

        var wanted = linkable.Select(p => "/" + p.Slug).ToHashSet(StringComparer.OrdinalIgnoreCase);

        // A link is only withdrawn when it points at a page that exists and is no longer meant to
        // be in the menu. A link to something that is not a page is somebody's own addition.
        var hiddenPageUrls = allPages
            .Where(p => !p.IsActive || !p.ShowInMenu)
            .Select(p => "/" + p.Slug)
            .Where(url => !wanted.Contains(url))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var stale = menu.Items.Where(item => hiddenPageUrls.Contains(item.Url)).ToList();
        if (stale.Count > 0)
        {
            _repository.RemoveMenuItems(stale);
            foreach (var item in stale)
            {
                menu.Items.Remove(item);
            }
        }

        var linked = menu.Items.Select(item => item.Url).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var nextOrder = menu.Items.Count == 0 ? 0 : menu.Items.Max(item => item.DisplayOrder) + 1;

        if (!linked.Contains("/"))
        {
            menu.Items.Add(NewMenuItem(tenantId, siteId, menu.Id, "Home", "/", nextOrder++));
            linked.Add("/");
        }

        foreach (var page in linkable.Where(p => !linked.Contains("/" + p.Slug)))
        {
            menu.Items.Add(NewMenuItem(tenantId, siteId, menu.Id, page.Title, "/" + page.Slug, nextOrder++));
        }

        menu.UpdatedDate = DateTime.UtcNow;
        menu.UpdatedBy = Actor;

        await _repository.SaveChangesAsync(cancellationToken);
    }

    private MenuItem NewMenuItem(
        Guid tenantId, Guid siteId, Guid menuId, string label, string url, int order) =>
        new()
        {
            TenantId = tenantId,
            SiteId = siteId,
            MenuId = menuId,
            Label = label,
            Url = url,
            DisplayOrder = order,
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = Actor
        };

    public async Task<IReadOnlyList<WebsiteSummaryDto>> GetWebsitesAsync(CancellationToken cancellationToken)
    {
        var tenantId = RequireTenant();
        var sites = await _repository.GetSitesAsync(tenantId, cancellationToken);
        var result = new List<WebsiteSummaryDto>();
        foreach (var site in sites)
        {
            result.Add(new WebsiteSummaryDto
            {
                Id = site.Id,
                Name = site.Name,
                SiteKey = site.SiteKey,
                WebsiteType = site.WebsiteType,
                HomeVariant = site.HomeVariant,
                IsDefault = site.IsDefault,
                IsActive = site.IsActive,
                LogoUrl = site.LogoUrl,
                Tagline = site.Tagline,
                PrimaryColor = site.PrimaryColor,
                Domains = site.Domains.Where(d => d.IsActive).Select(d => d.DomainName).OrderBy(x => x).ToList(),
                PageCount = await _repository.CountPagesAsync(tenantId, site.Id, cancellationToken)
            });
        }

        return result;
    }

    public async Task<WebsiteSummaryDto> ProvisionAsync(ProvisionWebsiteDto dto, CancellationToken cancellationToken)
    {
        dto.SiteKey = dto.SiteKey.Trim().ToLowerInvariant();
        dto.DomainName = string.IsNullOrWhiteSpace(dto.DomainName)
            ? null
            : dto.DomainName.Trim().ToLowerInvariant();
        if (dto.TemplateKeys.Count == 0)
        {
            dto.TemplateKeys = PageTemplateKeys.StarterPages.ToList();
        }

        await _provisionValidator.ValidateAndThrowAsync(dto, cancellationToken);
        var tenantId = RequireTenant();

        var existing = await _repository.GetSiteByKeyAsync(tenantId, dto.SiteKey, cancellationToken);
        if (existing is not null)
        {
            throw new ValidationAppException($"A website with key '{dto.SiteKey}' already exists.");
        }

        if (!string.IsNullOrWhiteSpace(dto.DomainName))
        {
            var domainTaken = await _repository.GetDomainAsync(dto.DomainName, cancellationToken);
            if (domainTaken is not null)
            {
                throw new ValidationAppException($"Domain '{dto.DomainName}' is already assigned.");
            }
        }

        if (dto.IsDefault)
        {
            var sites = await _repository.GetSitesAsync(tenantId, cancellationToken);
            foreach (var site in sites.Where(s => s.IsDefault))
            {
                site.IsDefault = false;
            }
        }

        var website = new Site
        {
            TenantId = tenantId,
            Name = dto.Name.Trim(),
            SiteKey = dto.SiteKey,
            WebsiteType = dto.WebsiteType,
            HomeVariant = dto.HomeVariant,
            IsDefault = dto.IsDefault,
            IsActive = true,
            LogoUrl = dto.LogoUrl?.Trim(),
            HeaderImageUrl = dto.HeaderImageUrl?.Trim(),
            Tagline = dto.Tagline?.Trim(),
            PrimaryColor = dto.PrimaryColor?.Trim() ?? "#0f2d5c",
            SecondaryColor = dto.SecondaryColor?.Trim() ?? "#c9a227",
            Address = dto.Address?.Trim(),
            Phone = dto.Phone?.Trim(),
            Email = dto.Email?.Trim(),
            FooterText = $"© {DateTime.UtcNow.Year} {dto.Name.Trim()}. All rights reserved.",
            CreatedDate = DateTime.UtcNow,
            CreatedBy = Actor
        };
        await _repository.AddSiteAsync(website, cancellationToken);

        if (!string.IsNullOrWhiteSpace(dto.DomainName))
        {
            await _repository.AddDomainAsync(new TenantDomain
            {
                TenantId = tenantId,
                SiteId = website.Id,
                DomainName = dto.DomainName,
                IsPrimary = true,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = Actor
            }, cancellationToken);
        }

        var menuItems = new List<MenuItem>
        {
            new()
            {
                TenantId = tenantId,
                SiteId = website.Id,
                Label = "Home",
                Url = "/",
                DisplayOrder = 0,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = Actor
            }
        };

        var order = 1;
        foreach (var templateKey in dto.TemplateKeys.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var template = await _repository.GetPageTemplateAsync(templateKey, cancellationToken)
                ?? throw new ValidationAppException($"Unknown page template '{templateKey}'.");
            if (!template.IsActive)
            {
                throw new ValidationAppException($"Page template '{templateKey}' is inactive.");
            }

            var page = new Page
            {
                TenantId = tenantId,
                SiteId = website.Id,
                PageType = template.PageType,
                TemplateKey = template.TemplateKey,
                Title = template.DefaultTitle ?? template.Name,
                Slug = template.DefaultSlug,
                Content = template.DefaultContent,
                JsonData = template.DefaultJsonData,
                ShowInMenu = template.PageType != PageType.Home,
                MenuOrder = order,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = Actor
            };
            await _repository.AddPageAsync(page, cancellationToken);

            if (page.ShowInMenu)
            {
                menuItems.Add(new MenuItem
                {
                    TenantId = tenantId,
                    SiteId = website.Id,
                    Label = page.Title,
                    Url = $"/{page.Slug}",
                    DisplayOrder = order,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = Actor
                });
            }

            order++;
        }

        await _repository.AddMenuAsync(new Menu
        {
            TenantId = tenantId,
            SiteId = website.Id,
            Name = "Main navigation",
            Location = "header",
            IsActive = true,
            Items = menuItems,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = Actor
        }, cancellationToken);

        await _repository.AddSeoAsync(new SeoSetting
        {
            TenantId = tenantId,
            SiteId = website.Id,
            MetaTitle = website.Name,
            MetaDescription = website.Tagline ?? $"{website.Name} — official website",
            CreatedDate = DateTime.UtcNow,
            CreatedBy = Actor
        }, cancellationToken);

        await _repository.SaveChangesAsync(cancellationToken);
        await _repository.EnsureHomeSectionsAsync(tenantId, website.Id, cancellationToken);

        // The new website must answer on its /{siteKey} URL immediately, not once the
        // host-resolution cache happens to expire.
        _hostCache.Invalidate();

        return (await GetWebsitesAsync(cancellationToken)).First(x => x.Id == website.Id);
    }

    // -----------------------------------------------------------------------
    // Website templates
    // -----------------------------------------------------------------------

    public Task<IReadOnlyList<SiteTemplateSummaryDto>> GetSiteTemplatesAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<SiteTemplateSummaryDto>>(SiteTemplateCatalog.All
            .Select(x => new SiteTemplateSummaryDto
            {
                Key = x.Key,
                Name = x.Name,
                Summary = x.Summary,
                BestFor = x.BestFor,
                WebsiteType = x.WebsiteType,
                HomeVariant = x.HomeVariant,
                PrimaryColor = x.PrimaryColor,
                SecondaryColor = x.SecondaryColor,
                SampleTagline = x.SampleTagline,
                Highlights = x.Highlights,
                PageCount = x.PageTemplateKeys.Count
            })
            .ToList());

    /// <summary>
    /// Creates a website that already looks finished: the standard provisioning run, then the
    /// template's palette, hero copy, statistics and sample staff, notices, events and
    /// departments layered on top. Everything written here is sample copy for the school to
    /// replace, so it is only ever applied to a website being created.
    /// </summary>
    public async Task<WebsiteSummaryDto> ProvisionFromTemplateAsync(
        ProvisionFromTemplateDto dto, CancellationToken cancellationToken)
    {
        var template = SiteTemplateCatalog.Find(dto.TemplateKey)
            ?? throw new ValidationAppException($"Unknown website template '{dto.TemplateKey}'.");

        var created = await ProvisionAsync(new ProvisionWebsiteDto
        {
            Name = dto.Name,
            SiteKey = dto.SiteKey,
            DomainName = dto.DomainName,
            WebsiteType = template.WebsiteType,
            HomeVariant = template.HomeVariant,
            PrimaryColor = template.PrimaryColor,
            SecondaryColor = template.SecondaryColor,
            Tagline = template.SampleTagline,
            HeaderImageUrl = template.HeroImageUrl,
            TemplateKeys = template.PageTemplateKeys.ToList()
        }, cancellationToken);

        var tenantId = RequireTenant();
        await ApplyHomeCopyAsync(tenantId, created.Id, template, dto.Name, cancellationToken);
        await ApplyPageCopyAsync(tenantId, created.Id, template, dto.Name, cancellationToken);

        if (dto.IncludeSampleContent)
        {
            await AddSampleContentAsync(tenantId, created.Id, template, cancellationToken);
        }

        return (await GetWebsitesAsync(cancellationToken)).First(x => x.Id == created.Id);
    }

    private async Task ApplyHomeCopyAsync(
        Guid tenantId, Guid siteId, SiteTemplate template, string schoolName, CancellationToken cancellationToken)
    {
        var sections = await _repository.GetHomeSectionsAsync(tenantId, siteId, cancellationToken);

        foreach (var section in sections)
        {
            switch (section.SectionKey)
            {
                case HomePageSectionKeys.Hero:
                    section.Title = template.HeroHeading;
                    section.SubTitle = template.HeroDescription;
                    // "description" is the section's own field, not configuration: holding it in
                    // both places gave the website two answers and the editor an error on save.
                    var heroConfig = new JsonObject
                    {
                        ["heading"] = template.HeroHeading,
                        ["primaryButton"] = "Apply now",
                        ["secondaryButton"] = "Visit us"
                    };

                    if (template.HeroImages.Count > 0)
                    {
                        var slides = new JsonArray();
                        foreach (var image in template.HeroImages)
                        {
                            slides.Add(new JsonObject
                            {
                                ["imageUrl"] = image,
                                ["alt"] = $"{schoolName} campus"
                            });
                        }

                        heroConfig["autoplaySeconds"] = template.HeroAutoplaySeconds;
                        heroConfig["items"] = slides;
                    }

                    section.JsonData = heroConfig.ToJsonString();
                    break;

                case HomePageSectionKeys.Statistics:
                    section.JsonData = new JsonObject
                    {
                        ["students"] = template.Statistics.Students,
                        ["teachers"] = template.Statistics.Teachers,
                        ["placements"] = template.Statistics.Placements,
                        ["years"] = template.Statistics.Years
                    }.ToJsonString();
                    break;

                case HomePageSectionKeys.Welcome:
                    section.Title = $"Welcome to {schoolName}";
                    section.Description = template.HeroDescription;
                    break;

                case HomePageSectionKeys.WhyChooseUs:
                    section.SubTitle = template.WhyIntro;
                    // The views read the intro out of the section's JSON, so write it there too
                    // rather than leaving the template's words in a field nothing renders.
                    var why = JsonNode.Parse(
                        string.IsNullOrWhiteSpace(section.JsonData) ? "{}" : section.JsonData) as JsonObject
                        ?? new JsonObject();
                    why["intro"] = template.WhyIntro;
                    section.JsonData = why.ToJsonString();
                    break;
            }

            section.UpdatedDate = DateTime.UtcNow;
            section.UpdatedBy = Actor;
        }

        await _repository.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Replaces the page gallery's placeholder text with the template's finished copy, so a
    /// site can be shown to a school without anyone first having to write eight pages.
    /// </summary>
    private async Task ApplyPageCopyAsync(
        Guid tenantId, Guid siteId, SiteTemplate template, string schoolName, CancellationToken cancellationToken)
    {
        if (template.PageContent.Count == 0)
        {
            return;
        }

        var pages = await _repository.GetPagesAsync(tenantId, siteId, activeOnly: false, cancellationToken);
        var changed = false;

        foreach (var page in pages)
        {
            if (page.TemplateKey is null
                || !template.PageContent.TryGetValue(page.TemplateKey, out var copy))
            {
                continue;
            }

            page.Content = TemplateSanitizer.Sanitize(copy.Replace("{name}", schoolName));
            page.UpdatedDate = DateTime.UtcNow;
            page.UpdatedBy = Actor;
            changed = true;
        }

        if (changed)
        {
            await _repository.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task AddSampleContentAsync(
        Guid tenantId, Guid siteId, SiteTemplate template, CancellationToken cancellationToken)
    {
        var order = 0;
        foreach (var person in template.Faculty)
        {
            await _contentRepository.AddEntryAsync(ContentEntry(tenantId, siteId, "person",
                Slug(person.FullName), person.FullName, person.Headline, null,
                new JsonObject
                {
                    ["designation"] = person.Designation,
                    ["category"] = person.Category,
                    ["qualification"] = person.Qualification
                }.ToJsonString(), null, order++), cancellationToken);
        }

        order = 0;
        foreach (var department in template.Departments)
        {
            await _contentRepository.AddEntryAsync(ContentEntry(tenantId, siteId, "department",
                Slug(department.Name), department.Name, department.Summary, null,
                new JsonObject
                {
                    ["programmes"] = new JsonArray(
                        department.Programmes.Select(x => (JsonNode)JsonValue.Create(x)!).ToArray())
                }.ToJsonString(), null, order++), cancellationToken);
        }

        order = 0;
        var publishedOn = DateTime.UtcNow;
        foreach (var item in template.News)
        {
            await _contentRepository.AddEntryAsync(ContentEntry(tenantId, siteId, "news",
                Slug(item.Headline), item.Headline, item.Summary, null,
                new JsonObject
                {
                    ["category"] = item.Category,
                    ["isFeatured"] = item.IsFeatured
                }.ToJsonString(), publishedOn.AddDays(-order * 9), order++), cancellationToken);
        }

        order = 0;
        foreach (var item in template.Events)
        {
            await _contentRepository.AddEntryAsync(ContentEntry(tenantId, siteId, "event",
                Slug(item.Title), item.Title, item.Summary, null,
                new JsonObject
                {
                    ["venue"] = item.Venue,
                    ["endsOn"] = DateTime.UtcNow.AddDays(item.DaysFromNow).AddHours(3).ToString("O")
                }.ToJsonString(), DateTime.UtcNow.AddDays(item.DaysFromNow), order++), cancellationToken);
        }

        await _contentRepository.SaveChangesAsync(cancellationToken);
    }

    private ContentEntry ContentEntry(
        Guid tenantId, Guid siteId, string type, string key, string title,
        string? summary, string? body, string? json, DateTime? publishDate, int order) => new()
        {
            TenantId = tenantId,
            SiteId = siteId,
            ContentType = type,
            Key = key,
            Title = title,
            Summary = summary,
            Body = body,
            JsonData = json,
            PublishDate = publishDate,
            DisplayOrder = order,
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = Actor
        };

    private static string Slug(string value)
    {
        var builder = new System.Text.StringBuilder(value.Length);
        var lastSeparator = false;
        foreach (var character in value.Trim().ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(character) && character < 128)
            {
                builder.Append(character);
                lastSeparator = false;
            }
            else if (!lastSeparator && builder.Length > 0)
            {
                builder.Append('-');
                lastSeparator = true;
            }
        }

        return builder.ToString().Trim('-');
    }

    // -----------------------------------------------------------------------
    // Domains
    //
    // Host resolution is what makes one deployment serve unlimited institutions, so
    // these rules are enforced here rather than trusted to the UI:
    //   * a host may belong to exactly one tenant, globally;
    //   * binding a host to a website gives that school clean root URLs;
    //   * leaving it unbound shares the host across the tenant's websites by path.
    // -----------------------------------------------------------------------

    public async Task<IReadOnlyList<SiteDomainDto>> GetDomainsAsync(CancellationToken cancellationToken)
    {
        var tenantId = RequireTenant();
        var domains = await _repository.GetDomainsAsync(tenantId, cancellationToken);
        var sites = await _repository.GetSitesAsync(tenantId, cancellationToken);
        var fallbackKey = sites.FirstOrDefault(x => x.IsDefault)?.SiteKey ?? sites.FirstOrDefault()?.SiteKey;

        return domains.Select(domain =>
        {
            var site = domain.SiteId is Guid id ? sites.FirstOrDefault(x => x.Id == id) : null;
            return new SiteDomainDto
            {
                Id = domain.Id,
                DomainName = domain.DomainName,
                SiteId = domain.SiteId,
                SiteName = site?.Name,
                SiteKey = site?.SiteKey ?? fallbackKey,
                IsPrimary = domain.IsPrimary,
                IsActive = domain.IsActive
            };
        }).ToList();
    }

    public async Task<SiteDomainDto> SaveDomainAsync(
        Guid? id, SaveSiteDomainDto dto, CancellationToken cancellationToken)
    {
        dto.DomainName = dto.DomainName.Trim().ToLowerInvariant().TrimEnd('.');
        await _domainValidator.ValidateAndThrowAsync(dto, cancellationToken);
        var tenantId = RequireTenant();

        // Globally unique: two tenants cannot claim the same host.
        var existingHost = await _repository.GetDomainAsync(dto.DomainName, cancellationToken);
        if (existingHost is not null && existingHost.Id != id)
        {
            throw new ValidationAppException(
                $"'{dto.DomainName}' is already in use. Each host can serve only one website.");
        }

        if (dto.SiteId is Guid targetSiteId
            && await _repository.GetSiteAsync(tenantId, targetSiteId, cancellationToken) is null)
        {
            throw new ValidationAppException("Select a website that belongs to this workspace.");
        }

        TenantDomain domain;
        if (id.HasValue)
        {
            domain = await _repository.GetDomainByIdAsync(tenantId, id.Value, cancellationToken)
                ?? throw new NotFoundException("Domain was not found.");
            domain.UpdatedDate = DateTime.UtcNow;
            domain.UpdatedBy = Actor;
        }
        else
        {
            domain = new TenantDomain
            {
                TenantId = tenantId,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = Actor
            };
            await _repository.AddDomainAsync(domain, cancellationToken);
        }

        // Taking the last live host off a bound website makes that school unreachable,
        // so it must be an explicit act elsewhere, not a side effect of an edit here.
        if (id.HasValue && domain.SiteId is Guid boundSite
            && (!dto.IsActive || dto.SiteId != domain.SiteId))
        {
            await GuardLastLiveHostAsync(
                tenantId, boundSite, domain.Id, domain.IsActive, cancellationToken);
        }

        domain.DomainName = dto.DomainName;
        domain.SiteId = dto.SiteId;
        domain.IsActive = dto.IsActive;
        domain.IsPrimary = dto.IsPrimary;

        if (dto.IsPrimary)
        {
            // One primary per website, so each school has a single canonical address.
            var siblings = await _repository.GetDomainsAsync(tenantId, cancellationToken);
            foreach (var other in siblings.Where(x => x.Id != domain.Id && x.SiteId == dto.SiteId))
            {
                other.IsPrimary = false;
            }
        }

        await _repository.SaveChangesAsync(cancellationToken);
        _hostCache.Invalidate();
        return (await GetDomainsAsync(cancellationToken)).First(x => x.Id == domain.Id);
    }

    public async Task DeleteDomainAsync(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = RequireTenant();
        var domain = await _repository.GetDomainByIdAsync(tenantId, id, cancellationToken)
            ?? throw new NotFoundException("Domain was not found.");

        if (domain.SiteId is Guid boundSite)
        {
            await GuardLastLiveHostAsync(
                tenantId, boundSite, domain.Id, domain.IsActive, cancellationToken);
        }

        _repository.RemoveDomain(domain);
        await _repository.SaveChangesAsync(cancellationToken);
        _hostCache.Invalidate();
    }

    /// <summary>
    /// Refuses an edit that would leave a website with no live host, which would make that
    /// school's site unreachable without any warning.
    /// </summary>
    /// <summary>
    /// Refuses to take a live website's last address away.
    ///
    /// It only applies when there is something to strand. A host that is already switched off is
    /// serving nobody, and a website that has been switched off has nobody to lose — guarding
    /// those left a school's only domain impossible to remove or even disable, with no way out
    /// of the console at all.
    /// </summary>
    private async Task GuardLastLiveHostAsync(
        Guid tenantId,
        Guid siteId,
        Guid excludingDomainId,
        bool hostIsLive,
        CancellationToken cancellationToken)
    {
        if (!hostIsLive)
        {
            return;
        }

        var site = await _repository.GetSiteAsync(tenantId, siteId, cancellationToken);
        if (site is null || !site.IsActive)
        {
            return;
        }

        var remaining = (await _repository.GetDomainsAsync(tenantId, cancellationToken))
            .Count(x => x.SiteId == siteId && x.IsActive && x.Id != excludingDomainId);

        if (remaining == 0)
        {
            throw new ValidationAppException(
                $"'{site.Name}' has no other live address, so this would take the website off the "
                + "internet. Add another domain first, or switch the website off under Websites "
                + "and then remove it.");
        }
    }

    public async Task<SiteBrandingDto> GetBrandingAsync(CancellationToken cancellationToken)
    {
        var (tenantId, siteId) = RequireContext();
        var site = await _repository.GetSiteAsync(tenantId, siteId, cancellationToken)
            ?? throw new NotFoundException("Website was not found.");
        return ToBranding(site);
    }

    public async Task<SiteBrandingDto> SaveBrandingAsync(SiteBrandingDto dto, CancellationToken cancellationToken)
    {
        await _brandingValidator.ValidateAndThrowAsync(dto, cancellationToken);
        var (tenantId, siteId) = RequireContext();
        var site = await _repository.GetSiteAsync(tenantId, siteId, cancellationToken)
            ?? throw new NotFoundException("Website was not found.");

        site.Name = dto.Name.Trim();
        site.WebsiteType = dto.WebsiteType;
        site.HomeVariant = dto.HomeVariant;
        site.LogoUrl = dto.LogoUrl?.Trim();
        site.FaviconUrl = dto.FaviconUrl?.Trim();
        site.Tagline = dto.Tagline?.Trim();
        site.PrimaryColor = dto.PrimaryColor?.Trim();
        site.SecondaryColor = dto.SecondaryColor?.Trim();
        site.HeaderImageUrl = dto.HeaderImageUrl?.Trim();
        site.FooterText = dto.FooterText?.Trim();
        site.Address = dto.Address?.Trim();
        site.Phone = dto.Phone?.Trim();
        site.Email = dto.Email?.Trim();
        site.MapEmbedUrl = dto.MapEmbedUrl?.Trim();
        site.SocialLinksJson = dto.SocialLinksJson;
        site.UpdatedDate = DateTime.UtcNow;
        site.UpdatedBy = Actor;
        await _repository.SaveChangesAsync(cancellationToken);
        return ToBranding(site);
    }

    public async Task<PublicWebsiteDto> GetPublicWebsiteAsync(CancellationToken cancellationToken)
    {
        var (tenantId, siteId) = RequireContext();
        var site = await _repository.GetSiteAsync(tenantId, siteId, cancellationToken)
            ?? throw new NotFoundException("Website was not found.");
        var seo = await _repository.GetSeoAsync(tenantId, siteId, cancellationToken);
        var menu = await _repository.GetMenuByLocationAsync(tenantId, siteId, "header", cancellationToken);
        var sections = await _repository.GetHomeSectionsAsync(tenantId, siteId, cancellationToken);

        // Empty on a host bound to this one website, "/{siteKey}" when the host is shared.
        var siteBasePath = _siteContext.BasePath;

        IReadOnlyList<PublicNavItemDto> navigation;
        if (menu is not null && menu.Items.Count > 0)
        {
            navigation = menu.Items.Where(i => i.IsActive)
                .OrderBy(i => i.DisplayOrder)
                .Select(i => new PublicNavItemDto
                {
                    Label = i.Label,
                    Url = ToPublicUrl(i.Url, siteBasePath) ?? HomeUrl(siteBasePath),
                    Target = i.Target
                })
                .ToList();
        }
        else
        {
            var pages = await _repository.GetPagesAsync(tenantId, siteId, activeOnly: true, cancellationToken);
            navigation =
            [
                new PublicNavItemDto { Label = "Home", Url = HomeUrl(siteBasePath) },
                .. pages.Where(p => p.ShowInMenu).OrderBy(p => p.MenuOrder)
                    .Select(p => new PublicNavItemDto { Label = p.Title, Url = $"{siteBasePath}/{p.Slug}" })
            ];
        }

        return new PublicWebsiteDto
        {
            BasePath = siteBasePath,
            Branding = ToBranding(site),
            Seo = new SeoPublicDto
            {
                MetaTitle = seo?.MetaTitle ?? site.Name,
                MetaDescription = seo?.MetaDescription ?? site.Tagline,
                MetaKeywords = seo?.MetaKeywords,
                OgImageUrl = seo?.OgImageUrl ?? site.LogoUrl,
                CanonicalUrl = seo?.CanonicalUrl,
                AllowIndexing = seo?.AllowIndexing ?? true
            },
            Navigation = navigation,
            HomeSections = sections.OrderBy(s => s.DisplayOrder).Select(s => new HomeSectionPublicDto
            {
                SectionKey = s.SectionKey,
                Title = s.Title ?? string.Empty,
                SubTitle = s.SubTitle,
                Description = s.Description,
                ButtonText = s.ButtonText,
                ButtonLink = ToPublicUrl(s.ButtonLink, siteBasePath),
                ImageUrl = s.ImageUrl,
                JsonData = s.JsonData,
                DisplayOrder = s.DisplayOrder
            }).ToList()
        };
    }

    public async Task<PublicPageDto> GetPublicPageAsync(string slug, CancellationToken cancellationToken)
    {
        var (tenantId, siteId) = RequireContext();
        var page = await _repository.GetPageBySlugAsync(
            tenantId, siteId, slug.Trim().ToLowerInvariant(), cancellationToken)
            ?? throw new NotFoundException("Page was not found.");
        if (!page.IsActive)
        {
            throw new NotFoundException("Page was not found.");
        }

        return new PublicPageDto
        {
            Id = page.Id,
            Title = page.Title,
            Slug = page.Slug,
            PageType = page.PageType,
            TemplateKey = page.TemplateKey,
            Excerpt = page.Excerpt,
            Content = page.Content,
            JsonData = page.JsonData,
            FeaturedImageUrl = page.FeaturedImageUrl,
            MetaTitle = page.MetaTitle,
            MetaDescription = page.MetaDescription
        };
    }

    public async Task<IReadOnlyList<ContactSubmissionDto>> GetContactSubmissionsAsync(CancellationToken cancellationToken)
    {
        var (tenantId, siteId) = RequireContext();
        var items = await _repository.GetContactsAsync(tenantId, siteId, cancellationToken);
        return items.Select(ToDto).ToList();
    }

    public async Task<int> GetUnreadContactCountAsync(CancellationToken cancellationToken)
    {
        if (!_siteContext.IsResolved || _tenantContext.TenantId is not Guid tenantId)
        {
            return 0;
        }

        return await _repository.CountUnreadContactsAsync(tenantId, _siteContext.SiteId!.Value, cancellationToken);
    }

    public async Task<ContactSubmissionDto> SubmitContactAsync(SubmitContactDto dto, CancellationToken cancellationToken)
    {
        await _contactValidator.ValidateAndThrowAsync(dto, cancellationToken);
        var (tenantId, siteId) = RequireContext();
        var submission = new ContactSubmission
        {
            TenantId = tenantId,
            SiteId = siteId,
            Name = dto.Name.Trim(),
            Email = dto.Email.Trim(),
            Phone = dto.Phone?.Trim(),
            Subject = dto.Subject?.Trim(),
            Message = dto.Message.Trim(),
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "public"
        };
        await _repository.AddContactAsync(submission, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return ToDto(submission);
    }

    public async Task MarkContactReadAsync(Guid id, bool isRead, CancellationToken cancellationToken)
    {
        var (tenantId, siteId) = RequireContext();
        var item = await _repository.GetContactAsync(tenantId, siteId, id, cancellationToken)
            ?? throw new NotFoundException("Contact submission was not found.");
        item.IsRead = isRead;
        item.UpdatedDate = DateTime.UtcNow;
        item.UpdatedBy = Actor;
        await _repository.SaveChangesAsync(cancellationToken);
    }

    private string Actor => _currentUser.UserId ?? "system";

    private Guid RequireTenant()
    {
        if (!_tenantContext.IsResolved || !_tenantContext.TenantId.HasValue)
        {
            throw new TenantNotResolvedException();
        }

        return _tenantContext.TenantId.Value;
    }

    private (Guid TenantId, Guid SiteId) RequireContext()
    {
        if (!_tenantContext.IsResolved || !_siteContext.IsResolved
            || !_tenantContext.TenantId.HasValue || !_siteContext.SiteId.HasValue)
        {
            throw new TenantNotResolvedException();
        }

        return (_tenantContext.TenantId.Value, _siteContext.SiteId.Value);
    }

    private static PageTemplateDto ToDto(PageTemplate x) => new()
    {
        Id = x.Id,
        TemplateKey = x.TemplateKey,
        Name = x.Name,
        Description = x.Description,
        PageType = x.PageType,
        DefaultSlug = x.DefaultSlug,
        DefaultTitle = x.DefaultTitle,
        DefaultContent = x.DefaultContent,
        DefaultJsonData = x.DefaultJsonData,
        IsStarter = x.IsStarter,
        IsActive = x.IsActive,
        DisplayOrder = x.DisplayOrder
    };

    private static SiteBrandingDto ToBranding(Site x) => new()
    {
        Name = x.Name,
        SiteKey = x.SiteKey,
        WebsiteType = x.WebsiteType,
        HomeVariant = x.HomeVariant,
        LogoUrl = x.LogoUrl,
        FaviconUrl = x.FaviconUrl,
        Tagline = x.Tagline,
        PrimaryColor = x.PrimaryColor,
        SecondaryColor = x.SecondaryColor,
        HeaderImageUrl = x.HeaderImageUrl,
        FooterText = x.FooterText,
        Address = x.Address,
        Phone = x.Phone,
        Email = x.Email,
        MapEmbedUrl = x.MapEmbedUrl,
        SocialLinksJson = x.SocialLinksJson
    };

    private static ContactSubmissionDto ToDto(ContactSubmission x) => new()
    {
        Id = x.Id,
        Name = x.Name,
        Email = x.Email,
        Phone = x.Phone,
        Subject = x.Subject,
        Message = x.Message,
        IsRead = x.IsRead,
        CreatedDate = x.CreatedDate
    };

    private static string HomeUrl(string siteBasePath) =>
        siteBasePath.Length == 0 ? "/" : siteBasePath;

    private static string? ToPublicUrl(string? url, string siteBasePath)
    {
        if (string.IsNullOrWhiteSpace(url) || !url.StartsWith('/'))
        {
            return url;
        }

        if (url == "/")
        {
            return HomeUrl(siteBasePath);
        }

        if (siteBasePath.Length == 0)
        {
            return url;
        }

        return url.StartsWith(siteBasePath + "/", StringComparison.OrdinalIgnoreCase)
            ? url
            : siteBasePath + url;
    }

    private static HtmlSanitizer CreateTemplateSanitizer()
    {
        var sanitizer = new HtmlSanitizer();
        sanitizer.AllowedAttributes.Add("class");
        sanitizer.AllowedAttributes.Add("target");
        sanitizer.AllowedSchemes.Add("tel");
        return sanitizer;
    }
}
