using Cms.Application.DTOs.Users;
using Cms.Application.Interfaces;
using Cms.Domain.Constants;
using Cms.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Cms.Api.Controllers;

/// <summary>
/// CMS account administration. Tenant scoping and role-escalation rules are enforced in
/// <see cref="IUserManagementService"/>, so the attributes here are the outer gate only.
/// </summary>
[ApiController]
[Route("api/users")]
[Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.TenantAdmin}")]
public sealed class UsersController : ControllerBase
{
    private readonly IUserManagementService _service;
    private readonly IConfiguration _configuration;

    public UsersController(IUserManagementService service, IConfiguration configuration)
    {
        _service = service;
        _configuration = configuration;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<CmsUserDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CmsUserDto>>>> GetAll(
        CancellationToken cancellationToken) =>
        Ok(ApiResponse<IReadOnlyList<CmsUserDto>>.Ok(await _service.GetUsersAsync(cancellationToken)));

    [HttpGet("roles")]
    public ActionResult<ApiResponse<IReadOnlyList<string>>> AssignableRoles() =>
        Ok(ApiResponse<IReadOnlyList<string>>.Ok(_service.GetAssignableRoles()));

    [HttpGet("{userId}")]
    public async Task<ActionResult<ApiResponse<CmsUserDto>>> Get(
        string userId, CancellationToken cancellationToken) =>
        Ok(ApiResponse<CmsUserDto>.Ok(await _service.GetUserAsync(userId, cancellationToken)));

    [HttpPost]
    public async Task<ActionResult<ApiResponse<UserInviteResultDto>>> Create(
        SaveUserDto dto, CancellationToken cancellationToken)
    {
        var data = await _service.CreateUserAsync(dto, BuildResetLink, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<UserInviteResultDto>.Created(data));
    }

    [HttpPut("{userId}")]
    public async Task<ActionResult<ApiResponse<CmsUserDto>>> Update(
        string userId, UpdateUserDto dto, CancellationToken cancellationToken) =>
        Ok(ApiResponse<CmsUserDto>.Ok(
            await _service.UpdateUserAsync(userId, dto, cancellationToken), "Account updated."));

    [HttpPatch("{userId}/status")]
    public async Task<ActionResult<ApiResponse>> SetStatus(
        string userId, SetUserStatusDto dto, CancellationToken cancellationToken)
    {
        await _service.SetStatusAsync(userId, dto.IsActive, cancellationToken);
        return Ok(ApiResponse.Ok(dto.IsActive ? "Account activated." : "Account deactivated."));
    }

    [HttpPost("{userId}/unlock")]
    public async Task<ActionResult<ApiResponse>> Unlock(string userId, CancellationToken cancellationToken)
    {
        await _service.UnlockAsync(userId, cancellationToken);
        return Ok(ApiResponse.Ok("Account unlocked."));
    }

    /// <summary>
    /// Issues a one-time reset link an administrator can pass to the account holder.
    /// </summary>
    [HttpPost("{userId}/password-reset-link")]
    public async Task<ActionResult<ApiResponse<object>>> CreateResetLink(
        string userId, CancellationToken cancellationToken)
    {
        var token = await _service.CreatePasswordResetTokenAsync(userId, cancellationToken);
        return Ok(ApiResponse<object>.Ok(
            new { token.UserId, token.Email, resetLink = BuildResetLink(token.Email, token.Token) },
            "Reset link generated."));
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [EnableRateLimiting("public-forms")]
    public async Task<ActionResult<ApiResponse>> ForgotPassword(
        ForgotPasswordDto dto, CancellationToken cancellationToken)
    {
        await _service.RequestPasswordResetAsync(dto, BuildResetLink, cancellationToken);

        // Always the same answer, whether or not the address is known.
        return Ok(ApiResponse.Ok(
            "If that address belongs to an account, a password reset email is on its way."));
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    [EnableRateLimiting("public-forms")]
    public async Task<ActionResult<ApiResponse>> ResetPassword(
        ResetPasswordDto dto, CancellationToken cancellationToken)
    {
        await _service.ResetPasswordAsync(dto, cancellationToken);
        return Ok(ApiResponse.Ok("Your password has been updated. You can now sign in."));
    }

    [HttpPost("change-password")]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.TenantAdmin},{AppRoles.Editor}")]
    public async Task<ActionResult<ApiResponse>> ChangePassword(
        ChangePasswordDto dto, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        await _service.ChangePasswordAsync(userId ?? string.Empty, dto, cancellationToken);
        return Ok(ApiResponse.Ok("Your password has been updated."));
    }

    /// <summary>
    /// Reset links point at the Admin workspace. <c>Platform:AdminBaseUrl</c> is used when
    /// the Admin app is on its own host; otherwise the current request origin is assumed.
    /// </summary>
    private string BuildResetLink(string email, string token)
    {
        var configured = _configuration["Platform:AdminBaseUrl"]?.TrimEnd('/');
        var origin = string.IsNullOrWhiteSpace(configured)
            ? $"{Request.Scheme}://{Request.Host}"
            : configured;

        return $"{origin}/Account/ResetPassword"
            + $"?email={Uri.EscapeDataString(email)}"
            + $"&token={Uri.EscapeDataString(token)}";
    }
}
