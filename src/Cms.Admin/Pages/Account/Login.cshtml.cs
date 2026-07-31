using System.ComponentModel.DataAnnotations;
using Cms.Application.Interfaces;
using Cms.Domain.Constants;
using Cms.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;

namespace Cms.Admin.Pages.Account;

[EnableRateLimiting("auth")]
public class LoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITenantContext _tenantContext;

    public LoginModel(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        ITenantContext tenantContext)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _tenantContext = tenantContext;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await _userManager.FindByEmailAsync(Input.Email);
        if (user is null || !user.IsActive)
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return Page();
        }

        var isSuperAdmin = await _userManager.IsInRoleAsync(user, AppRoles.SuperAdmin);
        if (!isSuperAdmin && (!_tenantContext.TenantId.HasValue || user.TenantId != _tenantContext.TenantId))
        {
            ModelState.AddModelError(string.Empty, "This account does not have access to the current institution.");
            return Page();
        }

        var result = await _signInManager.PasswordSignInAsync(user, Input.Password, true, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(
                string.Empty,
                result.IsLockedOut ? "Your account is temporarily locked. Please try again later." : "Invalid login attempt.");
            return Page();
        }

        return RedirectToPage("/HomePage/Index", new { area = "CMS" });
    }
}
