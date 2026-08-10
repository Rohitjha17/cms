using Cms.Application.DTOs.SchoolContent;
using Cms.Application.DTOs.Websites;
using Cms.Application.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.OutputCaching;

namespace Cms.Web.Pages;

[OutputCache(PolicyName = "public-pages")]
public sealed class DepartmentsModel : PageModel
{
    private readonly IWebsiteService _websiteService;
    private readonly ISchoolContentService _schoolContent;

    public DepartmentsModel(IWebsiteService websiteService, ISchoolContentService schoolContent)
    {
        _websiteService = websiteService;
        _schoolContent = schoolContent;
    }

    public PublicWebsiteDto Website { get; private set; } = new();
    public IReadOnlyList<DepartmentDto> Departments { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Website = await _websiteService.GetPublicWebsiteAsync(cancellationToken);
        Departments = await _schoolContent.GetDepartmentsAsync(includeUnpublished: false, cancellationToken);
        ViewData["Website"] = Website;
        ViewData["Title"] = "Departments";
    }
}
