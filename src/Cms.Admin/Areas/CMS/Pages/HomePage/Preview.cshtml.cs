using Cms.Application.DTOs.HomePage;
using Cms.Application.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace Cms.Admin.Areas.CMS.Pages.HomePage;

public class PreviewModel : PageModel
{
    private readonly IHomePageService _homePageService;
    private readonly ITenantContext _tenantContext;
    private readonly ISiteContext _siteContext;

    public PreviewModel(
        IHomePageService homePageService,
        ITenantContext tenantContext,
        ISiteContext siteContext)
    {
        _homePageService = homePageService;
        _tenantContext = tenantContext;
        _siteContext = siteContext;
    }

    public IReadOnlyList<HomePageSectionDto> Sections { get; private set; } = [];
    public HomePageSectionDto? SingleSection { get; private set; }
    public string TenantName => _tenantContext.TenantName ?? "Your institution";
    public string SiteName => _siteContext.SiteName ?? "Website";

    public async Task OnGetAsync(string? sectionKey, CancellationToken cancellationToken)
    {
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
