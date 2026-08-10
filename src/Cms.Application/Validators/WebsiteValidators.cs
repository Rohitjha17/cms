using Cms.Application.DTOs.Websites;
using Cms.Shared.Helpers;
using FluentValidation;

namespace Cms.Application.Validators;

public sealed class ProvisionWebsiteValidator : AbstractValidator<ProvisionWebsiteDto>
{
    public ProvisionWebsiteValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.SiteKey).NotEmpty().MaximumLength(50).Matches("^[a-z0-9_-]+$");
        RuleFor(x => x.WebsiteType).IsInEnum();
        RuleFor(x => x.HomeVariant).IsInEnum();
        RuleFor(x => x.DomainName).MaximumLength(255)
            .Matches(@"^(localhost|127\.0\.0\.1|(?=.{1,253}$)([a-z0-9](?:[a-z0-9-]{0,61}[a-z0-9])?\.)+[a-z]{2,})$")
            .When(x => !string.IsNullOrWhiteSpace(x.DomainName));
        RuleFor(x => x.LogoUrl).Must(url => UrlHelper.IsValidUrl(url))
            .When(x => !string.IsNullOrWhiteSpace(x.LogoUrl));
        RuleFor(x => x.PrimaryColor).MaximumLength(20);
        RuleFor(x => x.SecondaryColor).MaximumLength(20);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.TemplateKeys).NotEmpty()
            .WithMessage("Select at least one page template from the gallery.");
    }
}

public sealed class SiteBrandingValidator : AbstractValidator<SiteBrandingDto>
{
    public SiteBrandingValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Tagline).MaximumLength(250);
        RuleFor(x => x.PrimaryColor).MaximumLength(20);
        RuleFor(x => x.SecondaryColor).MaximumLength(20);
        RuleFor(x => x.FooterText).MaximumLength(1000);
        RuleFor(x => x.Address).MaximumLength(500);
        RuleFor(x => x.Phone).MaximumLength(50);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.LogoUrl).Must(url => UrlHelper.IsValidUrl(url))
            .When(x => !string.IsNullOrWhiteSpace(x.LogoUrl));
        RuleFor(x => x.FaviconUrl).Must(url => UrlHelper.IsValidUrl(url))
            .When(x => !string.IsNullOrWhiteSpace(x.FaviconUrl));
        RuleFor(x => x.HeaderImageUrl).Must(url => UrlHelper.IsValidUrl(url))
            .When(x => !string.IsNullOrWhiteSpace(x.HeaderImageUrl));
        RuleFor(x => x.MapEmbedUrl).Must(url => UrlHelper.IsValidUrl(url))
            .When(x => !string.IsNullOrWhiteSpace(x.MapEmbedUrl));
        RuleFor(x => x.HomeVariant).IsInEnum();
        RuleFor(x => x.WebsiteType).IsInEnum();
    }
}

public sealed class SubmitContactValidator : AbstractValidator<SubmitContactDto>
{
    public SubmitContactValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.Phone).MaximumLength(50);
        RuleFor(x => x.Subject).MaximumLength(250);
        RuleFor(x => x.Message).NotEmpty().MaximumLength(4000);
    }
}

public sealed class SavePageTemplateValidator : AbstractValidator<SavePageTemplateDto>
{
    public SavePageTemplateValidator()
    {
        RuleFor(x => x.TemplateKey).NotEmpty().MaximumLength(100).Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DefaultSlug).NotEmpty().MaximumLength(250).Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$");
        RuleFor(x => x.PageType).IsInEnum();
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
        RuleFor(x => x.DefaultJsonData).Must(IsValidJsonObject)
            .When(x => !string.IsNullOrWhiteSpace(x.DefaultJsonData))
            .WithMessage("Default JSON must be a valid JSON object.");
    }

    private static bool IsValidJsonObject(string? value)
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

public sealed class AssignTemplatesValidator : AbstractValidator<AssignTemplatesDto>
{
    public AssignTemplatesValidator()
    {
        RuleFor(x => x.TemplateKeys).NotEmpty();
    }
}

public sealed class SaveSiteDomainValidator : AbstractValidator<SaveSiteDomainDto>
{
    public SaveSiteDomainValidator()
    {
        RuleFor(x => x.DomainName).NotEmpty().MaximumLength(255)
            .Matches(@"^(localhost|127\.0\.0\.1|(?=.{1,253}$)([a-z0-9](?:[a-z0-9-]{0,61}[a-z0-9])?\.)+[a-z]{2,})$")
            .WithMessage("Enter a host name such as school.example.edu (no https:// and no path).");
    }
}
