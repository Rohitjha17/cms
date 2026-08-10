using System.Security.Cryptography;
using System.Text;
using Cms.Application.DTOs.Users;
using Cms.Application.Interfaces;
using Cms.Domain.Constants;
using Cms.Shared.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Cms.Infrastructure.Identity;

/// <summary>
/// Account administration for the CMS.
///
/// Isolation rules enforced here (in addition to role checks at the endpoint):
///   * A tenant administrator only ever sees or mutates accounts whose TenantId equals
///     the tenant resolved from the request host, and can never grant SuperAdmin.
///   * A super administrator spans tenants but must name the tenant explicitly.
///   * Nobody can change their own role or deactivate themselves, so a workspace can
///     never be left without an administrator by a single mistaken click.
/// </summary>
public sealed class UserManagementService : IUserManagementService
{
    private const string InvitationSubject = "Your CMS account is ready";
    private const string ResetSubject = "Reset your CMS password";

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITenantManagementRepository _tenants;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserContext _currentUser;
    private readonly IEmailSender _emailSender;
    private readonly IValidator<SaveUserDto> _saveValidator;
    private readonly IValidator<UpdateUserDto> _updateValidator;
    private readonly IValidator<ForgotPasswordDto> _forgotValidator;
    private readonly IValidator<ResetPasswordDto> _resetValidator;
    private readonly IValidator<ChangePasswordDto> _changeValidator;
    private readonly ILogger<UserManagementService> _logger;

    public UserManagementService(
        UserManager<ApplicationUser> userManager,
        ITenantManagementRepository tenants,
        ITenantContext tenantContext,
        ICurrentUserContext currentUser,
        IEmailSender emailSender,
        IValidator<SaveUserDto> saveValidator,
        IValidator<UpdateUserDto> updateValidator,
        IValidator<ForgotPasswordDto> forgotValidator,
        IValidator<ResetPasswordDto> resetValidator,
        IValidator<ChangePasswordDto> changeValidator,
        ILogger<UserManagementService> logger)
    {
        _userManager = userManager;
        _tenants = tenants;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
        _emailSender = emailSender;
        _saveValidator = saveValidator;
        _updateValidator = updateValidator;
        _forgotValidator = forgotValidator;
        _resetValidator = resetValidator;
        _changeValidator = changeValidator;
        _logger = logger;
    }

    public IReadOnlyList<string> GetAssignableRoles()
    {
        if (_currentUser.IsSuperAdmin)
        {
            return AppRoles.All;
        }

        return _currentUser.IsInRole(AppRoles.TenantAdmin)
            ? new[] { AppRoles.TenantAdmin, AppRoles.Editor }
            : Array.Empty<string>();
    }

    public async Task<IReadOnlyList<CmsUserDto>> GetUsersAsync(CancellationToken cancellationToken)
    {
        EnsureCanAdminister();

        var query = _userManager.Users.AsQueryable();
        if (!_currentUser.IsSuperAdmin)
        {
            var tenantId = RequireTenant();
            query = query.Where(x => x.TenantId == tenantId);
        }

        var users = await query
            .OrderBy(x => x.Email)
            .ToListAsync(cancellationToken);

        var tenantNames = await LoadTenantNamesAsync(cancellationToken);
        var result = new List<CmsUserDto>(users.Count);
        foreach (var user in users)
        {
            // A tenant administrator must not even be able to enumerate platform staff.
            if (!_currentUser.IsSuperAdmin && await IsSuperAdminAsync(user))
            {
                continue;
            }

            result.Add(await ToDtoAsync(user, tenantNames));
        }

        return result;
    }

    public async Task<CmsUserDto> GetUserAsync(string userId, CancellationToken cancellationToken)
    {
        var user = await LoadManageableUserAsync(userId, cancellationToken);
        return await ToDtoAsync(user, await LoadTenantNamesAsync(cancellationToken));
    }

    public async Task<UserInviteResultDto> CreateUserAsync(
        SaveUserDto dto,
        Func<string, string, string> resetLinkFactory,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(resetLinkFactory);
        EnsureCanAdminister();

        dto.Email = dto.Email.Trim().ToLowerInvariant();
        dto.Role = dto.Role.Trim();
        dto.FullName = dto.FullName?.Trim();
        await _saveValidator.ValidateAndThrowAsync(dto, cancellationToken);
        EnsureRoleAssignable(dto.Role);

        var tenantId = await ResolveTargetTenantAsync(dto.Role, dto.TenantId, cancellationToken);

        if (await _userManager.FindByEmailAsync(dto.Email) is not null)
        {
            throw new ValidationAppException($"An account already exists for '{dto.Email}'.");
        }

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            // Admin-created accounts are trusted; the reset link proves mailbox ownership.
            EmailConfirmed = true,
            FullName = dto.FullName,
            TenantId = tenantId,
            IsActive = dto.IsActive,
            CreatedDate = DateTime.UtcNow
        };

        var password = string.IsNullOrEmpty(dto.Password) ? GeneratePassword() : dto.Password;
        var created = await _userManager.CreateAsync(user, password);
        if (!created.Succeeded)
        {
            throw new ValidationAppException("The account could not be created.", Describe(created));
        }

        var roleAssigned = await _userManager.AddToRoleAsync(user, dto.Role);
        if (!roleAssigned.Succeeded)
        {
            // Never leave a roleless orphan behind: it could sign in but reach nothing.
            await _userManager.DeleteAsync(user);
            throw new ValidationAppException("The role could not be assigned.", Describe(roleAssigned));
        }

        var token = await CreateEncodedResetTokenAsync(user);
        var invitationSent = await TrySendAsync(
            user.Email!,
            InvitationSubject,
            InvitationBody(user, resetLinkFactory(user.Email!, token)),
            cancellationToken);

        _logger.LogInformation(
            "Created CMS account {Email} with role {Role} in tenant {TenantId}",
            user.Email, dto.Role, tenantId);

        return new UserInviteResultDto
        {
            User = await ToDtoAsync(user, await LoadTenantNamesAsync(cancellationToken)),
            PasswordResetToken = token,
            InvitationEmailSent = invitationSent
        };
    }

    public async Task<CmsUserDto> UpdateUserAsync(
        string userId, UpdateUserDto dto, CancellationToken cancellationToken)
    {
        dto.Role = dto.Role.Trim();
        dto.FullName = dto.FullName?.Trim();
        await _updateValidator.ValidateAndThrowAsync(dto, cancellationToken);
        EnsureRoleAssignable(dto.Role);

        var user = await LoadManageableUserAsync(userId, cancellationToken);
        var isSelf = IsSelf(user);
        var currentRoles = await _userManager.GetRolesAsync(user);
        var currentRole = currentRoles.FirstOrDefault();

        if (isSelf && !string.Equals(currentRole, dto.Role, StringComparison.Ordinal))
        {
            throw new ValidationAppException("You cannot change your own role.");
        }

        if (isSelf && !dto.IsActive)
        {
            throw new ValidationAppException("You cannot deactivate your own account.");
        }

        if (!string.Equals(currentRole, dto.Role, StringComparison.Ordinal))
        {
            await EnsureNotLastAdministratorAsync(user, cancellationToken);
            if (currentRoles.Count > 0)
            {
                var removed = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removed.Succeeded)
                {
                    throw new ValidationAppException("The existing role could not be replaced.", Describe(removed));
                }
            }

            var added = await _userManager.AddToRoleAsync(user, dto.Role);
            if (!added.Succeeded)
            {
                throw new ValidationAppException("The new role could not be assigned.", Describe(added));
            }
        }

        if (user.IsActive && !dto.IsActive)
        {
            await EnsureNotLastAdministratorAsync(user, cancellationToken);
        }

        user.FullName = dto.FullName;
        user.IsActive = dto.IsActive;
        var updated = await _userManager.UpdateAsync(user);
        if (!updated.Succeeded)
        {
            throw new ValidationAppException("The account could not be updated.", Describe(updated));
        }

        return await ToDtoAsync(user, await LoadTenantNamesAsync(cancellationToken));
    }

    public async Task SetStatusAsync(string userId, bool isActive, CancellationToken cancellationToken)
    {
        var user = await LoadManageableUserAsync(userId, cancellationToken);
        if (IsSelf(user) && !isActive)
        {
            throw new ValidationAppException("You cannot deactivate your own account.");
        }

        if (user.IsActive && !isActive)
        {
            await EnsureNotLastAdministratorAsync(user, cancellationToken);
        }

        user.IsActive = isActive;
        var updated = await _userManager.UpdateAsync(user);
        if (!updated.Succeeded)
        {
            throw new ValidationAppException("The account status could not be changed.", Describe(updated));
        }
    }

    public async Task UnlockAsync(string userId, CancellationToken cancellationToken)
    {
        var user = await LoadManageableUserAsync(userId, cancellationToken);
        await _userManager.ResetAccessFailedCountAsync(user);
        var unlocked = await _userManager.SetLockoutEndDateAsync(user, null);
        if (!unlocked.Succeeded)
        {
            throw new ValidationAppException("The account could not be unlocked.", Describe(unlocked));
        }
    }

    public async Task<PasswordResetTokenDto> CreatePasswordResetTokenAsync(
        string userId, CancellationToken cancellationToken)
    {
        var user = await LoadManageableUserAsync(userId, cancellationToken);
        var token = await CreateEncodedResetTokenAsync(user);
        _logger.LogInformation("Password reset token issued for {Email} by {Actor}", user.Email, Actor);
        return new PasswordResetTokenDto
        {
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            Token = token
        };
    }

    public async Task RequestPasswordResetAsync(
        ForgotPasswordDto dto,
        Func<string, string, string> resetLinkFactory,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(resetLinkFactory);
        dto.Email = dto.Email.Trim().ToLowerInvariant();
        await _forgotValidator.ValidateAndThrowAsync(dto, cancellationToken);

        var user = await _userManager.FindByEmailAsync(dto.Email);

        // Deliberately silent about whether the address exists, is disabled, or belongs to
        // another institution — the caller always sees the same "check your inbox" result.
        if (user is null || !user.IsActive)
        {
            _logger.LogInformation("Password reset requested for unknown or disabled address.");
            return;
        }

        var resolvedTenant = _tenantContext.TenantId;
        var belongsHere = user.TenantId is null || user.TenantId == resolvedTenant;
        if (!belongsHere)
        {
            _logger.LogWarning(
                "Password reset for {Email} requested from a host bound to a different tenant.", user.Email);
            return;
        }

        var token = await CreateEncodedResetTokenAsync(user);
        await TrySendAsync(
            user.Email!,
            ResetSubject,
            ResetBody(user, resetLinkFactory(user.Email!, token)),
            cancellationToken);
    }

    public async Task ResetPasswordAsync(ResetPasswordDto dto, CancellationToken cancellationToken)
    {
        dto.Email = dto.Email.Trim().ToLowerInvariant();
        await _resetValidator.ValidateAndThrowAsync(dto, cancellationToken);

        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null || !user.IsActive)
        {
            // Same opaque failure for a bad address and a bad token.
            throw new ValidationAppException("This reset link is no longer valid. Please request a new one.");
        }

        string token;
        try
        {
            token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(dto.Token));
        }
        catch (FormatException)
        {
            throw new ValidationAppException("This reset link is no longer valid. Please request a new one.");
        }

        var result = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);
        if (!result.Succeeded)
        {
            throw new ValidationAppException(
                "This reset link is no longer valid. Please request a new one.", Describe(result));
        }

        // A successful reset also clears a lockout, otherwise the user still cannot sign in.
        await _userManager.ResetAccessFailedCountAsync(user);
        await _userManager.SetLockoutEndDateAsync(user, null);
        _logger.LogInformation("Password reset completed for {Email}", user.Email);
    }

    public async Task ChangePasswordAsync(
        string userId, ChangePasswordDto dto, CancellationToken cancellationToken)
    {
        await _changeValidator.ValidateAndThrowAsync(dto, cancellationToken);
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new UnauthorizedAppException();
        }

        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new NotFoundException("Account was not found.");

        var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        if (!result.Succeeded)
        {
            throw new ValidationAppException("The password could not be changed.", Describe(result));
        }

        _logger.LogInformation("Password changed for {Email}", user.Email);
    }

    private string Actor => _currentUser.UserId ?? "system";

    private bool IsSelf(ApplicationUser user) =>
        !string.IsNullOrEmpty(_currentUser.UserId)
        && string.Equals(user.Id, _currentUser.UserId, StringComparison.Ordinal);

    private void EnsureCanAdminister()
    {
        if (!_currentUser.IsSuperAdmin && !_currentUser.IsInRole(AppRoles.TenantAdmin))
        {
            throw new ForbiddenAppException("You do not have permission to manage CMS accounts.");
        }
    }

    private void EnsureRoleAssignable(string role)
    {
        EnsureCanAdminister();
        if (!GetAssignableRoles().Contains(role, StringComparer.Ordinal))
        {
            throw new ForbiddenAppException($"You are not permitted to assign the '{role}' role.");
        }
    }

    private Guid RequireTenant()
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            throw new TenantNotResolvedException();
        }

        return _tenantContext.TenantId.Value;
    }

    private async Task<Guid?> ResolveTargetTenantAsync(
        string role, Guid? requestedTenantId, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsSuperAdmin)
        {
            // Ignore whatever tenant was posted — a tenant admin is pinned to their own host.
            return RequireTenant();
        }

        if (string.Equals(role, AppRoles.SuperAdmin, StringComparison.Ordinal))
        {
            return null;
        }

        var tenantId = requestedTenantId ?? _tenantContext.TenantId;
        if (!tenantId.HasValue)
        {
            throw new ValidationAppException("Choose the institution this account belongs to.");
        }

        _ = await _tenants.GetAsync(tenantId.Value, cancellationToken)
            ?? throw new ValidationAppException("The selected institution was not found.");

        return tenantId;
    }

    private async Task<ApplicationUser> LoadManageableUserAsync(
        string userId, CancellationToken cancellationToken)
    {
        EnsureCanAdminister();
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new NotFoundException("Account was not found.");
        }

        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new NotFoundException("Account was not found.");

        if (_currentUser.IsSuperAdmin)
        {
            return user;
        }

        var tenantId = RequireTenant();
        if (user.TenantId != tenantId || await IsSuperAdminAsync(user))
        {
            // Indistinguishable from "does not exist" so tenant admins cannot probe
            // for accounts belonging to other institutions.
            throw new NotFoundException("Account was not found.");
        }

        return user;
    }

    /// <summary>
    /// Blocks the edit when it would remove the final active administrator of a tenant.
    /// </summary>
    private async Task EnsureNotLastAdministratorAsync(
        ApplicationUser user, CancellationToken cancellationToken)
    {
        if (!user.TenantId.HasValue || !await IsTenantAdminAsync(user))
        {
            return;
        }

        var tenantId = user.TenantId.Value;
        var candidates = await _userManager.Users
            .Where(x => x.TenantId == tenantId && x.IsActive && x.Id != user.Id)
            .ToListAsync(cancellationToken);

        foreach (var candidate in candidates)
        {
            if (await IsTenantAdminAsync(candidate))
            {
                return;
            }
        }

        throw new ValidationAppException(
            "This is the only active administrator for the institution. "
            + "Add another administrator before changing this account.");
    }

    private Task<bool> IsSuperAdminAsync(ApplicationUser user) =>
        _userManager.IsInRoleAsync(user, AppRoles.SuperAdmin);

    private Task<bool> IsTenantAdminAsync(ApplicationUser user) =>
        _userManager.IsInRoleAsync(user, AppRoles.TenantAdmin);

    private async Task<string> CreateEncodedResetTokenAsync(ApplicationUser user)
    {
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        return WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
    }

    private async Task<bool> TrySendAsync(
        string to, string subject, string body, CancellationToken cancellationToken)
    {
        if (!_emailSender.IsConfigured)
        {
            return false;
        }

        try
        {
            await _emailSender.SendAsync(to, subject, body, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            // Mail must never break account administration — the caller falls back to
            // showing the reset link so the admin can deliver it by hand.
            _logger.LogError(ex, "Failed to send '{Subject}' email to {Recipient}", subject, to);
            return false;
        }
    }

    private async Task<Dictionary<Guid, string>> LoadTenantNamesAsync(CancellationToken cancellationToken)
    {
        if (!_currentUser.IsSuperAdmin)
        {
            return _tenantContext.TenantId.HasValue
                ? new Dictionary<Guid, string>
                {
                    [_tenantContext.TenantId.Value] = _tenantContext.TenantName ?? string.Empty
                }
                : new Dictionary<Guid, string>();
        }

        var tenants = await _tenants.GetAllAsync(cancellationToken);
        return tenants.ToDictionary(x => x.Id, x => x.Name);
    }

    private async Task<CmsUserDto> ToDtoAsync(ApplicationUser user, Dictionary<Guid, string> tenantNames)
    {
        var roles = await _userManager.GetRolesAsync(user);
        return new CmsUserDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName,
            Role = roles.FirstOrDefault() ?? string.Empty,
            TenantId = user.TenantId,
            TenantName = user.TenantId.HasValue && tenantNames.TryGetValue(user.TenantId.Value, out var name)
                ? name
                : user.TenantId.HasValue ? null : "Platform",
            IsActive = user.IsActive,
            IsLockedOut = user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow,
            CreatedDate = user.CreatedDate
        };
    }

    private static IEnumerable<string> Describe(IdentityResult result) =>
        result.Errors.Select(x => x.Description).ToList();

    /// <summary>
    /// Random password that always satisfies the configured Identity complexity rules.
    /// Only ever used as a throwaway before the invitee sets their own password.
    /// </summary>
    private static string GeneratePassword()
    {
        const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        const string lower = "abcdefghijkmnopqrstuvwxyz";
        const string digits = "23456789";
        const string symbols = "!@#$%^&*?-_";
        const string all = upper + lower + digits + symbols;

        var characters = new List<char>
        {
            Pick(upper),
            Pick(lower),
            Pick(digits),
            Pick(symbols)
        };

        while (characters.Count < 16)
        {
            characters.Add(Pick(all));
        }

        // Fisher-Yates with a cryptographic source so class positions are not predictable.
        for (var i = characters.Count - 1; i > 0; i--)
        {
            var j = RandomNumberGenerator.GetInt32(i + 1);
            (characters[i], characters[j]) = (characters[j], characters[i]);
        }

        return new string(characters.ToArray());

        static char Pick(string source) => source[RandomNumberGenerator.GetInt32(source.Length)];
    }

    private static string InvitationBody(ApplicationUser user, string resetLink) =>
        $"""
        <p>Hello {Escape(user.FullName ?? user.Email ?? "there")},</p>
        <p>An account has been created for you on the school website CMS.</p>
        <p>Choose your password using the link below. It can only be used once.</p>
        <p><a href="{Escape(resetLink)}">Set your password</a></p>
        <p>If you were not expecting this message you can safely ignore it.</p>
        """;

    private static string ResetBody(ApplicationUser user, string resetLink) =>
        $"""
        <p>Hello {Escape(user.FullName ?? user.Email ?? "there")},</p>
        <p>We received a request to reset your CMS password.</p>
        <p><a href="{Escape(resetLink)}">Reset your password</a></p>
        <p>If you did not request this, no action is needed and your password stays unchanged.</p>
        """;

    private static string Escape(string value) =>
        System.Net.WebUtility.HtmlEncode(value);
}
