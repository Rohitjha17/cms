using Cms.Infrastructure.Configuration;
using Cms.Application.DependencyInjection;
using Cms.Infrastructure.DependencyInjection;
using Cms.Infrastructure.Http;
using Cms.Infrastructure.Persistence.Seed;
using Cms.Infrastructure.Storage;
using Cms.Infrastructure.Tenancy;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using System.Security;
using Cms.Application.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddRazorPages();

// Public pages are read-heavy and identical for every visitor of a given host, so they are
// cached per host + full path. The window is short and configurable: an editor's change
// appears within it without needing an explicit purge. Set to 0 to disable.
var publicCacheSeconds = builder.Configuration.GetValue("PublicCache:Seconds", 30);
builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(policy => policy.NoCache());
    options.AddPolicy("public-pages", policy => policy
        .Expire(TimeSpan.FromSeconds(Math.Max(publicCacheSeconds, 1)))
        .SetVaryByHost(true)
        .SetVaryByRouteValue("slug", "key")
        // The site prefix on a shared domain lives in PathBase, which is NOT part of the
        // default cache key. Without this, example.com/school/news and
        // example.com/college/news would collide and one school could be served the
        // other's content.
        .VaryByValue(context =>
            new KeyValuePair<string, string>("pathbase", context.Request.PathBase.Value ?? string.Empty))
        .Tag("public"));
});
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Only form submissions are throttled. Reading pages must stay unlimited — a visitor
    // browsing the site hits the same Razor Page endpoint as the contact form POST.
    options.AddPolicy("public-forms", context => HttpMethods.IsPost(context.Request.Method)
        ? RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(10),
                QueueLimit = 0
            })
        : RateLimitPartition.GetNoLimiter("public-reads"));
});

var trustForwardedHeaders = builder.Configuration.GetValue<bool>("Proxy:TrustForwardedHeaders");
if (trustForwardedHeaders)
{
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();
    });
}

var app = builder.Build();

// Stop here rather than serve a school's website on demo settings.
ProductionReadiness.ThrowIfMisconfigured(
    app.Configuration,
    app.Environment,
    app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup"));


if (!builder.Configuration.GetValue<bool>("Seed:SkipStartup"))
{
    await DatabaseSeeder.SeedAsync(app.Services);
}

// A deployment can mount the public site under a prefix such as /site on the platform's own
// host while serving a school's own domain at the root, so the prefix is per request, not per
// process. Must run before tenant resolution so /{siteKey} prefixes still work.
// Read from the built application: configuration sources added by the host — a test host, or a
// provider that supplies settings late — are merged at Build(), so anything captured before it
// can be stale.
var trustProxy = app.Configuration.GetValue<bool>("Proxy:TrustForwardedHeaders");
app.UseMiddleware<ForwardedPrefixMiddleware>(
    trustProxy,
    app.Configuration["PathBase"] ?? string.Empty);

if (trustProxy)
{
    app.UseForwardedHeaders();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/_status/error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStatusCodePagesWithReExecute("/_status/not-found");
app.UseSecurityHeaders();

// Runs before static files and routing: it strips a shared-domain "/{siteKey}" prefix into
// PathBase, so assets, pages and endpoints below only ever see prefix-free paths.
app.UseMiddleware<TenantResolutionMiddleware>();

app.UseStaticFiles();
app.UseLocalMediaFiles();
app.UseRouting();
app.UseRateLimiter();

// Re-read from the built application: configuration added by the host — a test host, or a
// provider that supplies settings late — is merged at Build(), so the value captured before it
// can be stale. Reading it here is what makes "PublicCache:Seconds=0" actually stop caching.
if (app.Configuration.GetValue("PublicCache:Seconds", publicCacheSeconds) > 0)
{
    app.UseOutputCache();
}

// Pages opt in with [OutputCache(PolicyName = "public-pages")]. Only form-free pages do:
// anything rendering an antiforgery token must never be cached, or every visitor would be
// served one another's token and submissions would be rejected. That excludes
// Content.cshtml, which renders the contact form.
app.MapRazorPages();

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.MapGet("/robots.txt", async (
    HttpContext context,
    IWebsiteService websiteService,
    CancellationToken cancellationToken) =>
{
    // A website marked "do not index" must say so to crawlers, not just in a meta tag.
    var allowIndexing = true;
    try
    {
        var website = await websiteService.GetPublicWebsiteAsync(cancellationToken);
        allowIndexing = website.Seo.AllowIndexing;
    }
    catch
    {
        // Fall through to the permissive default rather than blocking the file.
    }

    var body = allowIndexing
        ? $"User-agent: *\nAllow: /\nSitemap: {Origin(context)}/sitemap.xml\n"
        : "User-agent: *\nDisallow: /\n";
    return Results.Text(body, "text/plain");
});

app.MapGet("/sitemap.xml", async (
    HttpContext context,
    ISiteContentService contentService,
    CancellationToken cancellationToken) =>
{
    var pages = await contentService.GetPagesAsync(false, cancellationToken);
    var origin = Origin(context);
    var entries = new List<(string Loc, DateTime? LastModified)> { (origin + "/", null) };
    entries.AddRange(pages.Select(x =>
        ($"{origin}/{Uri.EscapeDataString(x.Slug)}", (DateTime?)(x.UpdatedDate ?? x.CreatedDate))));

    var body = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>"
        + "<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">"
        + string.Concat(entries.Select(entry =>
            "<url><loc>" + SecurityElement.Escape(entry.Loc) + "</loc>"
            + (entry.LastModified is DateTime modified
                ? $"<lastmod>{modified:yyyy-MM-dd}</lastmod>"
                : string.Empty)
            + "</url>"))
        + "</urlset>";
    return Results.Text(body, "application/xml");
});

app.Run();

// Includes PathBase so a shared domain emits ".../school/about" rather than "/about".
static string Origin(HttpContext context) =>
    $"{context.Request.Scheme}://{context.Request.Host}{context.Request.PathBase}";

public partial class Program;
