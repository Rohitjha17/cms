using Cms.Application.DTOs.Websites;
using Cms.Application.Interfaces;
using Cms.Domain.Constants;
using Cms.Shared.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Cms.Admin.Areas.CMS.Pages.Domains;

public sealed class IndexModel : PageModel
{
    private readonly IWebsiteService _service;
    private readonly IConfiguration _configuration;

    public IndexModel(IWebsiteService service, IConfiguration configuration)
    {
        _service = service;
        _configuration = configuration;
    }

    /// <summary>CNAME target for subdomains, supplied by the deployment.</summary>
    public string? DeploymentHost => Blank(_configuration["Platform:DeploymentHost"]);

    /// <summary>A record target for apex domains, supplied by the deployment.</summary>
    public string? ServerIp => Blank(_configuration["Platform:ServerIp"]);

    /// <summary>The host this admin request arrived on, so the operator can see where they are.</summary>
    public string CurrentHost => Request.Host.Host;

    /// <summary>Websites with no live host are invisible to the public, so they are called out.</summary>
    public IReadOnlyList<WebsiteSummaryDto> UnreachableWebsites { get; private set; } = [];

    private static string? Blank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    [BindProperty] public Guid? EditId { get; set; }
    [BindProperty] public SaveSiteDomainDto Input { get; set; } = new();

    public IReadOnlyList<SiteDomainDto> Domains { get; private set; } = [];
    public IReadOnlyList<WebsiteSummaryDto> Websites { get; private set; } = [];
    public bool CanManage => User.IsInRole(AppRoles.SuperAdmin) || User.IsInRole(AppRoles.TenantAdmin);

    [TempData] public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid? edit, CancellationToken cancellationToken)
    {
        await LoadAsync(cancellationToken);

        if (edit.HasValue)
        {
            var domain = Domains.FirstOrDefault(x => x.Id == edit.Value);
            if (domain is null)
            {
                return NotFound();
            }

            EditId = domain.Id;
            Input = new SaveSiteDomainDto
            {
                DomainName = domain.DomainName,
                SiteId = domain.SiteId,
                IsPrimary = domain.IsPrimary,
                IsActive = domain.IsActive
            };
        }

        return Page();
    }

    public async Task<IActionResult> OnPostSaveAsync(CancellationToken cancellationToken)
    {
        if (!CanManage)
        {
            return Forbid();
        }

        try
        {
            await _service.SaveDomainAsync(EditId, Input, cancellationToken);
        }
        catch (ValidationException exception)
        {
            foreach (var error in exception.Errors)
            {
                ModelState.AddModelError(string.Empty, error.ErrorMessage);
            }

            await LoadAsync(cancellationToken);
            return Page();
        }
        catch (ValidationAppException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            await LoadAsync(cancellationToken);
            return Page();
        }

        StatusMessage = EditId.HasValue ? "Domain updated." : "Domain added.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        if (!CanManage)
        {
            return Forbid();
        }

        await _service.DeleteDomainAsync(id, cancellationToken);
        StatusMessage = "Domain removed.";
        return RedirectToPage();
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        Domains = await _service.GetDomainsAsync(cancellationToken);
        Websites = await _service.GetWebsitesAsync(cancellationToken);

        var sharedHostExists = Domains.Any(x => x.IsShared && x.IsActive);
        UnreachableWebsites = sharedHostExists
            ? []
            : Websites.Where(site => site.IsActive
                    && !Domains.Any(d => d.IsActive && d.SiteId == site.Id))
                .ToList();
    }
}
