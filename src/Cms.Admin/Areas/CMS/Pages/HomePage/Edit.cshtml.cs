using Cms.Application.DTOs.HomePage;
using Cms.Application.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Cms.Admin.Areas.CMS.Pages.HomePage;

public class EditModel : PageModel
{
    private readonly IHomePageService _homePageService;
    private readonly IMediaService _mediaService;
    private readonly IValidator<UpdateHomePageSectionDto> _validator;

    public EditModel(
        IHomePageService homePageService,
        IMediaService mediaService,
        IValidator<UpdateHomePageSectionDto> validator)
    {
        _homePageService = homePageService;
        _mediaService = mediaService;
        _validator = validator;
    }

    [BindProperty(SupportsGet = true)]
    public string SectionKey { get; set; } = string.Empty;

    [BindProperty]
    public UpdateHomePageSectionDto Input { get; set; } = new();

    [BindProperty]
    public bool IsActiveChecked { get; set; }

    public DateTime? LastUpdated { get; private set; }
    public DateTime CreatedDate { get; private set; }

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var section = await _homePageService.GetSectionAsync(SectionKey, includeInactive: true, cancellationToken);
        Input = new UpdateHomePageSectionDto
        {
            Title = section.Title,
            SubTitle = section.SubTitle,
            Description = section.Description,
            ButtonText = section.ButtonText,
            ButtonLink = section.ButtonLink,
            ImageUrl = section.ImageUrl,
            BackgroundImageUrl = section.BackgroundImageUrl,
            JsonData = section.JsonData,
            DisplayOrder = section.DisplayOrder,
            IsActive = section.IsActive
        };
        IsActiveChecked = section.IsActive;
        LastUpdated = section.UpdatedDate;
        CreatedDate = section.CreatedDate;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(IFormFile? imageFile, IFormFile? backgroundFile, CancellationToken cancellationToken)
    {
        Input.IsActive = IsActiveChecked;

        if (imageFile is { Length: > 0 })
        {
            var uploaded = await _mediaService.UploadImageAsync(imageFile, "homepage", cancellationToken);
            Input.ImageUrl = uploaded.Url;
        }

        if (backgroundFile is { Length: > 0 })
        {
            var uploaded = await _mediaService.UploadImageAsync(backgroundFile, "homepage", cancellationToken);
            Input.BackgroundImageUrl = uploaded.Url;
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

        await _homePageService.UpdateSectionAsync(SectionKey, Input, cancellationToken);
        StatusMessage = "Section saved successfully.";
        return RedirectToPage(new { sectionKey = SectionKey });
    }

    public async Task<IActionResult> OnPostDeleteAsync(CancellationToken cancellationToken)
    {
        await _homePageService.DeleteSectionAsync(SectionKey, hardDelete: false, cancellationToken);
        TempData["StatusMessage"] = $"Section '{SectionKey}' has been hidden.";
        return RedirectToPage("Index");
    }
}
