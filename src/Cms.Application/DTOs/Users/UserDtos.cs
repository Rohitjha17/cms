using Cms.Domain.Constants;

namespace Cms.Application.DTOs.Users;

/// <summary>
/// A CMS account as presented in the admin workspace. Never carries password material.
/// </summary>
public sealed class CmsUserDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string Role { get; set; } = string.Empty;
    public Guid? TenantId { get; set; }
    public string? TenantName { get; set; }
    public bool IsActive { get; set; }
    public bool IsLockedOut { get; set; }
    public DateTime CreatedDate { get; set; }
}

public sealed class SaveUserDto
{
    public string Email { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string Role { get; set; } = AppRoles.Editor;

    /// <summary>
    /// Honoured only for platform super administrators. Tenant administrators always
    /// create accounts inside their own tenant regardless of what is posted here.
    /// </summary>
    public Guid? TenantId { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Optional. When omitted a random password is generated and the account must be
    /// activated through the returned reset link (or the emailed invitation).
    /// </summary>
    public string? Password { get; set; }
}

public sealed class UpdateUserDto
{
    public string? FullName { get; set; }
    public string Role { get; set; } = AppRoles.Editor;
    public bool IsActive { get; set; } = true;
}

public sealed class SetUserStatusDto
{
    public bool IsActive { get; set; }
}

/// <summary>
/// Result of creating an account. <see cref="PasswordResetToken"/> is URL-safe and is
/// combined with the reset page address by the web layer to form a one-time link.
/// </summary>
public sealed class UserInviteResultDto
{
    public CmsUserDto User { get; set; } = new();
    public string PasswordResetToken { get; set; } = string.Empty;
    public bool InvitationEmailSent { get; set; }
}

public sealed class PasswordResetTokenDto
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}

public sealed class ForgotPasswordDto
{
    public string Email { get; set; } = string.Empty;
}

public sealed class ResetPasswordDto
{
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}

public sealed class ChangePasswordDto
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}
