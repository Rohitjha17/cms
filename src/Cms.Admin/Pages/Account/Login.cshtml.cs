using System.ComponentModel.DataAnnotations;
using Cms.Application.Interfaces;
using Cms.Domain.Constants;
using Cms.Infrastructure.Identity;
using Cms.Infrastructure.Tenancy;
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
    private readonly ITenantHostResolver _tenants;

    public LoginModel(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        ITenantHostResolver tenants)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _tenants = tenants;
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
        // Every institution signs in at the same console address, and the account decides which
        // institution the user then works in (see ManagementTenantMiddleware). Signing in used to
        // require the address to belong to the user's own institution, so the staff of any school
        // without a console address of its own could not sign in at all. What is still required
        // is that the account belongs to an institution, and that the institution is active.
        if (!isSuperAdmin
            && (user.TenantId is not Guid ownTenant
                || await _tenants.ResolveTenantAsync(ownTenant, refresh: true) is null))
        {
            ModelState.AddModelError(string.Empty, "This account's institution is not active. Please contact the platform administrator.");
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
