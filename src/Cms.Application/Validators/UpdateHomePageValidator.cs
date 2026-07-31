using Cms.Application.DTOs.HomePage;
using Cms.Shared.Helpers;
using FluentValidation;

namespace Cms.Application.Validators;

public class UpdateHomePageSectionValidator : AbstractValidator<UpdateHomePageSectionDto>
{
    public UpdateHomePageSectionValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required.")
            .MaximumLength(250);

        RuleFor(x => x.SubTitle).MaximumLength(500);
        RuleFor(x => x.ButtonText).MaximumLength(100);
        RuleFor(x => x.ButtonLink)
            .Must(url => UrlHelper.IsValidUrl(url))
            .WithMessage("Button link must be a valid URL.")
            .When(x => !string.IsNullOrWhiteSpace(x.ButtonLink));

        RuleFor(x => x.ImageUrl)
            .Must(url => UrlHelper.IsValidUrl(url))
            .WithMessage("Image URL must be a valid URL.")
            .When(x => !string.IsNullOrWhiteSpace(x.ImageUrl));

        RuleFor(x => x.BackgroundImageUrl)
            .Must(url => UrlHelper.IsValidUrl(url))
            .WithMessage("Background image URL must be a valid URL.")
            .When(x => !string.IsNullOrWhiteSpace(x.BackgroundImageUrl));

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0)
            .When(x => x.DisplayOrder.HasValue);

        RuleFor(x => x.JsonData)
            .Must(BeValidJson)
            .WithMessage("JsonData must be valid JSON.")
            .When(x => !string.IsNullOrWhiteSpace(x.JsonData));
    }

    private static bool BeValidJson(string? json)
    {
        try
        {
            System.Text.Json.JsonDocument.Parse(json!);
            return true;
        }
        catch
        {
            return false;
        }
    }
}

public class CreateHomePageSectionValidator : AbstractValidator<CreateHomePageSectionDto>
{
    public CreateHomePageSectionValidator()
    {
        RuleFor(x => x.SectionKey)
            .NotEmpty()
            .MaximumLength(100)
            .Matches("^[a-z0-9_]+$")
            .WithMessage("SectionKey must be lowercase letters, numbers, or underscores.");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required.")
            .MaximumLength(250);

        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);

        RuleFor(x => x.ButtonLink)
            .Must(url => UrlHelper.IsValidUrl(url))
            .When(x => !string.IsNullOrWhiteSpace(x.ButtonLink));

        RuleFor(x => x.ImageUrl)
            .Must(url => UrlHelper.IsValidUrl(url))
            .When(x => !string.IsNullOrWhiteSpace(x.ImageUrl));

        RuleFor(x => x.BackgroundImageUrl)
            .Must(url => UrlHelper.IsValidUrl(url))
            .When(x => !string.IsNullOrWhiteSpace(x.BackgroundImageUrl));

        RuleFor(x => x.JsonData)
            .Must(BeValidJson)
            .WithMessage("JsonData must be valid JSON.")
            .When(x => !string.IsNullOrWhiteSpace(x.JsonData));
    }

    private static bool BeValidJson(string? json)
    {
        try
        {
            System.Text.Json.JsonDocument.Parse(json!);
            return true;
        }
        catch
        {
            return false;
        }
    }
}

public class ReorderHomePageSectionsValidator : AbstractValidator<ReorderHomePageSectionsDto>
{
    public ReorderHomePageSectionsValidator()
    {
        RuleFor(x => x.Items).NotEmpty();
        RuleFor(x => x.Items)
            .Must(items => items.Select(x => x.SectionKey).Distinct(StringComparer.OrdinalIgnoreCase).Count() == items.Count)
            .WithMessage("Each section may appear only once.");
        RuleFor(x => x.Items)
            .Must(items => items.Select(x => x.DisplayOrder).Distinct().Count() == items.Count)
            .WithMessage("Each display order must be unique.");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.SectionKey).NotEmpty();
            item.RuleFor(i => i.DisplayOrder).GreaterThanOrEqualTo(0);
        });
    }
}
