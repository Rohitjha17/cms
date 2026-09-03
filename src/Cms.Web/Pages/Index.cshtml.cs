using Cms.Application.DTOs.SchoolContent;
using Cms.Application.DTOs.Websites;
using Cms.Application.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.OutputCaching;

namespace Cms.Web.Pages;

[OutputCache(PolicyName = "public-pages")]
public sealed class IndexModel : PageModel
{
    private readonly IWebsiteService _websiteService;
    private readonly ISchoolContentService _schoolContent;

    public IndexModel(IWebsiteService websiteService, ISchoolContentService schoolContent)
    {
        _websiteService = websiteService;
        _schoolContent = schoolContent;
    }

    public PublicWebsiteDto Website { get; private set; } = new();

    /// <summary>
    /// The notices and events the school actually maintains. The home page sections for these
    /// used to show a list typed into the section's own configuration, so a notice added under
    /// News and notices never appeared on the home page — it only existed on /news.
    /// </summary>
    public IReadOnlyList<NewsArticleDto> News { get; private set; } = [];

    public IReadOnlyList<SchoolEventDto> Events { get; private set; } = [];

    /// <summary>
    /// The school's appearance choices. Read here rather than in the view so a settings record
    /// that cannot be loaded leaves the page with defaults instead of an exception.
    /// </summary>
    public SiteSettingsDto Settings { get; private set; } = new();

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Website = await _websiteService.GetPublicWebsiteAsync(cancellationToken);
        News = await _schoolContent.GetNewsAsync(includeUnpublished: false, cancellationToken);

        try { Settings = await _schoolContent.GetSettingsAsync(cancellationToken); }
        catch { Settings = new SiteSettingsDto(); }

        var events = await _schoolContent.GetEventsAsync(includeUnpublished: false, cancellationToken);
        var now = DateTime.UtcNow;
        Events = events.Where(e => !e.HasFinished(now)).Concat(events.Where(e => e.HasFinished(now))).ToList();

        ViewData["Website"] = Website;
        ViewData["News"] = News;
        ViewData["Events"] = Events;
        ViewData["Title"] = Website.Branding.Name;
    }
}
