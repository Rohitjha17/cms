using Cms.Application.Interfaces;
using Cms.Domain.Constants;
using Cms.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Cms.Infrastructure.Tenancy;

public class TenantResolutionMiddleware
{
    private const string SiteCookieName = "cms.site";
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TenantResolutionMiddleware> _logger;

    public TenantResolutionMiddleware(
        RequestDelegate next,
        IConfiguration configuration,
        ILogger<TenantResolutionMiddleware> logger)
    {
        _next = next;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ApplicationDbContext db, ITenantContext tenantContext, ISiteContext siteContext)
    {
        var host = context.Request.Host.Host;
        var domain = await db.TenantDomains
            .IgnoreQueryFilters()
            .Include(d => d.Tenant)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.DomainName == host && d.IsActive && d.Tenant.IsActive);

        var tenant = domain?.Tenant;
        if (tenant is null && _configuration.GetValue<bool>("DemoMode:Enabled"))
        {
            tenant = await db.Tenants
                .IgnoreQueryFilters()
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.Code == "demo" && item.IsActive);
        }

        if (tenant is null)
        {
            _logger.LogDebug("No tenant mapped for host {Host}", host);
            await _next(context);
            return;
        }

        tenantContext.Set(tenant.Id, tenant.Code, tenant.Name);

        var siteKey = ResolveSiteKey(context);
        if (context.Request.Query.TryGetValue("site", out var selectedSite) && !string.IsNullOrWhiteSpace(selectedSite))
        {
            context.Response.Cookies.Append(SiteCookieName, selectedSite.ToString(), new CookieOptions
            {
                HttpOnly = true,
                IsEssential = true,
                SameSite = SameSiteMode.Lax,
                Secure = context.Request.IsHttps,
                Expires = DateTimeOffset.UtcNow.AddYears(1)
            });
        }

        var sites = await db.Sites
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(s => s.TenantId == tenant.Id && s.IsActive)
            .ToListAsync();

        var site = !string.IsNullOrWhiteSpace(siteKey)
            ? sites.FirstOrDefault(s => string.Equals(s.SiteKey, siteKey, StringComparison.OrdinalIgnoreCase))
            : null;
        site ??= sites.FirstOrDefault(s => s.IsDefault) ?? sites.FirstOrDefault();

        if (site is not null)
        {
            siteContext.Set(site.Id, site.SiteKey, site.Name);
        }

        await _next(context);
    }

    private static string? ResolveSiteKey(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(HttpHeaderNames.SiteKey, out var header) && !string.IsNullOrWhiteSpace(header))
        {
            return header.ToString();
        }

        if (context.Request.Query.TryGetValue("site", out var query) && !string.IsNullOrWhiteSpace(query))
        {
            return query.ToString();
        }

        if (context.Request.Cookies.TryGetValue(SiteCookieName, out var cookie) && !string.IsNullOrWhiteSpace(cookie))
        {
            return cookie;
        }

        var path = context.Request.Path.Value ?? string.Empty;
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length > 0 && (segments[0].Equals("school", StringComparison.OrdinalIgnoreCase)
            || segments[0].Equals("college", StringComparison.OrdinalIgnoreCase)))
        {
            return segments[0].ToLowerInvariant();
        }

        return null;
    }
}
