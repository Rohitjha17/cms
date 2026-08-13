using Cms.Admin.Filters;
using Cms.Application.DTOs.Websites;
using Cms.Application.Interfaces;
using Cms.Domain.Constants;
using Cms.Domain.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Cms.Admin.Areas.CMS.Pages.Websites;

public sealed class IndexModel : PageModel, IReloadablePage
{
    private readonly IWebsiteService _service;
    private readonly IValidator<ProvisionWebsiteDto> _validator;

    public IndexModel(IWebsiteService service, IValidator<ProvisionWebsiteDto> validator)
    {
        _service = service;
        _validator = validator;
    }

    public IReadOnlyList<WebsiteSummaryDto> Websites { get; private set; } = [];
    public IReadOnlyList<PageTemplateDto> Templates { get; private set; } = [];
    public bool CanProvision => User.IsInRole(AppRoles.SuperAdmin) || User.IsInRole(AppRoles.TenantAdmin);

    [BindProperty]
    public ProvisionWebsiteDto Input { get; set; } = new()
    {
        WebsiteType = WebsiteType.School,
        HomeVariant = HomeVariant.Classic,
        TemplateKeys = PageTemplateKeys.StarterPages.ToList()
    };

    [TempData]
    public string? StatusMessage { get; set; }

    /// <summary>Set right after provisioning so the page can show tailored next steps.</summary>
    [TempData]
    public string? CreatedSiteKey { get; set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        await LoadAsync(cancellationToken);
        if (Input.TemplateKeys.Count == 0)
        {
            Input.TemplateKeys = Templates.Where(t => t.IsStarter).Select(t => t.TemplateKey).ToList();
        }
    }

    public async Task<IActionResult> OnPostProvisionAsync(CancellationToken cancellationToken)
    {
        if (!CanProvision)
        {
            return Forbid();
        }

        Input.TemplateKeys = (Input.TemplateKeys ?? [])
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var validation = await _validator.ValidateAsync(Input, cancellationToken);
        if (!validation.IsValid)
        {
            foreach (var error in validation.Errors)
            {
                ModelState.AddModelError($"Input.{error.PropertyName}", error.ErrorMessage);
            }

            await LoadAsync(cancellationToken);
            return Page();
        }

        var created = await _service.ProvisionAsync(Input, cancellationToken);
        StatusMessage = $"'{created.Name}' is live in the workspace with {created.PageCount} starter page(s).";
        CreatedSiteKey = created.SiteKey;
        return RedirectToPage();
    }

    /// <summary>Refetches the lists when a failed save redisplays this page.</summary>
    public Task ReloadAsync(CancellationToken cancellationToken) => LoadAsync(cancellationToken);

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        Websites = await _service.GetWebsitesAsync(cancellationToken);
        Templates = await _service.GetPageTemplatesAsync(cancellationToken);
    }
}
