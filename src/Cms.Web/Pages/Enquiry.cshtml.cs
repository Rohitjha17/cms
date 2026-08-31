using Cms.Application.DTOs.Websites;
using Cms.Application.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;

namespace Cms.Web.Pages;

/// <summary>
/// The enquiry form that can appear in the opening popup.
///
/// The contact form lives on a page of its own and posts back to that page. The popup is on
/// every page, so it needs somewhere fixed to post to — otherwise the enquiry would only work
/// on the one page that happens to be a contact page, which is exactly where a visitor is
/// least likely to need it.
///
/// Submissions land in the same inbox as the contact form; nothing new to check.
/// </summary>
[EnableRateLimiting("public-forms")]
public sealed class EnquiryModel : PageModel
{
    private readonly IWebsiteService _websites;
    private readonly IValidator<SubmitContactDto> _validator;

    public EnquiryModel(IWebsiteService websites, IValidator<SubmitContactDto> validator)
    {
        _websites = websites;
        _validator = validator;
    }

    [BindProperty]
    public SubmitContactDto Input { get; set; } = new();

    [TempData]
    public string? EnquiryStatus { get; set; }

    // Nothing to look at: the popup is the form, and this address is only ever posted to.
    public IActionResult OnGet() => Redirect(Home());

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(Input.Subject))
        {
            Input.Subject = "Enquiry from the website";
        }

        var validation = await _validator.ValidateAsync(Input, cancellationToken);
        if (!validation.IsValid)
        {
            EnquiryStatus = string.Join(" ", validation.Errors.Select(x => x.ErrorMessage));
            return Redirect(BackTo());
        }

        await _websites.SubmitContactAsync(Input, cancellationToken);
        EnquiryStatus = "Thank you. We have your enquiry and will be in touch.";
        return Redirect(BackTo());
    }

    private string Home() => Request.PathBase.HasValue ? Request.PathBase.Value! : "/";

    /// <summary>
    /// Back to the page the visitor was reading. Only a path on this site is followed: the
    /// header is sent by the browser and would otherwise be a way to bounce visitors elsewhere.
    /// </summary>
    private string BackTo()
    {
        var referer = Request.Headers.Referer.ToString();
        if (!string.IsNullOrWhiteSpace(referer)
            && Uri.TryCreate(referer, UriKind.Absolute, out var uri)
            && string.Equals(uri.Host, Request.Host.Host, StringComparison.OrdinalIgnoreCase))
        {
            return uri.PathAndQuery;
        }

        return Home();
    }
}
