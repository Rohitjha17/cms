using Cms.Admin.Filters;
using Cms.Application.DTOs.Content;
using Cms.Application.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Cms.Admin.Areas.CMS.Pages.Seo;

public sealed class IndexModel : PageModel, IReloadablePage
{
    // The lengths Google actually truncates at. Used for the counters and the audit.
    public const int TitleMin = 30;
    public const int TitleMax = 60;
    public const int DescriptionMin = 70;
    public const int DescriptionMax = 160;

    private readonly ISiteContentService _service;
    private readonly IWebsiteService _websiteService;
    private readonly IValidator<SeoSettingDto> _validator;

    public IndexModel(
        ISiteContentService service,
        IWebsiteService websiteService,
        IValidator<SeoSettingDto> validator)
    {
        _service = service;
        _websiteService = websiteService;
        _validator = validator;
    }

    [BindProperty] public SeoSettingDto Input { get; set; } = new();
    [TempData] public string? StatusMessage { get; set; }

    public string SiteName { get; private set; } = "Your website";
    public string PreviewHost { get; private set; } = "www.example.edu";
    public IReadOnlyList<PageSeoAudit> Audit { get; private set; } = [];

    public int IssueCount => Audit.Count(x => x.Severity != SeoSeverity.Ok);

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Input = await _service.GetSeoAsync(cancellationToken);
        await LoadContextAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(Input, cancellationToken);
        if (!validation.IsValid)
        {
            foreach (var error in validation.Errors)
            {
                ModelState.AddModelError($"Input.{error.PropertyName}", error.ErrorMessage);
            }

            await LoadContextAsync(cancellationToken);
            return Page();
        }

        await _service.SaveSeoAsync(Input, cancellationToken);
        StatusMessage = "SEO settings saved.";
        return RedirectToPage();
    }


    /// <summary>
    /// Refetches the lists when a save or a removal is refused. Without it the page comes back
    /// with the error beside an empty table, which reads as though the refused action destroyed
    /// everything — the opposite of what happened.
    /// </summary>
    public Task ReloadAsync(CancellationToken cancellationToken) => LoadContextAsync(cancellationToken);
    private async Task LoadContextAsync(CancellationToken cancellationToken)
    {
        try
        {
            var branding = await _websiteService.GetBrandingAsync(cancellationToken);
            SiteName = branding.Name;

            var domains = await _websiteService.GetDomainsAsync(cancellationToken);
            var primary = domains.FirstOrDefault(x => x.IsActive && x.IsPrimary)
                ?? domains.FirstOrDefault(x => x.IsActive);
            if (primary is not null)
            {
                PreviewHost = primary.DomainName;
            }
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            // Preview context is cosmetic; never block the settings form on it.
        }

        var pages = await _service.GetPagesAsync(includeInactive: false, cancellationToken);
        Audit = pages
            .Select(page => Evaluate(page, Input))
            .OrderByDescending(x => x.Severity)
            .ThenBy(x => x.Title)
            .ToList();
    }

    /// <summary>
    /// A page inherits the site defaults when it has no override, so the audit judges the
    /// value a visitor would actually see rather than the stored field alone.
    /// </summary>
    private static PageSeoAudit Evaluate(PageDto page, SeoSettingDto defaults)
    {
        var title = Coalesce(page.MetaTitle, page.Title, defaults.MetaTitle);
        var description = Coalesce(page.MetaDescription, page.Excerpt, defaults.MetaDescription);

        var problems = new List<string>();
        if (string.IsNullOrWhiteSpace(description))
        {
            problems.Add("No description");
        }
        else if (description.Length > DescriptionMax)
        {
            problems.Add($"Description {description.Length} chars (over {DescriptionMax})");
        }
        else if (description.Length < DescriptionMin)
        {
            problems.Add($"Description only {description.Length} chars");
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            problems.Add("No title");
        }
        else if (title.Length > TitleMax)
        {
            problems.Add($"Title {title.Length} chars (over {TitleMax})");
        }

        var severity = problems.Count == 0
            ? SeoSeverity.Ok
            : problems.Any(x => x.StartsWith("No ", StringComparison.Ordinal))
                ? SeoSeverity.Missing
                : SeoSeverity.Warning;

        return new PageSeoAudit
        {
            PageId = page.Id,
            Title = page.Title,
            Slug = page.Slug,
            UsesOverride = !string.IsNullOrWhiteSpace(page.MetaTitle)
                || !string.IsNullOrWhiteSpace(page.MetaDescription),
            EffectiveTitle = title,
            EffectiveDescription = description,
            Severity = severity,
            Problems = problems
        };
    }

    private static string? Coalesce(params string?[] values) =>
        values.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
}

public enum SeoSeverity
{
    Ok = 0,
    Warning = 1,
    Missing = 2
}

public sealed class PageSeoAudit
{
    public Guid PageId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public bool UsesOverride { get; init; }
    public string? EffectiveTitle { get; init; }
    public string? EffectiveDescription { get; init; }
    public SeoSeverity Severity { get; init; }
    public IReadOnlyList<string> Problems { get; init; } = [];
}
