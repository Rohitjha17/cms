using Cms.Application.DTOs.Tenancy;
using Cms.Application.Interfaces;
using Cms.Domain.Constants;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Cms.Admin.Areas.CMS.Pages.Tenants;

public sealed class IndexModel : PageModel
{
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
    [BindProperty] public string DomainsText { get; set; } = string.Empty;
    [BindProperty] public string SitesText { get; set; } = string.Empty;
    [TempData] public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid? edit, CancellationToken cancellationToken)
    {
        if (!User.IsInRole(AppRoles.SuperAdmin)) return Forbid();
        await LoadAsync(cancellationToken);
        if (edit.HasValue)
        {
            var tenant = await _service.GetAsync(edit.Value, cancellationToken);
            EditId = tenant.Id;
            Input = new SaveTenantDto
            {
                Name = tenant.Name, Code = tenant.Code, LogoUrl = tenant.LogoUrl, IsActive = tenant.IsActive
            };
            DomainsText = string.Join(Environment.NewLine, tenant.Domains.Select(x =>
                $"{x.DomainName}|{x.IsPrimary.ToString().ToLowerInvariant()}|{x.IsActive.ToString().ToLowerInvariant()}"));
            SitesText = string.Join(Environment.NewLine, tenant.Sites.Select(x =>
                $"{x.Name}|{x.SiteKey}|{x.WebsiteType}|{x.IsDefault.ToString().ToLowerInvariant()}|{x.IsActive.ToString().ToLowerInvariant()}"));
        }
        return Page();
    }

    public async Task<IActionResult> OnPostSaveAsync(CancellationToken cancellationToken)
    {
        if (!User.IsInRole(AppRoles.SuperAdmin)) return Forbid();
        Input.Domains = ParseDomains(DomainsText);
        Input.Sites = ParseSites(SitesText);
        var validation = await _validator.ValidateAsync(Input, cancellationToken);
        if (!validation.IsValid)
        {
            foreach (var error in validation.Errors) ModelState.AddModelError(string.Empty, error.ErrorMessage);
            await LoadAsync(cancellationToken);
            return Page();
        }
        await _service.SaveAsync(EditId, Input, cancellationToken);
        StatusMessage = EditId.HasValue ? "Tenant updated." : "Tenant created.";
        return RedirectToPage();
    }

    private async Task LoadAsync(CancellationToken cancellationToken) =>
        Tenants = await _service.GetAllAsync(cancellationToken);

    private static List<TenantDomainInputDto> ParseDomains(string value) =>
        Lines(value).Select(parts => new TenantDomainInputDto
        {
            DomainName = parts[0],
            IsPrimary = parts.Length > 1 && bool.TryParse(parts[1], out var primary) && primary,
            IsActive = parts.Length <= 2 || !bool.TryParse(parts[2], out var active) || active
        }).ToList();

    private static List<TenantSiteInputDto> ParseSites(string value) =>
        Lines(value).Where(parts => parts.Length >= 3).Select(parts => new TenantSiteInputDto
        {
            Name = parts[0],
            SiteKey = parts[1],
            WebsiteType = parts[2],
            IsDefault = parts.Length > 3 && bool.TryParse(parts[3], out var isDefault) && isDefault,
            IsActive = parts.Length <= 4 || !bool.TryParse(parts[4], out var active) || active
        }).ToList();

    private static IEnumerable<string[]> Lines(string value) =>
        value.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(line => line.Split('|', StringSplitOptions.TrimEntries));
}
