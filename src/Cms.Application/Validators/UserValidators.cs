using Cms.Application.DTOs.Users;
using Cms.Domain.Constants;
using FluentValidation;

namespace Cms.Application.Validators;

/// <summary>
/// Password rules mirror the Identity options configured in AddInfrastructure so the
/// user sees one consistent message instead of an Identity error after the fact.
/// </summary>
internal static class PasswordRules
{
    internal const int MinimumLength = 8;

    internal static IRuleBuilderOptions<T, string> Password<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().WithMessage("Password is required.")
            .MinimumLength(MinimumLength)
                .WithMessage($"Password must be at least {MinimumLength} characters long.")
            .MaximumLength(128)
                .WithMessage("Password must be 128 characters or fewer.")
            .Matches("[A-Z]").WithMessage("Password must contain an uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain a lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain a digit.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain a special character.");
}

public sealed class SaveUserValidator : AbstractValidator<SaveUserDto>
{
    public SaveUserValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.FullName).MaximumLength(200);
        RuleFor(x => x.Role)
            .NotEmpty()
            .Must(role => AppRoles.All.Contains(role))
            .WithMessage("Select a valid role.");
        RuleFor(x => x.Password!)
            .Password()
            .When(x => !string.IsNullOrEmpty(x.Password));
    }
}

public sealed class UpdateUserValidator : AbstractValidator<UpdateUserDto>
{
    public UpdateUserValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.FullName).MaximumLength(200);
        RuleFor(x => x.Role)
            .NotEmpty()
            .Must(role => AppRoles.All.Contains(role))
            .WithMessage("Select a valid role.");
    }
}

public sealed class ForgotPasswordValidator : AbstractValidator<ForgotPasswordDto>
{
    public ForgotPasswordValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
    }
}

public sealed class ResetPasswordValidator : AbstractValidator<ResetPasswordDto>
{
    public ResetPasswordValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.Token).NotEmpty().WithMessage("This reset link is incomplete or has expired.");
        RuleFor(x => x.NewPassword).Password();
        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.NewPassword)
            .WithMessage("The two passwords do not match.");
    }
}

public sealed class ChangePasswordValidator : AbstractValidator<ChangePasswordDto>
{
    public ChangePasswordValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.CurrentPassword).NotEmpty().WithMessage("Enter your current password.");
        RuleFor(x => x.NewPassword).Password();
        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.NewPassword)
            .WithMessage("The two passwords do not match.");
    }
}
