using Cms.Application.DTOs.Websites;
using Cms.Application.Interfaces;
using Cms.Shared.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;

namespace Cms.Web.Pages;

[EnableRateLimiting("public-forms")]
public sealed class ContentModel : PageModel
{
    private readonly IWebsiteService _websiteService;
    private readonly ISchoolContentService _schoolContent;
    private readonly IValidator<SubmitContactDto> _contactValidator;

    public ContentModel(
        IWebsiteService websiteService,
        ISchoolContentService schoolContent,
        IValidator<SubmitContactDto> contactValidator)
    {
        _websiteService = websiteService;
        _schoolContent = schoolContent;
        _contactValidator = contactValidator;
    }

    /// <summary>
    /// The school's own settings, for the enquiry types the contact form offers. Read here so a
    /// settings record that cannot be loaded leaves the form usable rather than throwing.
    /// </summary>
    public Cms.Application.DTOs.SchoolContent.SiteSettingsDto Settings { get; private set; } = new();

    public PublicWebsiteDto Website { get; private set; } = new();
    public PublicPageDto ContentPage { get; private set; } = new();

    [BindProperty]
    public SubmitContactDto ContactInput { get; set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(string slug, CancellationToken cancellationToken)
    {
        try
        {
            Website = await _websiteService.GetPublicWebsiteAsync(cancellationToken);
            ContentPage = await _websiteService.GetPublicPageAsync(slug, cancellationToken);
            Settings = await LoadSettingsAsync(cancellationToken);
        }
        catch (NotFoundException)
        {
            return NotFound();
        }

        ViewData["Website"] = Website;
        ViewData["Title"] = ContentPage.MetaTitle ?? ContentPage.Title;
        return Page();
    }

    private async Task<Cms.Application.DTOs.SchoolContent.SiteSettingsDto> LoadSettingsAsync(
        CancellationToken cancellationToken)
    {
        try { return await _schoolContent.GetSettingsAsync(cancellationToken); }
        catch { return new Cms.Application.DTOs.SchoolContent.SiteSettingsDto(); }
    }

    public async Task<IActionResult> OnPostContactAsync(string slug, CancellationToken cancellationToken)
    {
        Website = await _websiteService.GetPublicWebsiteAsync(cancellationToken);
        ContentPage = await _websiteService.GetPublicPageAsync(slug, cancellationToken);
        Settings = await LoadSettingsAsync(cancellationToken);
        ViewData["Website"] = Website;
        ViewData["Title"] = ContentPage.Title;

        var validation = await _contactValidator.ValidateAsync(ContactInput, cancellationToken);
        if (!validation.IsValid)
        {
            foreach (var error in validation.Errors)
            {
                ModelState.AddModelError($"ContactInput.{error.PropertyName}", error.ErrorMessage);
            }

            return Page();
        }

        await _websiteService.SubmitContactAsync(ContactInput, cancellationToken);
        StatusMessage = "Thank you. Your message has been sent.";
        return RedirectToPage(new { slug });
    }
}
