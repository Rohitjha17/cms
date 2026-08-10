using System.Security.Claims;
using Cms.Application.DTOs.Users;
using Cms.Application.Interfaces;
using Cms.Shared.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;

namespace Cms.Admin.Pages.Account;

[EnableRateLimiting("auth")]
public sealed class ChangePasswordModel : PageModel
{
    private readonly IUserManagementService _service;

    public ChangePasswordModel(IUserManagementService service)
    {
        _service = service;
    }

    [BindProperty] public ChangePasswordDto Input { get; set; } = new();

    [TempData] public string? StatusMessage { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        try
        {
            await _service.ChangePasswordAsync(userId, Input, cancellationToken);
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

        StatusMessage = "Your password has been updated.";
        return RedirectToPage();
    }
}
