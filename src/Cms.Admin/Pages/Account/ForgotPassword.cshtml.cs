using Cms.Application.DTOs.Users;
using Cms.Application.Interfaces;
using Cms.Shared.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;

namespace Cms.Admin.Pages.Account;

[EnableRateLimiting("auth")]
public sealed class ForgotPasswordModel : PageModel
{
    private readonly IUserManagementService _service;

    public ForgotPasswordModel(IUserManagementService service)
    {
        _service = service;
    }

    [BindProperty] public ForgotPasswordDto Input { get; set; } = new();

    public bool Submitted { get; private set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _service.RequestPasswordResetAsync(Input, BuildResetLink, cancellationToken);
        }
        catch (FluentValidation.ValidationException ex)
        {
            foreach (var error in ex.Errors)
            {
                ModelState.AddModelError(string.Empty, error.ErrorMessage);
            }

            return Page();
        }
        catch (AppException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }

        // Deliberately identical whether or not the address matched an account.
        Submitted = true;
        return Page();
    }

    private string BuildResetLink(string email, string token) =>
        Url.Page(
            "/Account/ResetPassword",
            pageHandler: null,
            values: new { email, token },
            protocol: Request.Scheme)
        ?? $"{Request.Scheme}://{Request.Host}/Account/ResetPassword";
}
