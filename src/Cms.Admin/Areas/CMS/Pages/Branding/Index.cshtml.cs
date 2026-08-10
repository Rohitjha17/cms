using Cms.Application.DTOs.Websites;
using Cms.Application.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Cms.Admin.Areas.CMS.Pages.Branding;

public sealed class IndexModel : PageModel
{
    private readonly IWebsiteService _service;
    private readonly IValidator<SiteBrandingDto> _validator;

    public IndexModel(IWebsiteService service, IValidator<SiteBrandingDto> validator)
    {
        _service = service;
        _validator = validator;
    }

    [BindProperty]
    public SiteBrandingDto Input { get; set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task OnGetAsync(CancellationToken cancellationToken) =>
        Input = await _service.GetBrandingAsync(cancellationToken);

    public async Task<IActionResult> OnPostSaveAsync(CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(Input, cancellationToken);
        if (!validation.IsValid)
        {
            foreach (var error in validation.Errors)
            {
                ModelState.AddModelError($"Input.{error.PropertyName}", error.ErrorMessage);
            }

            return Page();
        }

        Input = await _service.SaveBrandingAsync(Input, cancellationToken);
        StatusMessage = "Branding saved. Title, logo, colors, header/footer and contact details are live on the public website.";
        return RedirectToPage();
    }
}
