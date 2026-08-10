using Cms.Application.DTOs.Users;
using Cms.Application.Interfaces;
using Cms.Shared.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;

namespace Cms.Admin.Pages.Account;

[EnableRateLimiting("auth")]
public sealed class ResetPasswordModel : PageModel
{
    private readonly IUserManagementService _service;

    public ResetPasswordModel(IUserManagementService service)
    {
        _service = service;
    }

    [BindProperty] public ResetPasswordDto Input { get; set; } = new();

    public bool Completed { get; private set; }

    public IActionResult OnGet(string? email, string? token)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
        {
            return RedirectToPage("/Account/ForgotPassword");
        }

        Input.Email = email;
        Input.Token = token;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _service.ResetPasswordAsync(Input, cancellationToken);
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
            if (ex is ValidationAppException validation)
            {
                foreach (var error in validation.Errors.Where(x => x != ex.Message))
                {
                    ModelState.AddModelError(string.Empty, error);
                }
            }

            return Page();
        }

        Completed = true;
        return Page();
    }
}
