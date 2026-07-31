using Cms.Application.DTOs.HomePage;
using Cms.Application.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Cms.Admin.Areas.CMS.Pages.HomePage;

public sealed class CreateModel : PageModel
{
    private readonly IHomePageService _homePageService;
    private readonly IMediaService _mediaService;
    private readonly IValidator<CreateHomePageSectionDto> _validator;

    public CreateModel(
        IHomePageService homePageService,
        IMediaService mediaService,
        IValidator<CreateHomePageSectionDto> validator)
    {
        _homePageService = homePageService;
        _mediaService = mediaService;
        _validator = validator;
    }

    [BindProperty]
    public CreateHomePageSectionDto Input { get; set; } = new();

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        var sections = await _homePageService.GetSectionsAsync(true, cancellationToken);
        Input.DisplayOrder = sections.Count == 0 ? 1 : sections.Max(x => x.DisplayOrder) + 1;
    }

    public async Task<IActionResult> OnPostAsync(
        IFormFile? imageFile,
        IFormFile? backgroundFile,
        CancellationToken cancellationToken)
    {
        if (imageFile is { Length: > 0 })
        {
            Input.ImageUrl = (await _mediaService.UploadImageAsync(
                imageFile, "homepage", cancellationToken)).Url;
        }

        if (backgroundFile is { Length: > 0 })
        {
            Input.BackgroundImageUrl = (await _mediaService.UploadImageAsync(
                backgroundFile, "homepage", cancellationToken)).Url;
        }

        var validation = await _validator.ValidateAsync(Input, cancellationToken);
        if (!validation.IsValid)
        {
            foreach (var error in validation.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
            return Page();
        }

        await _homePageService.CreateSectionAsync(Input, cancellationToken);
        TempData["StatusMessage"] = $"Section '{Input.Title}' created successfully.";
        return RedirectToPage("Index");
    }
}
