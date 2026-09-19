using Cms.Application.DTOs.Websites;
using Cms.Application.Interfaces;
using Cms.Shared.Exceptions;
using Cms.Web.Helpers;
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

    /// <summary>
    /// The school's own HTML for a page it has taken over, served as a document of its own for
    /// the page to show in a frame.
    ///
    /// Written into the page directly, the school's markup lived under the design's stylesheet:
    /// every heading, paragraph, table and image it contained was restyled by the template, the
    /// page's title band and padding sat around it, and a full document pasted in — with its own
    /// head, stylesheets and scripts — was broken open inside a div. In a frame it is the whole
    /// page, exactly as written, and nothing of the template's reaches it.
    ///
    /// It is served under the site's own security headers, with one restriction added: the
    /// browser sandboxes the document into an origin of its own. The school's HTML is kept
    /// whole, scripts included, so it must not run as the website — here it cannot read the
    /// website's cookies or storage, reach into the page that frames it, or reach the console.
    /// The sandbox is a response header rather than only the frame's attribute, so it holds
    /// when this address is opened directly as well.
    /// </summary>
    public async Task<IActionResult> OnGetHtmlAsync(string slug, CancellationToken cancellationToken)
    {
        PublicPageDto page;
        try
        {
            page = await _websiteService.GetPublicPageAsync(slug, cancellationToken);
        }
        catch (NotFoundException)
        {
            return NotFound();
        }

        if (!page.UseCustomHtml)
        {
            return NotFound();
        }

        var policy = Response.Headers.ContentSecurityPolicy.ToString();
        Response.Headers.ContentSecurityPolicy =
            (string.IsNullOrWhiteSpace(policy) ? string.Empty : policy.TrimEnd(' ', ';') + "; ")
            + "sandbox " + CustomHtmlDocument.SandboxFlags;
        return Content(CustomHtmlDocument.Build(page.Content, page.Title), "text/html; charset=utf-8");
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
