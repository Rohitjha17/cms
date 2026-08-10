using Cms.Application.DTOs.SchoolContent;
using Cms.Shared.Helpers;
using FluentValidation;

namespace Cms.Application.Validators;

public sealed class SaveFacultyMemberValidator : AbstractValidator<SaveFacultyMemberDto>
{
    public SaveFacultyMemberValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Designation).MaximumLength(150);
        RuleFor(x => x.Department).MaximumLength(150);
        RuleFor(x => x.Qualification).MaximumLength(250);
        RuleFor(x => x.Category).IsInEnum();
        RuleFor(x => x.ExperienceYears).InclusiveBetween(0, 80)
            .When(x => x.ExperienceYears.HasValue);
        RuleFor(x => x.Email).EmailAddress().MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Phone).MaximumLength(50);
        RuleFor(x => x.Headline).MaximumLength(300);
        RuleFor(x => x.PhotoUrl).Must(url => UrlHelper.IsValidUrl(url))
            .WithMessage("Photo URL must be a valid URL.")
            .When(x => !string.IsNullOrWhiteSpace(x.PhotoUrl));
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}

public sealed class SaveNewsArticleValidator : AbstractValidator<SaveNewsArticleDto>
{
    public SaveNewsArticleValidator()
    {
        RuleFor(x => x.Headline).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Category).IsInEnum();
        RuleFor(x => x.Summary).MaximumLength(1000);
        RuleFor(x => x.ImageUrl).Must(url => UrlHelper.IsValidUrl(url))
            .WithMessage("Image URL must be a valid URL.")
            .When(x => !string.IsNullOrWhiteSpace(x.ImageUrl));
        RuleFor(x => x.AttachmentUrl).Must(url => UrlHelper.IsValidUrl(url))
            .WithMessage("Attachment URL must be a valid URL.")
            .When(x => !string.IsNullOrWhiteSpace(x.AttachmentUrl));
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}

public sealed class SaveSchoolEventValidator : AbstractValidator<SaveSchoolEventDto>
{
    public SaveSchoolEventValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Summary).MaximumLength(1000);
        RuleFor(x => x.Venue).MaximumLength(250);
        RuleFor(x => x.StartsOn).NotNull().WithMessage("A start date is required.");
        RuleFor(x => x.EndsOn).GreaterThanOrEqualTo(x => x.StartsOn!.Value)
            .WithMessage("The event cannot finish before it starts.")
            .When(x => x.EndsOn.HasValue && x.StartsOn.HasValue);
        RuleFor(x => x.ImageUrl).Must(url => UrlHelper.IsValidUrl(url))
            .WithMessage("Image URL must be a valid URL.")
            .When(x => !string.IsNullOrWhiteSpace(x.ImageUrl));
        RuleFor(x => x.RegistrationUrl).Must(url => UrlHelper.IsValidUrl(url))
            .WithMessage("Registration URL must be a valid URL.")
            .When(x => !string.IsNullOrWhiteSpace(x.RegistrationUrl));
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}

public sealed class SiteSettingsValidator : AbstractValidator<SiteSettingsDto>
{
    public SiteSettingsValidator()
    {
        RuleFor(x => x.NoticeTicker).MaximumLength(300);
        RuleFor(x => x.AdmissionStatus).IsInEnum();
        RuleFor(x => x.AdmissionsEmail).EmailAddress().MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.AdmissionsEmail));
        RuleFor(x => x.AdmissionsPhone).MaximumLength(50);
        RuleFor(x => x.OfficeHours).MaximumLength(250);
        RuleFor(x => x.WhatsAppNumber).MaximumLength(50);

        UrlRule(x => x.BrochureUrl, "Brochure URL");
        UrlRule(x => x.ApplicationUrl, "Application URL");
        UrlRule(x => x.Facebook, "Facebook URL");
        UrlRule(x => x.Instagram, "Instagram URL");
        UrlRule(x => x.YouTube, "YouTube URL");
        UrlRule(x => x.Twitter, "X/Twitter URL");
        UrlRule(x => x.LinkedIn, "LinkedIn URL");
    }

    private void UrlRule(
        System.Linq.Expressions.Expression<Func<SiteSettingsDto, string?>> selector, string label)
    {
        RuleFor(selector).Must(url => UrlHelper.IsValidUrl(url))
            .WithMessage($"{label} must be a valid URL.");
    }
}

public sealed class SaveDepartmentValidator : AbstractValidator<SaveDepartmentDto>
{
    public SaveDepartmentValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.HeadOfDepartment).MaximumLength(200);
        RuleFor(x => x.Summary).MaximumLength(1000);
        RuleFor(x => x.Email).EmailAddress().MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Phone).MaximumLength(50);
        RuleFor(x => x.ImageUrl).Must(url => UrlHelper.IsValidUrl(url))
            .WithMessage("Image URL must be a valid URL.")
            .When(x => !string.IsNullOrWhiteSpace(x.ImageUrl));
        RuleFor(x => x.Programmes).MaximumLength(4000);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}
