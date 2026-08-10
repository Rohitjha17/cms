using Cms.Application.DTOs.SchoolContent;
using Cms.Application.DTOs.Websites;
using Cms.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.OutputCaching;

namespace Cms.Web.Pages;

[OutputCache(PolicyName = "public-pages")]
public sealed class EventsModel : PageModel
{
    private readonly IWebsiteService _websiteService;
    private readonly ISchoolContentService _schoolContent;

    public EventsModel(IWebsiteService websiteService, ISchoolContentService schoolContent)
    {
        _websiteService = websiteService;
        _schoolContent = schoolContent;
    }

    public PublicWebsiteDto Website { get; private set; } = new();
    public SchoolEventDto? Event { get; private set; }
    public IReadOnlyList<SchoolEventDto> Upcoming { get; private set; } = [];
    public IReadOnlyList<SchoolEventDto> Past { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(string? key, CancellationToken cancellationToken)
    {
        Website = await _websiteService.GetPublicWebsiteAsync(cancellationToken);
        ViewData["Website"] = Website;

        if (!string.IsNullOrWhiteSpace(key))
        {
            Event = await _schoolContent.GetEventByKeyAsync(key, cancellationToken);
            if (Event is null)
            {
                return NotFound();
            }

            ViewData["Title"] = Event.Title;
            return Page();
        }

        var all = await _schoolContent.GetEventsAsync(includeUnpublished: false, cancellationToken);
        var now = DateTime.UtcNow;
        Upcoming = all.Where(x => !x.HasFinished(now)).ToList();
        Past = all.Where(x => x.HasFinished(now)).Reverse().ToList();
        ViewData["Title"] = "Events";
        return Page();
    }

    public static string Schedule(SchoolEventDto item)
    {
        if (item.StartsOn is not DateTime start)
        {
            return "Date to be confirmed";
        }

        if (item.EndsOn is not DateTime end)
        {
            return start.ToString("dddd d MMMM yyyy, HH:mm");
        }

        return end.Date == start.Date
            ? $"{start:dddd d MMMM yyyy}, {start:HH:mm}–{end:HH:mm}"
            : $"{start:d MMM yyyy} – {end:d MMM yyyy}";
    }
}
