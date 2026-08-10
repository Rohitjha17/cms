using Cms.Application.Interfaces;
using Cms.Domain.Constants;
using Cms.Shared.Exceptions;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Cms.Admin.Pages;

public class IndexModel : PageModel
{
    private readonly IWebsiteService _websiteService;
    private readonly ISiteContentService _contentService;
    private readonly IHomePageService _homePageService;
    private readonly ISiteContext _siteContext;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(
        IWebsiteService websiteService,
        ISiteContentService contentService,
        IHomePageService homePageService,
        ISiteContext siteContext,
        ITenantContext tenantContext,
        ILogger<IndexModel> logger)
    {
        _websiteService = websiteService;
        _contentService = contentService;
        _homePageService = homePageService;
        _siteContext = siteContext;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public string SchoolName { get; private set; } = "Your website";
    public string TenantName { get; private set; } = "Workspace";
    public string HomeVariant { get; private set; } = "Classic";
    public string? Tagline { get; private set; }
    public int PageCount { get; private set; }
    public int PublishedPageCount { get; private set; }
    public int HomeSectionCount { get; private set; }
    public int UnreadContacts { get; private set; }
    public bool CanProvision => User.IsInRole(AppRoles.SuperAdmin) || User.IsInRole(AppRoles.TenantAdmin);
    public bool IsSchoolEditor => !User.IsInRole(AppRoles.SuperAdmin);

    /// <summary>No website provisioned yet for this workspace.</summary>
    public bool NeedsWebsite { get; private set; }

    /// <summary>Overview data could not be loaded; the figures below are not reliable.</summary>
    public bool LoadFailed { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        TenantName = _tenantContext.TenantName ?? "Workspace";
        try
        {
            var branding = await _websiteService.GetBrandingAsync(cancellationToken);
            SchoolName = branding.Name;
            HomeVariant = branding.HomeVariant.ToString();
            Tagline = branding.Tagline;

            var pages = await _contentService.GetPagesAsync(true, cancellationToken);
            PageCount = pages.Count;
            PublishedPageCount = pages.Count(x => x.IsActive);

            var sections = await _homePageService.GetSectionsAsync(true, cancellationToken);
            HomeSectionCount = sections.Count;

            var contacts = await _websiteService.GetContactSubmissionsAsync(cancellationToken);
            UnreadContacts = contacts.Count(x => !x.IsRead);
        }
        catch (Exception exception) when (exception is TenantNotResolvedException or NotFoundException)
        {
            // Expected before a website exists for this workspace: show the empty state.
            SchoolName = _siteContext.SiteName ?? SchoolName;
            NeedsWebsite = true;
        }
        catch (Exception exception)
        {
            // Anything else is a real failure — never present it as an empty workspace.
            _logger.LogError(exception, "Failed to load the workspace overview.");
            SchoolName = _siteContext.SiteName ?? SchoolName;
            LoadFailed = true;
        }
    }
}
