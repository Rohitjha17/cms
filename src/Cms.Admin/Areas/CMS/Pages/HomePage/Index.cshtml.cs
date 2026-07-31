using Cms.Application.DTOs.HomePage;
using Cms.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Cms.Admin.Areas.CMS.Pages.HomePage;

public class IndexModel : PageModel
{
    private readonly IHomePageService _homePageService;
    private readonly ITenantContext _tenantContext;
    private readonly ISiteContext _siteContext;

    public IndexModel(IHomePageService homePageService, ITenantContext tenantContext, ISiteContext siteContext)
    {
        _homePageService = homePageService;
        _tenantContext = tenantContext;
        _siteContext = siteContext;
    }

    public IReadOnlyList<HomePageSectionDto> Sections { get; private set; } = [];
    public string? TenantName => _tenantContext.TenantName;
    public string? SiteName => _siteContext.SiteName;
    public int ActiveCount => Sections.Count(x => x.IsActive);
    public int ConfiguredCount => Sections.Count(IsConfigured);
    public int CompletionPercentage => Sections.Count == 0
        ? 0
        : (int)Math.Round(ConfiguredCount * 100d / Sections.Count);
    public DateTime? LastUpdated => Sections
        .Select(x => x.UpdatedDate ?? x.CreatedDate)
        .DefaultIfEmpty()
        .Max();

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Sections = await _homePageService.GetSectionsAsync(includeInactive: true, cancellationToken);
    }

    public async Task<IActionResult> OnPostToggleAsync(string sectionKey, bool isActive, CancellationToken cancellationToken)
    {
        await _homePageService.SetStatusAsync(sectionKey, isActive, cancellationToken);
        StatusMessage = $"Section '{sectionKey}' is now {(isActive ? "active" : "inactive")}.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostMoveAsync(
        string sectionKey,
        string direction,
        CancellationToken cancellationToken)
    {
        var sections = (await _homePageService.GetSectionsAsync(includeInactive: true, cancellationToken))
            .OrderBy(x => x.DisplayOrder)
            .ToList();
        var currentIndex = sections.FindIndex(x =>
            string.Equals(x.SectionKey, sectionKey, StringComparison.OrdinalIgnoreCase));
        var targetIndex = direction == "up" ? currentIndex - 1 : currentIndex + 1;

        if (currentIndex >= 0 && targetIndex >= 0 && targetIndex < sections.Count)
        {
            (sections[currentIndex], sections[targetIndex]) = (sections[targetIndex], sections[currentIndex]);
            await _homePageService.ReorderAsync(new ReorderHomePageSectionsDto
            {
                Items = sections.Select((item, index) => new ReorderItemDto
                {
                    SectionKey = item.SectionKey,
                    DisplayOrder = index + 1
                }).ToList()
            }, cancellationToken);
            StatusMessage = $"Section '{sectionKey}' moved {direction}.";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostReorderAsync(
        string orderedKeys,
        CancellationToken cancellationToken)
    {
        var keys = orderedKeys
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (keys.Count == 0)
        {
            StatusMessage = "No section order changes were submitted.";
            return RedirectToPage();
        }

        await _homePageService.ReorderAsync(new ReorderHomePageSectionsDto
        {
            Items = keys.Select((key, index) => new ReorderItemDto
            {
                SectionKey = key,
                DisplayOrder = index + 1
            }).ToList()
        }, cancellationToken);

        StatusMessage = "Homepage section order updated.";
        return RedirectToPage();
    }

    private static bool IsConfigured(HomePageSectionDto section) =>
        !string.IsNullOrWhiteSpace(section.Title)
        && (!string.IsNullOrWhiteSpace(section.Description)
            || !string.IsNullOrWhiteSpace(section.ImageUrl)
            || !string.IsNullOrWhiteSpace(section.JsonData));
}
