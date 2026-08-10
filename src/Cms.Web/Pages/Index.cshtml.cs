using Cms.Application.DTOs.Websites;
using Cms.Application.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.OutputCaching;

namespace Cms.Web.Pages;

[OutputCache(PolicyName = "public-pages")]
public sealed class IndexModel : PageModel
{
    private readonly IWebsiteService _websiteService;

    public IndexModel(IWebsiteService websiteService) => _websiteService = websiteService;

    public PublicWebsiteDto Website { get; private set; } = new();

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Website = await _websiteService.GetPublicWebsiteAsync(cancellationToken);
        ViewData["Website"] = Website;
        ViewData["Title"] = Website.Branding.Name;
    }
}
