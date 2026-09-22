using Cms.Application.Interfaces;
using Cms.Domain.Constants;
using Microsoft.AspNetCore.Http;

namespace Cms.Infrastructure.Tenancy;

/// <summary>
/// Decides which institution a signed-in console user is working in.
///
/// The console is served from one address for every institution. Resolving the tenant from
/// that address alone put every user, and every website created from the console, into
/// whichever institution owned the address — so a new school's staff could see every other
/// school, and a second institution could never be worked on at all.
///
/// After sign-in the institution comes from the account instead:
/// <list type="bullet">
/// <item>A school's own user always works in the institution their account belongs to. They
/// cannot choose another; nothing they send is consulted.</item>
/// <item>A SuperAdmin chooses one with <c>?tenant={id}</c>, remembered in a cookie, and falls
/// back to the address's institution until they do.</item>
/// </list>
/// The website within it is then chosen the way it always was — <c>?site=</c>, then the
/// remembered site — among that institution's own websites only.
///
/// Runs after authentication, which is the first point the account is known, and before
/// <see cref="TenantAuthorizationMiddleware"/>, which then finds the account and the
/// institution in agreement.
/// </summary>
public sealed class ManagementTenantMiddleware
{
    public const string TenantCookieName = "cms.tenant";
    private const string SiteCookieName = "cms.site";

    private readonly RequestDelegate _next;

    public ManagementTenantMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(
        HttpContext context,
        ITenantHostResolver resolver,
        ITenantContext tenantContext,
        ISiteContext siteContext)
    {
        var user = context.User;
        if (user.Identity?.IsAuthenticated != true)
        {
            await _next(context);
            return;
        }

        Guid? wanted = null;
        var switched = false;

        if (user.IsInRole(AppRoles.SuperAdmin))
        {
            if (context.Request.Query.TryGetValue("tenant", out var chosen)
                && Guid.TryParse(chosen, out var chosenId))
            {
                wanted = chosenId;
                switched = true;
            }
            else if (context.Request.Cookies.TryGetValue(TenantCookieName, out var remembered)
                && Guid.TryParse(remembered, out var rememberedId))
            {
                wanted = rememberedId;
            }
        }
        else if (Guid.TryParse(user.FindFirst(AppClaimTypes.TenantId)?.Value, out var ownId))
        {
            wanted = ownId;
        }

        if (wanted is not Guid tenantId)
        {
            await _next(context);
            return;
        }

        var resolved = await resolver.ResolveTenantAsync(tenantId, cancellationToken: context.RequestAborted);
        if (resolved is null)
        {
            // A SuperAdmin's remembered choice that has since been disabled or removed: forget it
            // and carry on in the address's institution. A school user whose institution is gone
            // is left unresolved, and the authorization step turns them away.
            if (user.IsInRole(AppRoles.SuperAdmin))
            {
                context.Response.Cookies.Delete(TenantCookieName);
            }

            await _next(context);
            return;
        }

        if (switched)
        {
            Remember(context, TenantCookieName, tenantId.ToString());
        }

        if (!switched && resolved.TenantId == tenantContext.TenantId && siteContext.IsResolved)
        {
            // Already the right institution and website — the address's own.
            await _next(context);
            return;
        }

        tenantContext.Set(resolved.TenantId, resolved.TenantCode, resolved.TenantName);

        // The remembered website belongs to whichever institution was open before; after a switch
        // only an explicit ?site= in the same request is honoured.
        string? requestedKey = context.Request.Query.TryGetValue("site", out var siteQuery)
            && !string.IsNullOrWhiteSpace(siteQuery)
                ? siteQuery.ToString()
                : switched
                    ? null
                    : context.Request.Cookies.TryGetValue(SiteCookieName, out var siteCookie) ? siteCookie : null;

        var site = Find(resolved, requestedKey);
        if (site is null && !string.IsNullOrWhiteSpace(requestedKey))
        {
            // A website created moments ago may not be in the cached list yet.
            resolved = await resolver.ResolveTenantAsync(tenantId, refresh: true, context.RequestAborted) ?? resolved;
            site = Find(resolved, requestedKey);
        }

        site ??= resolved.Sites.FirstOrDefault(s => s.IsDefault) ?? resolved.Sites.FirstOrDefault();

        if (switched)
        {
            if (site is null)
            {
                context.Response.Cookies.Delete(SiteCookieName);
            }
            else
            {
                Remember(context, SiteCookieName, site.SiteKey);
            }
        }

        if (site is null)
        {
            siteContext.Clear();
        }
        else
        {
            var appBase = context.Request.PathBase.Value ?? string.Empty;
            siteContext.Set(site.Id, site.SiteKey, site.Name, appBase + "/" + site.SiteKey);
        }

        await _next(context);
    }

    private static ResolvedSite? Find(ResolvedHost resolved, string? siteKey) =>
        string.IsNullOrWhiteSpace(siteKey)
            ? null
            : resolved.Sites.FirstOrDefault(s => string.Equals(s.SiteKey, siteKey, StringComparison.OrdinalIgnoreCase));

    private static void Remember(HttpContext context, string name, string value) =>
        context.Response.Cookies.Append(name, value, new CookieOptions
        {
            HttpOnly = true,
            IsEssential = true,
            SameSite = SameSiteMode.Lax,
            Secure = context.Request.IsHttps,
            Expires = DateTimeOffset.UtcNow.AddYears(1)
        });
}
