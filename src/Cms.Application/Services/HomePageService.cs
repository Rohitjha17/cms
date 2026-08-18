using Cms.Application.DTOs.HomePage;
using Cms.Application.Interfaces;
using Cms.Application.Mapping;
using Cms.Application.Validators;
using Cms.Domain.Constants;
using Cms.Domain.Entities;
using Cms.Shared.Exceptions;
using Ganss.Xss;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace Cms.Application.Services;

public class HomePageService : IHomePageService
{
    private static readonly HtmlSanitizer RichTextSanitizer = CreateSanitizer();
    private readonly IHomePageRepository _repository;
    private readonly ITenantContext _tenantContext;
    private readonly ISiteContext _siteContext;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IMapper _mapper;
    private readonly ILogger<HomePageService> _logger;

    public HomePageService(
        IHomePageRepository repository,
        ITenantContext tenantContext,
        ISiteContext siteContext,
        ICurrentUserContext currentUserContext,
        IMapper mapper,
        ILogger<HomePageService> logger)
    {
        _repository = repository;
        _tenantContext = tenantContext;
        _siteContext = siteContext;
        _currentUserContext = currentUserContext;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<HomePageResponseDto> GetHomePageAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var (tenantId, siteId) = RequireTenantSite();
        await EnsureDefaultSectionsInternalAsync(tenantId, siteId, cancellationToken);

        var sections = await _repository.GetAllAsync(tenantId, siteId, activeOnly: !includeInactive, cancellationToken);
        var response = new HomePageResponseDto();

        foreach (var section in sections.OrderBy(s => s.DisplayOrder))
        {
            response.Sections[section.SectionKey] = HomePageMapper.ToFrontendSection(section);
        }

        return response;
    }

    public async Task<IReadOnlyList<HomePageSectionDto>> GetSectionsAsync(bool includeInactive = true, CancellationToken cancellationToken = default)
    {
        var (tenantId, siteId) = RequireTenantSite();
        await EnsureDefaultSectionsInternalAsync(tenantId, siteId, cancellationToken);
        var sections = await _repository.GetAllAsync(tenantId, siteId, activeOnly: !includeInactive, cancellationToken);
        return sections.OrderBy(s => s.DisplayOrder)
            .Select(section => _mapper.Map<HomePageSectionDto>(section))
            .ToList();
    }

    public async Task<HomePageSectionDto> GetSectionAsync(string sectionKey, bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var (tenantId, siteId) = RequireTenantSite();
        var key = HomePageSectionKeys.Normalize(sectionKey);
        await EnsureDefaultSectionsInternalAsync(tenantId, siteId, cancellationToken);

        var section = await _repository.GetByKeyAsync(tenantId, siteId, key, cancellationToken)
            ?? throw new NotFoundException($"Homepage section '{key}' was not found.");

        if (!includeInactive && !section.IsActive)
        {
            throw new NotFoundException($"Homepage section '{key}' is not active.");
        }

        return _mapper.Map<HomePageSectionDto>(section);
    }

    public async Task<HomePageSectionDto> CreateSectionAsync(CreateHomePageSectionDto dto, CancellationToken cancellationToken = default)
    {
        var (tenantId, siteId) = RequireTenantSite();
        var key = HomePageSectionKeys.Normalize(dto.SectionKey);

        if (await _repository.ExistsAsync(tenantId, siteId, key, cancellationToken))
        {
            throw new ValidationAppException($"SectionKey '{key}' already exists for this site.");
        }

        var json = HomePageSectionConfigValidator.StripReservedFields(dto.JsonData, out var adopted);
        ValidateConfiguration(key, json);
        var entity = new HomePageSection
        {
            TenantId = tenantId,
            SiteId = siteId,
            SectionKey = key,
            Title = Adopt(dto.Title, adopted, "title"),
            SubTitle = Adopt(dto.SubTitle, adopted, "subtitle"),
            Description = SanitizeDescription(Adopt(dto.Description, adopted, "description")),
            ButtonText = Adopt(dto.ButtonText, adopted, "buttonText"),
            ButtonLink = Adopt(dto.ButtonLink, adopted, "buttonLink"),
            ImageUrl = Adopt(dto.ImageUrl, adopted, "imageUrl"),
            BackgroundImageUrl = Adopt(dto.BackgroundImageUrl, adopted, "backgroundImageUrl"),
            JsonData = json,
            DisplayOrder = dto.DisplayOrder,
            IsActive = dto.IsActive,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = _currentUserContext.UserId ?? "system"
        };

        await _repository.AddAsync(entity, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created homepage section {SectionKey} for tenant {TenantId} site {SiteId}", key, tenantId, siteId);
        return _mapper.Map<HomePageSectionDto>(entity);
    }

    public async Task<HomePageSectionDto> UpdateSectionAsync(string sectionKey, UpdateHomePageSectionDto dto, CancellationToken cancellationToken = default)
    {
        var (tenantId, siteId) = RequireTenantSite();
        var key = HomePageSectionKeys.Normalize(sectionKey);

        var entity = await _repository.GetByKeyAsync(tenantId, siteId, key, cancellationToken)
            ?? throw new NotFoundException($"Homepage section '{key}' was not found.");

        var json = HomePageSectionConfigValidator.StripReservedFields(dto.JsonData, out var adopted);
        ValidateConfiguration(key, json);
        entity.Title = Adopt(dto.Title, adopted, "title");
        entity.SubTitle = Adopt(dto.SubTitle, adopted, "subtitle");
        entity.Description = SanitizeDescription(Adopt(dto.Description, adopted, "description"));
        entity.ButtonText = Adopt(dto.ButtonText, adopted, "buttonText");
        entity.ButtonLink = Adopt(dto.ButtonLink, adopted, "buttonLink");
        entity.ImageUrl = Adopt(dto.ImageUrl, adopted, "imageUrl");
        entity.BackgroundImageUrl = Adopt(dto.BackgroundImageUrl, adopted, "backgroundImageUrl");
        entity.JsonData = json;

        if (dto.DisplayOrder.HasValue)
        {
            entity.DisplayOrder = dto.DisplayOrder.Value;
        }

        if (dto.IsActive.HasValue)
        {
            entity.IsActive = dto.IsActive.Value;
        }

        entity.UpdatedDate = DateTime.UtcNow;
        entity.UpdatedBy = _currentUserContext.UserId ?? "system";

        await _repository.UpdateAsync(entity, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated homepage section {SectionKey} for tenant {TenantId} site {SiteId}", key, tenantId, siteId);
        return _mapper.Map<HomePageSectionDto>(entity);
    }

    public async Task SetStatusAsync(string sectionKey, bool isActive, CancellationToken cancellationToken = default)
    {
        var (tenantId, siteId) = RequireTenantSite();
        var key = HomePageSectionKeys.Normalize(sectionKey);

        var entity = await _repository.GetByKeyAsync(tenantId, siteId, key, cancellationToken)
            ?? throw new NotFoundException($"Homepage section '{key}' was not found.");

        entity.IsActive = isActive;
        entity.UpdatedDate = DateTime.UtcNow;
        entity.UpdatedBy = _currentUserContext.UserId ?? "system";

        await _repository.UpdateAsync(entity, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task ReorderAsync(ReorderHomePageSectionsDto dto, CancellationToken cancellationToken = default)
    {
        var (tenantId, siteId) = RequireTenantSite();
        var existing = await _repository.GetAllAsync(
            tenantId, siteId, activeOnly: false, cancellationToken);
        var byKey = existing.ToDictionary(x => x.SectionKey, StringComparer.OrdinalIgnoreCase);
        var unknownKeys = dto.Items
            .Where(x => !byKey.ContainsKey(HomePageSectionKeys.Normalize(x.SectionKey)))
            .Select(x => x.SectionKey)
            .ToList();

        if (unknownKeys.Count > 0)
        {
            throw new ValidationAppException(
                $"Unknown homepage section keys: {string.Join(", ", unknownKeys)}.");
        }

        foreach (var item in dto.Items)
        {
            var key = HomePageSectionKeys.Normalize(item.SectionKey);
            var entity = byKey[key];
            entity.DisplayOrder = item.DisplayOrder;
            entity.UpdatedDate = DateTime.UtcNow;
            entity.UpdatedBy = _currentUserContext.UserId ?? "system";
            await _repository.UpdateAsync(entity, cancellationToken);
        }

        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteSectionAsync(string sectionKey, bool hardDelete = false, CancellationToken cancellationToken = default)
    {
        var (tenantId, siteId) = RequireTenantSite();
        var key = HomePageSectionKeys.Normalize(sectionKey);

        var entity = await _repository.GetByKeyAsync(tenantId, siteId, key, cancellationToken)
            ?? throw new NotFoundException($"Homepage section '{key}' was not found.");

        if (hardDelete)
        {
            if (HomePageSectionKeys.IsKnown(key))
            {
                throw new ValidationAppException(
                    "Built-in homepage sections cannot be permanently deleted. Disable the section instead.");
            }
            await _repository.DeleteAsync(entity, cancellationToken);
        }
        else
        {
            entity.IsActive = false;
            entity.UpdatedDate = DateTime.UtcNow;
            entity.UpdatedBy = _currentUserContext.UserId ?? "system";
            await _repository.UpdateAsync(entity, cancellationToken);
        }

        await _repository.SaveChangesAsync(cancellationToken);
    }

    public Task EnsureDefaultSectionsAsync(CancellationToken cancellationToken = default)
    {
        var (tenantId, siteId) = RequireTenantSite();
        return EnsureDefaultSectionsInternalAsync(tenantId, siteId, cancellationToken);
    }

    private async Task EnsureDefaultSectionsInternalAsync(Guid tenantId, Guid siteId, CancellationToken cancellationToken)
    {
        var existing = await _repository.GetAllAsync(tenantId, siteId, activeOnly: false, cancellationToken);
        var existingKeys = existing.Select(x => x.SectionKey).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var missing = HomePageSectionKeys.All
            .Where(x => !existingKeys.Contains(x.Key))
            .Select(x => new HomePageSection
            {
                TenantId = tenantId,
                SiteId = siteId,
                SectionKey = x.Key,
                Title = x.DisplayName,
                DisplayOrder = x.Order,
                IsActive = false,
                JsonData = GetDefaultJson(x.Key),
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "system"
            })
            .ToList();

        if (missing.Count == 0)
        {
            return;
        }

        await _repository.AddRangeAsync(missing, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Seeded {Count} homepage sections for tenant {TenantId} site {SiteId}", missing.Count, tenantId, siteId);
    }

    private static string? GetDefaultJson(string key) => key switch
    {
        HomePageSectionKeys.Hero => """{"heading":"Welcome","primaryButton":"Apply Now","secondaryButton":"Contact Us","videoUrl":""}""",
        HomePageSectionKeys.Statistics => """{"students":0,"teachers":0,"placements":0,"years":0}""",
        HomePageSectionKeys.Gallery => """{"items":[]}""",
        HomePageSectionKeys.UpcomingEvents => """{"items":[]}""",
        HomePageSectionKeys.Testimonials => """{"items":[]}""",
        HomePageSectionKeys.Partners => """{"items":[]}""",
        HomePageSectionKeys.Contact => """{"email":"","phone":"","address":"","mapEmbedUrl":""}""",
        _ => null
    };

    /// <summary>
    /// The field the editor shows always wins; a value rescued from the configuration only fills
    /// a field that was left empty, so nothing anybody typed is overwritten by old duplicate data.
    /// </summary>
    private static string? Adopt(string? value, IReadOnlyDictionary<string, string> adopted, string key) =>
        !string.IsNullOrWhiteSpace(value) ? value
            : adopted.TryGetValue(key, out var rescued) ? rescued
            : value;

    private static void ValidateConfiguration(string sectionKey, string? jsonData)
    {
        var errors = HomePageSectionConfigValidator.Validate(sectionKey, jsonData);
        if (errors.Count > 0)
        {
            throw new ValidationAppException(
                $"Configuration for homepage section '{sectionKey}' is invalid.",
                errors);
        }
    }

    private static HtmlSanitizer CreateSanitizer()
    {
        var sanitizer = new HtmlSanitizer();
        sanitizer.AllowedTags.UnionWith(["h2", "h3", "p", "br", "strong", "em", "u", "ul", "ol", "li", "blockquote", "a"]);
        sanitizer.AllowedAttributes.Add("href");
        sanitizer.AllowedSchemes.UnionWith(["http", "https", "mailto", "tel"]);
        return sanitizer;
    }

    private static string? SanitizeDescription(string? value) =>
        string.IsNullOrWhiteSpace(value) ? value : RichTextSanitizer.Sanitize(value);

    private (Guid TenantId, Guid SiteId) RequireTenantSite()
    {
        if (!_tenantContext.IsResolved || _tenantContext.TenantId is null)
        {
            throw new TenantNotResolvedException();
        }

        if (!_siteContext.IsResolved || _siteContext.SiteId is null)
        {
            throw new TenantNotResolvedException("Unable to resolve site for this request.");
        }

        return (_tenantContext.TenantId.Value, _siteContext.SiteId.Value);
    }
}
