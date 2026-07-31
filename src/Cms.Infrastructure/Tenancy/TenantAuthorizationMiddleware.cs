using Cms.Application.Interfaces;
using Cms.Domain.Constants;
using Cms.Shared.Responses;
using Microsoft.AspNetCore.Http;

namespace Cms.Infrastructure.Tenancy;

/// <summary>
/// Ensures an authenticated tenant user can act only on the tenant resolved from the host.
/// Super administrators intentionally span tenants.
/// </summary>
public sealed class TenantAuthorizationMiddleware
{
    private readonly RequestDelegate _next;

    public TenantAuthorizationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        if (context.User.Identity?.IsAuthenticated == true
            && !context.User.IsInRole(AppRoles.SuperAdmin))
        {
            var tenantClaim = context.User.FindFirst(AppClaimTypes.TenantId)?.Value;
            var matchesResolvedTenant = tenantContext.TenantId.HasValue
                && Guid.TryParse(tenantClaim, out var userTenantId)
                && userTenantId == tenantContext.TenantId.Value;

            if (!matchesResolvedTenant)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                if (context.Request.Path.StartsWithSegments("/api"))
                {
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(ApiResponse.Fail(
                        "Your account does not have access to this tenant.",
                        StatusCodes.Status403Forbidden));
                }
                else
                {
                    context.Response.ContentType = "text/plain";
                    await context.Response.WriteAsync("Your account does not have access to this tenant.");
                }
                return;
            }
        }

        await _next(context);
    }
}
