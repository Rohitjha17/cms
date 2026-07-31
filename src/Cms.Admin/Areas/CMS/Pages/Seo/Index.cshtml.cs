using Cms.Application.DTOs.Content;
using Cms.Application.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Cms.Admin.Areas.CMS.Pages.Seo;

public sealed class IndexModel : PageModel
{
    private readonly ISiteContentService _service;
    private readonly IValidator<SeoSettingDto> _validator;

    public IndexModel(ISiteContentService service, IValidator<SeoSettingDto> validator)
    {
        _service = service;
        _validator = validator;
    }

    [BindProperty] public SeoSettingDto Input { get; set; } = new();
    [TempData] public string? StatusMessage { get; set; }

    public async Task OnGetAsync(CancellationToken cancellationToken) =>
        Input = await _service.GetSeoAsync(cancellationToken);

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
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

        await _service.SaveSeoAsync(Input, cancellationToken);
        StatusMessage = "SEO settings saved.";
        return RedirectToPage();
    }
}
