using Cms.Application.DTOs.SchoolContent;
using Cms.Application.Interfaces;
using Cms.Shared.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Cms.Admin.Areas.CMS.Pages.Settings;

public sealed class IndexModel : PageModel
{
    private readonly ISchoolContentService _service;

    public IndexModel(ISchoolContentService service) => _service = service;

    [BindProperty] public SiteSettingsDto Input { get; set; } = new();

    [TempData] public string? StatusMessage { get; set; }

    public async Task OnGetAsync(CancellationToken cancellationToken) =>
        Input = await _service.GetSettingsAsync(cancellationToken);

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _service.SaveSettingsAsync(Input, cancellationToken);
        }
        catch (ValidationException exception)
        {
            foreach (var error in exception.Errors)
            {
                ModelState.AddModelError(string.Empty, error.ErrorMessage);
            }

            return Page();
        }
        catch (ValidationAppException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return Page();
        }

        StatusMessage = "Site settings saved.";
        return RedirectToPage();
    }
}
