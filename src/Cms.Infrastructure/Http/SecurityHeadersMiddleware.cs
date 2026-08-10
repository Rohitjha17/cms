using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Cms.Infrastructure.Http;

/// <summary>
/// Baseline browser hardening applied to every response.
///
/// The content security policy allows inline styles/scripts because the Razor views compose
/// per-tenant branding inline, and permits the small set of CDNs the applications load
/// (Google Fonts for the public sites, jsDelivr for the Admin editor). Media may come from
/// any HTTPS origin so tenant S3 buckets and CDNs work without redeploying.
/// </summary>
public sealed class SecurityHeadersMiddleware
{
    private const string ContentSecurityPolicy =
        "default-src 'self'; " +
        "base-uri 'self'; " +
        "object-src 'none'; " +
        "frame-ancestors 'self'; " +
        "form-action 'self'; " +
        "img-src 'self' data: blob: https:; " +
        "media-src 'self' blob: https:; " +
        "font-src 'self' data: https://fonts.gstatic.com https://cdn.jsdelivr.net; " +
        "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com https://cdn.jsdelivr.net; " +
        "script-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net; " +
        "connect-src 'self' https:; " +
        "frame-src 'self' https:";

    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next) => _next = next;

    public Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;
        headers["X-Content-Type-Options"] = "nosniff";
        headers["X-Frame-Options"] = "SAMEORIGIN";
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=(), payment=()";
        headers["Content-Security-Policy"] = ContentSecurityPolicy;
        headers.Remove("X-Powered-By");
        return _next(context);
    }
}

public static class SecurityHeadersApplicationBuilderExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app) =>
        app.UseMiddleware<SecurityHeadersMiddleware>();
}
