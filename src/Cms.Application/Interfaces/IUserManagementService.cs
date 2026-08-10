using Cms.Application.DTOs.Users;

namespace Cms.Application.Interfaces;

/// <summary>
/// CMS account administration. Every method is tenant-scoped: a tenant administrator
/// may only see and modify accounts inside the tenant resolved from the request host,
/// while a platform super administrator spans tenants.
/// </summary>
public interface IUserManagementService
{
    Task<IReadOnlyList<CmsUserDto>> GetUsersAsync(CancellationToken cancellationToken);

    Task<CmsUserDto> GetUserAsync(string userId, CancellationToken cancellationToken);

    /// <summary>
    /// Creates an account and returns a URL-safe password reset token so the caller can
    /// build an activation link. Sends an invitation email when a transport is configured.
    /// </summary>
    Task<UserInviteResultDto> CreateUserAsync(
        SaveUserDto dto,
        Func<string, string, string> resetLinkFactory,
        CancellationToken cancellationToken);

    Task<CmsUserDto> UpdateUserAsync(string userId, UpdateUserDto dto, CancellationToken cancellationToken);

    Task SetStatusAsync(string userId, bool isActive, CancellationToken cancellationToken);

    /// <summary>Clears an Identity lockout after too many failed sign-in attempts.</summary>
    Task UnlockAsync(string userId, CancellationToken cancellationToken);

    /// <summary>Admin-initiated reset. Returns a URL-safe token for a one-time link.</summary>
    Task<PasswordResetTokenDto> CreatePasswordResetTokenAsync(string userId, CancellationToken cancellationToken);

    /// <summary>
    /// Self-service reset request. Always completes without revealing whether the address
    /// exists; only sends mail when the account is real, active and in the resolved tenant.
    /// </summary>
    Task RequestPasswordResetAsync(
        ForgotPasswordDto dto,
        Func<string, string, string> resetLinkFactory,
        CancellationToken cancellationToken);

    Task ResetPasswordAsync(ResetPasswordDto dto, CancellationToken cancellationToken);

    Task ChangePasswordAsync(string userId, ChangePasswordDto dto, CancellationToken cancellationToken);

    /// <summary>Roles the current caller is permitted to grant.</summary>
    IReadOnlyList<string> GetAssignableRoles();
}
