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
        HomeVariant = HomeVariant.Prestige,
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

    /// <summary>
    /// Deleting a website is two steps: close it — gone from the public web and the console, but
    /// kept whole — then, from the closed list, delete it permanently by typing its key. A closed
    /// website can be restored until that second step.
    /// </summary>
    public Task<IActionResult> OnPostCloseAsync(Guid id, CancellationToken cancellationToken) =>
        RunAsync(() => _service.CloseWebsiteAsync(id, cancellationToken),
            "Website closed. It is off the public web and can be restored from Closed websites below.");

    public Task<IActionResult> OnPostRestoreAsync(Guid id, CancellationToken cancellationToken) =>
        RunAsync(() => _service.RestoreWebsiteAsync(id, cancellationToken), "Website restored.");

    public Task<IActionResult> OnPostDeleteAsync(Guid id, string? confirmation, CancellationToken cancellationToken) =>
        RunAsync(() => _service.DeleteWebsitePermanentlyAsync(id, confirmation ?? string.Empty, cancellationToken),
            "Website deleted permanently.");

    private async Task<IActionResult> RunAsync(Func<Task> action, string done)
    {
        if (!CanProvision)
        {
            return Forbid();
        }

        try
        {
            await action();
            StatusMessage = done;
        }
        catch (Cms.Shared.Exceptions.AppException ex)
        {
            StatusMessage = ex.Message;
        }

        return RedirectToPage();
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
