using System.Security.Claims;
using Cms.Application.Interfaces;
using Cms.Domain.Constants;
using Microsoft.AspNetCore.Http;

namespace Cms.Infrastructure.Identity;

public sealed class CurrentUserContext : ICurrentUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId =>
        _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

    public string? DisplayName =>
        _httpContextAccessor.HttpContext?.User.Identity?.Name;

    public bool IsSuperAdmin => IsInRole(AppRoles.SuperAdmin);

    public bool IsInRole(string role) =>
        _httpContextAccessor.HttpContext?.User.IsInRole(role) == true;
}
