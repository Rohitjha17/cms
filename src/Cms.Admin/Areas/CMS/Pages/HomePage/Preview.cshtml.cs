using Cms.Application.DTOs.HomePage;
using Cms.Application.DTOs.Websites;
using Cms.Application.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace Cms.Admin.Areas.CMS.Pages.HomePage;

public class PreviewModel : PageModel
{
    private readonly IHomePageService _homePageService;
    private readonly IWebsiteService _websiteService;
    private readonly ITenantContext _tenantContext;
    private readonly ISiteContext _siteContext;
    private readonly IConfiguration _configuration;

    public PreviewModel(
        IHomePageService homePageService,
        IWebsiteService websiteService,
        ITenantContext tenantContext,
        ISiteContext siteContext,
        IConfiguration configuration)
    {
        _homePageService = homePageService;
        _websiteService = websiteService;
        _tenantContext = tenantContext;
        _siteContext = siteContext;
        _configuration = configuration;
    }

    public IReadOnlyList<HomePageSectionDto> Sections { get; private set; } = [];
    public HomePageSectionDto? SingleSection { get; private set; }

    /// <summary>Branding, navigation and SEO exactly as the public website receives them.</summary>
    public PublicWebsiteDto Website { get; private set; } = new();

    public string TenantName => _tenantContext.TenantName ?? "Your institution";
    public string SiteName => _siteContext.SiteName ?? "Website";

    /// <summary>Absolute URL of the real published site, when the deployment declares one.</summary>
    public string? LiveSiteUrl { get; private set; }

    public async Task OnGetAsync(string? sectionKey, CancellationToken cancellationToken)
    {
        Website = await _websiteService.GetPublicWebsiteAsync(cancellationToken);

        var publicBaseUrl = _configuration["PublicSite:BaseUrl"]?.TrimEnd('/');
        if (!string.IsNullOrWhiteSpace(publicBaseUrl))
        {
            LiveSiteUrl = publicBaseUrl + (Website.BasePath.Length == 0 ? "/" : Website.BasePath);
        }

        if (!string.IsNullOrWhiteSpace(sectionKey))
        {
            SingleSection = await _homePageService.GetSectionAsync(sectionKey, includeInactive: true, cancellationToken);
            return;
        }

        Sections = await _homePageService.GetSectionsAsync(includeInactive: false, cancellationToken);
    }

    public string? ConfigString(HomePageSectionDto section, string property)
    {
        if (string.IsNullOrWhiteSpace(section.JsonData))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(section.JsonData);
            return document.RootElement.TryGetProperty(property, out var value)
                ? value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString()
                : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public int ConfigNumber(HomePageSectionDto section, string property) =>
        int.TryParse(ConfigString(section, property), out var value) ? value : 0;

    public IReadOnlyList<IReadOnlyDictionary<string, string>> ConfigItems(HomePageSectionDto section)
    {
        if (string.IsNullOrWhiteSpace(section.JsonData))
        {
            return [];
        }

        try
        {
            using var document = JsonDocument.Parse(section.JsonData);
            if (!document.RootElement.TryGetProperty("items", out var items)
                || items.ValueKind != JsonValueKind.Array)
            {
                return [];
            }

            return items.EnumerateArray()
                .Where(x => x.ValueKind == JsonValueKind.Object)
                .Select(item => (IReadOnlyDictionary<string, string>)item.EnumerateObject()
                    .ToDictionary(
                        property => property.Name,
                        property => property.Value.ValueKind == JsonValueKind.String
                            ? property.Value.GetString() ?? string.Empty
                            : property.Value.ToString(),
                        StringComparer.OrdinalIgnoreCase))
                .ToList();
        }
        catch (JsonException)
        {
            return [];
        }
    }

    public string ItemValue(IReadOnlyDictionary<string, string> item, string key) =>
        item.TryGetValue(key, out var value) ? value : string.Empty;
}
