using Cms.Application.DTOs.SchoolContent;
using Cms.Application.DTOs.Websites;
using Cms.Application.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.OutputCaching;

namespace Cms.Web.Pages;

[OutputCache(PolicyName = "public-pages")]
public sealed class FacultyModel : PageModel
{
    private readonly IWebsiteService _websiteService;
    private readonly ISchoolContentService _schoolContent;

    public FacultyModel(IWebsiteService websiteService, ISchoolContentService schoolContent)
    {
        _websiteService = websiteService;
        _schoolContent = schoolContent;
    }

    public PublicWebsiteDto Website { get; private set; } = new();
    public IReadOnlyList<FacultyMemberDto> Members { get; private set; } = [];

    public IEnumerable<IGrouping<FacultyCategory, FacultyMemberDto>> Groups =>
        Members.GroupBy(x => x.Category).OrderBy(x => x.Key);

    public static string GroupTitle(FacultyCategory category) => category switch
    {
        FacultyCategory.Leadership => "Leadership",
        FacultyCategory.Teaching => "Teaching faculty",
        FacultyCategory.Administration => "Administration",
        _ => "Support staff"
    };

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Website = await _websiteService.GetPublicWebsiteAsync(cancellationToken);
        Members = await _schoolContent.GetFacultyAsync(includeUnpublished: false, cancellationToken);
        ViewData["Website"] = Website;
        ViewData["Title"] = "Faculty and staff";
    }
}
