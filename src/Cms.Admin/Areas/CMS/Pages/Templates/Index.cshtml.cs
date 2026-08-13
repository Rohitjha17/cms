using Cms.Application.DTOs.Websites;
using Cms.Application.Interfaces;
using Cms.Domain.Constants;
using Cms.Shared.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Cms.Admin.Areas.CMS.Pages.Templates;

public sealed class IndexModel : PageModel
{
    private readonly IWebsiteService _service;

    public IndexModel(IWebsiteService service) => _service = service;

    [BindProperty] public ProvisionFromTemplateDto Input { get; set; } = new();

    public IReadOnlyList<SiteTemplateSummaryDto> Templates { get; private set; } = [];
    public bool CanProvision => User.IsInRole(AppRoles.SuperAdmin) || User.IsInRole(AppRoles.TenantAdmin);

    [TempData] public string? StatusMessage { get; set; }
    [TempData] public string? CreatedSiteKey { get; set; }

    /// <summary>Set when the form is reopened after a validation failure.</summary>
    public string? SelectedTemplateKey { get; private set; }

    public async Task OnGetAsync(string? use, CancellationToken cancellationToken)
    {
        Templates = await _service.GetSiteTemplatesAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(use) && Templates.Any(x => x.Key == use))
        {
            SelectedTemplateKey = use;
            Input.TemplateKey = use;
        }
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!CanProvision)
        {
            return Forbid();
        }

        try
        {
            var created = await _service.ProvisionFromTemplateAsync(Input, cancellationToken);
            StatusMessage = $"'{created.Name}' created from the template with {created.PageCount} pages and sample content.";
            CreatedSiteKey = created.SiteKey;
            return RedirectToPage();
        }
        catch (ValidationException exception)
        {
            foreach (var error in exception.Errors)
            {
                ModelState.AddModelError(string.Empty, error.ErrorMessage);
            }
        }
        catch (ValidationAppException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
        }

        Templates = await _service.GetSiteTemplatesAsync(cancellationToken);
        SelectedTemplateKey = Input.TemplateKey;
        return Page();
    }
}
