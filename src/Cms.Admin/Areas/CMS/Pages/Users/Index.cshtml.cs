using Cms.Application.DTOs.Tenancy;
using Cms.Application.DTOs.Users;
using Cms.Application.Interfaces;
using Cms.Domain.Constants;
using Cms.Shared.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Cms.Admin.Areas.CMS.Pages.Users;

/// <summary>
/// Account administration for the current institution. Tenant scoping and role limits are
/// enforced by <see cref="IUserManagementService"/>; this page only shapes the workspace.
/// </summary>
public sealed class IndexModel : PageModel
{
    private readonly IUserManagementService _service;
    private readonly ITenantManagementService _tenantService;

    public IndexModel(IUserManagementService service, ITenantManagementService tenantService)
    {
        _service = service;
        _tenantService = tenantService;
    }

    public IReadOnlyList<CmsUserDto> Users { get; private set; } = [];
    public IReadOnlyList<string> AssignableRoles { get; private set; } = [];
    public IReadOnlyList<TenantManagementDto> Tenants { get; private set; } = [];
    public bool IsSuperAdmin => User.IsInRole(AppRoles.SuperAdmin);
    public bool CanManage => IsSuperAdmin || User.IsInRole(AppRoles.TenantAdmin);

    [BindProperty] public SaveUserDto Input { get; set; } = new();
    [BindProperty] public UpdateUserDto EditInput { get; set; } = new();
    [BindProperty] public string? EditId { get; set; }

    [TempData] public string? StatusMessage { get; set; }

    /// <summary>Surfaced after creating an account when no SMTP transport is configured.</summary>
    [TempData] public string? GeneratedResetLink { get; set; }

    [TempData] public string? GeneratedResetLinkEmail { get; set; }

    public async Task<IActionResult> OnGetAsync(string? edit, CancellationToken cancellationToken)
    {
        if (!CanManage)
        {
            return Forbid();
        }

        await LoadAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(edit))
        {
            var user = Users.FirstOrDefault(x => x.Id == edit);
            if (user is null)
            {
                return NotFound();
            }

            EditId = user.Id;
            EditInput = new UpdateUserDto
            {
                FullName = user.FullName,
                Role = user.Role,
                IsActive = user.IsActive
            };
        }

        return Page();
    }

    public async Task<IActionResult> OnPostCreateAsync(CancellationToken cancellationToken)
    {
        if (!CanManage)
        {
            return Forbid();
        }

        try
        {
            var result = await _service.CreateUserAsync(Input, BuildResetLink, cancellationToken);
            if (result.InvitationEmailSent)
            {
                StatusMessage =
                    $"Account created for {result.User.Email}. An invitation email has been sent.";
            }
            else
            {
                StatusMessage =
                    $"Account created for {result.User.Email}. "
                    + "Email is not configured, so share the one-time link below with them.";
                GeneratedResetLink = BuildResetLink(result.User.Email, result.PasswordResetToken);
                GeneratedResetLinkEmail = result.User.Email;
            }

            return RedirectToPage();
        }
        catch (AppException ex)
        {
            AddErrors(ex);
            await LoadAsync(cancellationToken);
            return Page();
        }
    }

    public async Task<IActionResult> OnPostUpdateAsync(CancellationToken cancellationToken)
    {
        if (!CanManage)
        {
            return Forbid();
        }

        if (string.IsNullOrWhiteSpace(EditId))
        {
            return NotFound();
        }

        try
        {
            var updated = await _service.UpdateUserAsync(EditId, EditInput, cancellationToken);
            StatusMessage = $"{updated.Email} updated.";
            return RedirectToPage();
        }
        catch (AppException ex)
        {
            AddErrors(ex);
            await LoadAsync(cancellationToken);
            return Page();
        }
    }

    public async Task<IActionResult> OnPostStatusAsync(
        string id, bool isActive, CancellationToken cancellationToken)
    {
        if (!CanManage)
        {
            return Forbid();
        }

        try
        {
            await _service.SetStatusAsync(id, isActive, cancellationToken);
            StatusMessage = isActive ? "Account activated." : "Account deactivated.";
        }
        catch (AppException ex)
        {
            StatusMessage = ex.Message;
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUnlockAsync(string id, CancellationToken cancellationToken)
    {
        if (!CanManage)
        {
            return Forbid();
        }

        try
        {
            await _service.UnlockAsync(id, cancellationToken);
            StatusMessage = "Account unlocked.";
        }
        catch (AppException ex)
        {
            StatusMessage = ex.Message;
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostResetLinkAsync(string id, CancellationToken cancellationToken)
    {
        if (!CanManage)
        {
            return Forbid();
        }

        try
        {
            var token = await _service.CreatePasswordResetTokenAsync(id, cancellationToken);
            GeneratedResetLink = BuildResetLink(token.Email, token.Token);
            GeneratedResetLinkEmail = token.Email;
            StatusMessage = $"One-time reset link generated for {token.Email}.";
        }
        catch (AppException ex)
        {
            StatusMessage = ex.Message;
        }

        return RedirectToPage();
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        Users = await _service.GetUsersAsync(cancellationToken);
        AssignableRoles = _service.GetAssignableRoles();
        if (IsSuperAdmin)
        {
            Tenants = await _tenantService.GetAllAsync(cancellationToken);
        }

        if (string.IsNullOrWhiteSpace(Input.Role) && AssignableRoles.Count > 0)
        {
            Input.Role = AssignableRoles.Contains(AppRoles.Editor) ? AppRoles.Editor : AssignableRoles[0];
        }
    }

    private void AddErrors(AppException exception)
    {
        if (exception is ValidationAppException validation && validation.Errors.Count > 0)
        {
            foreach (var error in validation.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }

            return;
        }

        ModelState.AddModelError(string.Empty, exception.Message);
    }

    private string BuildResetLink(string email, string token) =>
        Url.Page(
            "/Account/ResetPassword",
            pageHandler: null,
            values: new { area = "", email, token },
            protocol: Request.Scheme)
        ?? $"{Request.Scheme}://{Request.Host}/Account/ResetPassword";
}
