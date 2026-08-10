using Cms.Application.DTOs.Tenancy;
using Cms.Application.Interfaces;
using Cms.Domain.Constants;
using Cms.Domain.Enums;
using Cms.Shared.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Cms.Admin.Areas.CMS.Pages.Tenants;

public sealed class IndexModel : PageModel
{
    private const int BlankDomainRows = 3;
    private const int BlankSiteRows = 2;

    private readonly ITenantManagementService _service;
    private readonly IValidator<SaveTenantDto> _validator;

    public IndexModel(ITenantManagementService service, IValidator<SaveTenantDto> validator)
    {
        _service = service;
        _validator = validator;
    }

    public IReadOnlyList<TenantManagementDto> Tenants { get; private set; } = [];
    [BindProperty] public Guid? EditId { get; set; }
    [BindProperty] public SaveTenantDto Input { get; set; } = new();
    [TempData] public string? StatusMessage { get; set; }

    public IReadOnlyList<string> WebsiteTypes { get; } = Enum.GetNames<WebsiteType>();

    public IReadOnlyList<string> HomeVariants { get; } = Enum.GetNames<HomeVariant>();

    /// <summary>Site keys a domain can be bound to, taken from the rows currently on screen.</summary>
    public IReadOnlyList<string> AvailableSiteKeys =>
        Input.Sites
            .Select(x => x.SiteKey?.Trim() ?? string.Empty)
            .Where(x => x.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

    public async Task<IActionResult> OnGetAsync(Guid? edit, CancellationToken cancellationToken)
    {
        if (!User.IsInRole(AppRoles.SuperAdmin))
        {
            return Forbid();
        }

        await LoadAsync(cancellationToken);
        if (edit.HasValue)
        {
            var tenant = await _service.GetAsync(edit.Value, cancellationToken);
            EditId = tenant.Id;
            Input = new SaveTenantDto
            {
                Name = tenant.Name,
                Code = tenant.Code,
                LogoUrl = tenant.LogoUrl,
                IsActive = tenant.IsActive,
                Domains = tenant.Domains.ToList(),
                Sites = tenant.Sites.ToList()
            };
        }

        PadRows();
        return Page();
    }

    public async Task<IActionResult> OnPostSaveAsync(CancellationToken cancellationToken)
    {
        if (!User.IsInRole(AppRoles.SuperAdmin))
        {
            return Forbid();
        }

        Normalize();

        var validation = await _validator.ValidateAsync(Input, cancellationToken);
        if (!validation.IsValid)
        {
            foreach (var error in validation.Errors)
            {
                ModelState.AddModelError(string.Empty, error.ErrorMessage);
            }

            await LoadAsync(cancellationToken);
            PadRows();
            return Page();
        }

        try
        {
            await _service.SaveAsync(EditId, Input, cancellationToken);
        }
        catch (AppException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await LoadAsync(cancellationToken);
            PadRows();
            return Page();
        }

        StatusMessage = EditId.HasValue ? "Tenant updated." : "Tenant created.";
        return RedirectToPage();
    }

    private async Task LoadAsync(CancellationToken cancellationToken) =>
        Tenants = await _service.GetAllAsync(cancellationToken);

    /// <summary>
    /// Drops the blank rows the form always renders and tidies the values the service
    /// and validator expect to be lower-case and trimmed.
    /// </summary>
    private void Normalize()
    {
        Input.Name = Input.Name?.Trim() ?? string.Empty;
        Input.Code = Input.Code?.Trim().ToLowerInvariant() ?? string.Empty;
        Input.LogoUrl = string.IsNullOrWhiteSpace(Input.LogoUrl) ? null : Input.LogoUrl.Trim();

        Input.Domains = Input.Domains
            .Where(x => !string.IsNullOrWhiteSpace(x.DomainName))
            .Select(x => new TenantDomainInputDto
            {
                DomainName = x.DomainName.Trim().ToLowerInvariant(),
                SiteKey = string.IsNullOrWhiteSpace(x.SiteKey) ? null : x.SiteKey.Trim().ToLowerInvariant(),
                IsPrimary = x.IsPrimary,
                IsActive = x.IsActive
            })
            .ToList();

        Input.Sites = Input.Sites
            .Where(x => !string.IsNullOrWhiteSpace(x.SiteKey) || !string.IsNullOrWhiteSpace(x.Name))
            .Select(x => new TenantSiteInputDto
            {
                Name = x.Name?.Trim() ?? string.Empty,
                SiteKey = x.SiteKey?.Trim().ToLowerInvariant() ?? string.Empty,
                WebsiteType = string.IsNullOrWhiteSpace(x.WebsiteType) ? "School" : x.WebsiteType.Trim(),
                HomeVariant = string.IsNullOrWhiteSpace(x.HomeVariant) ? "Classic" : x.HomeVariant.Trim(),
                IsDefault = x.IsDefault,
                IsActive = x.IsActive
            })
            .ToList();
    }

    /// <summary>
    /// Appends spare blank rows so several domains or sites can be added in one save
    /// without relying on JavaScript. Blank rows are discarded again by <see cref="Normalize"/>.
    /// </summary>
    private void PadRows()
    {
        var blankSites = Input.Sites.Count(x => string.IsNullOrWhiteSpace(x.SiteKey));
        for (var i = blankSites; i < BlankSiteRows; i++)
        {
            Input.Sites.Add(new TenantSiteInputDto
            {
                WebsiteType = nameof(WebsiteType.School),
                HomeVariant = nameof(HomeVariant.Classic),
                IsActive = true,
                IsDefault = Input.Sites.Count == 0
            });
        }

        var blankDomains = Input.Domains.Count(x => string.IsNullOrWhiteSpace(x.DomainName));
        for (var i = blankDomains; i < BlankDomainRows; i++)
        {
            Input.Domains.Add(new TenantDomainInputDto
            {
                IsActive = true,
                IsPrimary = Input.Domains.Count == 0
            });
        }
    }
}
