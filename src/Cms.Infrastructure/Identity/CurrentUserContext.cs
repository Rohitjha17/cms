using System.Security.Claims;
using Cms.Application.Interfaces;
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
}
