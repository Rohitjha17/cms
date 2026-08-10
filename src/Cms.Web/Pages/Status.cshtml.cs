using Cms.Application.DTOs.Websites;
using Cms.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Cms.Web.Pages;

/// <summary>
/// Public error and not-found page. Without it the production pipeline would re-execute into a
/// missing route and fall back to an empty browser error page.
/// </summary>
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class StatusModel : PageModel
{
    private readonly IWebsiteService _websiteService;

    public StatusModel(IWebsiteService websiteService) => _websiteService = websiteService;

    public string StatusLabel { get; private set; } = "Error";
    public string Heading { get; private set; } = "Something went wrong";
    public string Message { get; private set; } = "Please try again in a moment.";
    public string HomeUrl { get; private set; } = "/";

    public async Task OnGetAsync(string code, CancellationToken cancellationToken)
    {
        var isNotFound = string.Equals(code, "not-found", StringComparison.OrdinalIgnoreCase);
        StatusLabel = isNotFound ? "404" : "500";
        Heading = isNotFound ? "Page not found" : "Something went wrong";
        Message = isNotFound
            ? "The page you are looking for has been moved or no longer exists."
            : "We could not load this page. Please try again in a moment.";

        PublicWebsiteDto website;
        try
        {
            website = await _websiteService.GetPublicWebsiteAsync(cancellationToken);
        }
        catch
        {
            // The error page must render even when the website itself cannot be loaded.
            website = new PublicWebsiteDto();
        }

        HomeUrl = website.BasePath.Length == 0 ? "/" : website.BasePath;
        ViewData["Website"] = website;
        ViewData["Title"] = Heading;
    }
}
