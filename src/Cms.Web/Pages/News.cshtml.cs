using Cms.Application.DTOs.SchoolContent;
using Cms.Application.DTOs.Websites;
using Cms.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.OutputCaching;

namespace Cms.Web.Pages;

[OutputCache(PolicyName = "public-pages")]
public sealed class NewsModel : PageModel
{
    private readonly IWebsiteService _websiteService;
    private readonly ISchoolContentService _schoolContent;

    public NewsModel(IWebsiteService websiteService, ISchoolContentService schoolContent)
    {
        _websiteService = websiteService;
        _schoolContent = schoolContent;
    }

    public PublicWebsiteDto Website { get; private set; } = new();
    public IReadOnlyList<NewsArticleDto> Articles { get; private set; } = [];

    /// <summary>A single article when the URL names one, otherwise the listing.</summary>
    public NewsArticleDto? Article { get; private set; }

    public async Task<IActionResult> OnGetAsync(string? key, CancellationToken cancellationToken)
    {
        Website = await _websiteService.GetPublicWebsiteAsync(cancellationToken);
        ViewData["Website"] = Website;

        if (!string.IsNullOrWhiteSpace(key))
        {
            Article = await _schoolContent.GetNewsArticleByKeyAsync(key, cancellationToken);
            if (Article is null)
            {
                return NotFound();
            }

            ViewData["Title"] = Article.Headline;
            return Page();
        }

        Articles = await _schoolContent.GetNewsAsync(includeUnpublished: false, cancellationToken);
        ViewData["Title"] = "News and notices";
        return Page();
    }
}
