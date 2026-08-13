using Cms.Application.Interfaces;
using Cms.Domain.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Cms.Infrastructure.Tenancy;

/// <summary>
/// Maps the incoming host to a tenant and the website being requested.
///
/// Two public URL shapes are supported:
///   * a host bound to one website  — <c>school.example.edu/about</c>       (no prefix)
///   * a host shared by several     — <c>example.edu/school/about</c>       (site-key prefix)
///
/// For the shared shape the site segment is stripped into <see cref="HttpRequest.PathBase"/>,
/// so pages, static files and endpoints only ever deal with prefix-free paths and any site
/// key works — not just a hard-coded school/college pair.
/// </summary>
public class TenantResolutionMiddleware
{
    private const string SiteCookieName = "cms.site";
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantResolutionMiddleware> _logger;

    public TenantResolutionMiddleware(
        RequestDelegate next,
        ILogger<TenantResolutionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context,
        ITenantHostResolver resolver,
        ITenantContext tenantContext,
        ISiteContext siteContext)
    {
        var host = context.Request.Host.Host.TrimEnd('.').ToLowerInvariant();
        var resolved = await resolver.ResolveAsync(host, context.RequestAborted);

        if (resolved is null)
        {
            _logger.LogDebug("No tenant mapped for host {Host}", host);
            if (context.Request.Path.StartsWithSegments("/health")
                || context.Request.Path.StartsWithSegments("/swagger"))
            {
                await _next(context);
                return;
            }

            context.Response.StatusCode = StatusCodes.Status404NotFound;
            await context.Response.WriteAsync("No active website is configured for this domain.");
            return;
        }

        tenantContext.Set(resolved.TenantId, resolved.TenantCode, resolved.TenantName);

        var isManagementRequest = context.Request.Path.StartsWithSegments("/api")
            || context.Request.Path.StartsWithSegments("/CMS")
            || context.Request.Path.StartsWithSegments("/Account");

        if (isManagementRequest
            && context.Request.Query.TryGetValue("site", out var selectedSite)
            && !string.IsNullOrWhiteSpace(selectedSite))
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

        ResolvedSite? site = null;
        var basePath = string.Empty;

        if (isManagementRequest)
        {
            // Management callers may switch websites explicitly; a bound host is the fallback.
            var requestedKey = ResolveManagementSiteKey(context);
            if (!string.IsNullOrWhiteSpace(requestedKey))
            {
                site = FindByKey(resolved, requestedKey);
            }

            site ??= FindById(resolved, resolved.BoundSiteId);
        }
        else if (TryConsumeSitePrefix(context, resolved, out var prefixed, out var consumedSegment))
        {
            // An explicit site key in the URL wins, including on a bound domain. Binding decides
            // which website answers the *root* of a host; it must not make the tenant's other
            // websites unreachable, or a platform host with a domain row against it would serve
            // one site for every URL and turn every /{siteKey} link into a 404.
            site = prefixed;
            basePath = consumedSegment;
        }
        else if (resolved.BoundSiteId is Guid boundSiteId)
        {
            // No prefix: a bound public domain is authoritative and cannot be overridden.
            site = FindById(resolved, boundSiteId);
        }

        site ??= resolved.Sites.FirstOrDefault(s => s.IsDefault) ?? resolved.Sites.FirstOrDefault();

        if (site is not null)
        {
            // BasePath is the prefix every link on this website must start with, so it has to
            // include whatever the application itself is mounted under. A deployment that serves
            // the public site at /site needs "/site/college", not "/college" — the latter leaves
            // the application entirely and lands on whatever else the host is serving.
            var appBase = context.Request.PathBase.Value ?? string.Empty;

            if (basePath.Length > 0)
            {
                // The site key was taken out of the path, so it is already part of the path base.
                basePath = appBase;
            }
            else if (resolved.BoundSiteId == site.Id)
            {
                basePath = appBase;
            }
            else
            {
                // Shared host with no prefix in the URL — links must still carry one.
                basePath = appBase + "/" + site.SiteKey;
            }

            siteContext.Set(site.Id, site.SiteKey, site.Name, basePath);
        }

        await _next(context);
    }

    private static ResolvedSite? FindById(ResolvedHost resolved, Guid? siteId) =>
        siteId is Guid id ? resolved.Sites.FirstOrDefault(s => s.Id == id) : null;

    private static ResolvedSite? FindByKey(ResolvedHost resolved, string siteKey) =>
        resolved.Sites.FirstOrDefault(s => string.Equals(s.SiteKey, siteKey, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Moves a leading <c>/{siteKey}</c> segment from the path into the path base so the rest of
    /// the pipeline sees prefix-free paths and generated links keep the prefix automatically.
    /// </summary>
    private static bool TryConsumeSitePrefix(
        HttpContext context,
        ResolvedHost resolved,
        out ResolvedSite? site,
        out string consumedSegment)
    {
        site = null;
        consumedSegment = string.Empty;

        var path = context.Request.Path.Value;
        if (string.IsNullOrEmpty(path) || path.Length < 2)
        {
            return false;
        }

        var end = path.IndexOf('/', 1);
        var segment = end < 0 ? path[1..] : path[1..end];
        if (segment.Length == 0)
        {
            return false;
        }

        var match = FindByKey(resolved, segment);
        if (match is null)
        {
            return false;
        }

        var remainder = end < 0 ? string.Empty : path[end..];
        context.Request.PathBase = context.Request.PathBase.Add("/" + segment);
        context.Request.Path = remainder.Length == 0 ? "/" : remainder;

        // Minimal hosting runs endpoint matching ahead of this middleware, so an endpoint may
        // already have been selected against the *un-rewritten* path — "/school" matching the
        // page-by-slug route, for example. UseRouting() further down skips matching when an
        // endpoint is already set, so that stale choice would win and 404. Dropping it forces a
        // fresh match against the path we just rewrote.
        context.SetEndpoint(null);

        site = match;
        consumedSegment = "/" + segment;
        return true;
    }

    private static string? ResolveManagementSiteKey(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(HttpHeaderNames.SiteKey, out var header)
            && !string.IsNullOrWhiteSpace(header))
        {
            return header.ToString();
        }

        if (context.Request.Query.TryGetValue("site", out var query) && !string.IsNullOrWhiteSpace(query))
        {
            return query.ToString();
        }

        if (context.Request.Cookies.TryGetValue(SiteCookieName, out var cookie)
            && !string.IsNullOrWhiteSpace(cookie))
        {
            return cookie;
        }

        return null;
    }
}
