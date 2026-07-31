using Cms.Application.DTOs.Content;
using Cms.Application.DTOs.Tenancy;
using Cms.Shared.Helpers;
using FluentValidation;

namespace Cms.Application.Validators;

public sealed class SavePageValidator : AbstractValidator<SavePageDto>
{
    public SavePageValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(250)
            .Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$")
            .WithMessage("Slug must contain lowercase letters, numbers and single hyphens.");
        RuleFor(x => x.Excerpt).MaximumLength(500);
        RuleFor(x => x.MetaTitle).MaximumLength(250);
        RuleFor(x => x.MetaDescription).MaximumLength(500);
        RuleFor(x => x.FeaturedImageUrl).Must(url => UrlHelper.IsValidUrl(url))
            .When(x => !string.IsNullOrWhiteSpace(x.FeaturedImageUrl));
    }
}

public sealed class SaveMenuValidator : AbstractValidator<SaveMenuDto>
{
    public SaveMenuValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Location).NotEmpty().MaximumLength(50)
            .Matches("^[a-z0-9_-]+$");
        RuleFor(x => x.Items).Must(items => items.Count <= 100)
            .WithMessage("A menu cannot contain more than 100 items.");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.Label).NotEmpty().MaximumLength(150);
            item.RuleFor(x => x.Url).NotEmpty().Must(url => UrlHelper.IsValidUrl(url));
            item.RuleFor(x => x.Target).Must(x => x is null or "_self" or "_blank");
            item.RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
        });
    }
}

public sealed class SeoSettingValidator : AbstractValidator<SeoSettingDto>
{
    public SeoSettingValidator()
    {
        RuleFor(x => x.MetaTitle).MaximumLength(250);
        RuleFor(x => x.MetaDescription).MaximumLength(500);
        RuleFor(x => x.MetaKeywords).MaximumLength(500);
        RuleFor(x => x.OgImageUrl).Must(url => UrlHelper.IsValidUrl(url))
            .When(x => !string.IsNullOrWhiteSpace(x.OgImageUrl));
        RuleFor(x => x.CanonicalUrl).Must(url => UrlHelper.IsValidUrl(url))
            .When(x => !string.IsNullOrWhiteSpace(x.CanonicalUrl));
    }
}

public sealed class SaveContentEntryValidator : AbstractValidator<SaveContentEntryDto>
{
    private static readonly string[] AllowedTypes =
        ["event", "news", "person", "department", "setting", "theme"];

    public SaveContentEntryValidator()
    {
        RuleFor(x => x.ContentType).NotEmpty()
            .Must(x => AllowedTypes.Contains(x, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"Content type must be one of: {string.Join(", ", AllowedTypes)}.");
        RuleFor(x => x.Key).NotEmpty().MaximumLength(150)
            .Matches("^[a-z0-9]+(?:[-_][a-z0-9]+)*$");
        RuleFor(x => x.Title).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Summary).MaximumLength(1000);
        RuleFor(x => x.ImageUrl).Must(url => UrlHelper.IsValidUrl(url))
            .When(x => !string.IsNullOrWhiteSpace(x.ImageUrl));
        RuleFor(x => x.JsonData).Must(IsValidJson)
            .When(x => !string.IsNullOrWhiteSpace(x.JsonData))
            .WithMessage("Additional configuration must be valid JSON.");
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }

    private static bool IsValidJson(string? value)
    {
        try
        {
            using var document = System.Text.Json.JsonDocument.Parse(value!);
            return document.RootElement.ValueKind == System.Text.Json.JsonValueKind.Object;
        }
        catch
        {
            return false;
        }
    }
}

public sealed class SaveTenantValidator : AbstractValidator<SaveTenantDto>
{
    public SaveTenantValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50).Matches("^[a-z0-9-]+$");
        RuleFor(x => x.LogoUrl).Must(url => UrlHelper.IsValidUrl(url))
            .When(x => !string.IsNullOrWhiteSpace(x.LogoUrl));
        RuleFor(x => x.Domains).NotEmpty().Must(x => x.Count(d => d.IsPrimary) <= 1)
            .WithMessage("A tenant can have at most one primary domain.");
        RuleForEach(x => x.Domains).ChildRules(domain =>
        {
            domain.RuleFor(x => x.DomainName).NotEmpty().MaximumLength(255)
                .Matches(@"^(localhost|127\.0\.0\.1|(?=.{1,253}$)([a-z0-9](?:[a-z0-9-]{0,61}[a-z0-9])?\.)+[a-z]{2,})$");
        });
        RuleFor(x => x.Sites).NotEmpty().Must(x => x.Count(s => s.IsDefault) == 1)
            .WithMessage("Exactly one site must be the default.");
        RuleForEach(x => x.Sites).ChildRules(site =>
        {
            site.RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            site.RuleFor(x => x.SiteKey).NotEmpty().MaximumLength(50).Matches("^[a-z0-9_-]+$");
            site.RuleFor(x => x.WebsiteType).Must(x => x is "School" or "College");
        });
    }
}
