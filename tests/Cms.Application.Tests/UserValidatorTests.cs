using Cms.Application.DTOs.Users;
using Cms.Application.Validators;
using Cms.Domain.Constants;

namespace Cms.Application.Tests;

public class UserValidatorTests
{
    private readonly SaveUserValidator _saveValidator = new();
    private readonly ResetPasswordValidator _resetValidator = new();
    private readonly ChangePasswordValidator _changeValidator = new();

    [Fact]
    public void SaveUser_AcceptsAnInviteWithoutAPassword()
    {
        var result = _saveValidator.Validate(new SaveUserDto
        {
            Email = "teacher@school.test",
            FullName = "A Teacher",
            Role = AppRoles.Editor
        });

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    [InlineData("missing@")]
    public void SaveUser_RejectsInvalidEmail(string email)
    {
        var result = _saveValidator.Validate(new SaveUserDto { Email = email, Role = AppRoles.Editor });
        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("Administrator")]
    [InlineData("superadmin")]
    [InlineData("")]
    public void SaveUser_RejectsRolesOutsideTheKnownSet(string role)
    {
        var result = _saveValidator.Validate(new SaveUserDto { Email = "a@b.test", Role = role });
        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("short1A!")]      // 8 chars, satisfies every class
    [InlineData("Str0ng&Password")]
    public void Password_AcceptsCompliantValues(string password)
    {
        var result = _resetValidator.Validate(new ResetPasswordDto
        {
            Email = "a@b.test",
            Token = "token",
            NewPassword = password,
            ConfirmPassword = password
        });

        Assert.True(result.IsValid, string.Join("; ", result.Errors.Select(x => x.ErrorMessage)));
    }

    [Theory]
    [InlineData("Ab1!xyz", "at least 8")]           // too short
    [InlineData("alllower1!", "uppercase")]
    [InlineData("ALLUPPER1!", "lowercase")]
    [InlineData("NoDigits!!", "digit")]
    [InlineData("NoSpecial123", "special")]
    public void Password_RejectsWeakValues(string password, string expectedFragment)
    {
        var result = _resetValidator.Validate(new ResetPasswordDto
        {
            Email = "a@b.test",
            Token = "token",
            NewPassword = password,
            ConfirmPassword = password
        });

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.ErrorMessage.Contains(expectedFragment, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ResetPassword_RequiresMatchingConfirmation()
    {
        var result = _resetValidator.Validate(new ResetPasswordDto
        {
            Email = "a@b.test",
            Token = "token",
            NewPassword = "Str0ng&Password",
            ConfirmPassword = "Different&Password1"
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.ErrorMessage.Contains("do not match"));
    }

    [Fact]
    public void ResetPassword_RequiresAToken()
    {
        var result = _resetValidator.Validate(new ResetPasswordDto
        {
            Email = "a@b.test",
            Token = "",
            NewPassword = "Str0ng&Password",
            ConfirmPassword = "Str0ng&Password"
        });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void ChangePassword_RequiresTheCurrentPassword()
    {
        var result = _changeValidator.Validate(new ChangePasswordDto
        {
            CurrentPassword = "",
            NewPassword = "Str0ng&Password",
            ConfirmPassword = "Str0ng&Password"
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.ErrorMessage.Contains("current password"));
    }

    [Fact]
    public void ChangePassword_AcceptsAValidChange()
    {
        var result = _changeValidator.Validate(new ChangePasswordDto
        {
            CurrentPassword = "Whatever@123",
            NewPassword = "Str0ng&Password",
            ConfirmPassword = "Str0ng&Password"
        });

        Assert.True(result.IsValid);
    }
}
