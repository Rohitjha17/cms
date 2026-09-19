using Cms.Application.DTOs.Content;
using Cms.Application.Interfaces;
using Cms.Domain.Entities;
using Cms.Domain.Enums;
using Cms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Cms.Api.IntegrationTests;

/// <summary>
/// A school can take a page over completely: its own markup, the full width, and none of the
/// built-in layout underneath. Without the second half of that, a school that wrote its own
/// gallery got ours printed under it as well.
/// </summary>
public sealed class CustomHtmlPageTests : IClassFixture<PublicWebFactory>
{
    private readonly PublicWebFactory _factory;
    private readonly HttpClient _client;

    public CustomHtmlPageTests(PublicWebFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    /// <summary>
    /// With the switch on, the page is a frame and nothing else: no title band and none of the
    /// built-in layout. The school's markup is served at the frame's address as a document of
    /// its own, without the design's stylesheet, so the template cannot restyle it.
    /// </summary>
    [Fact]
    public async Task WithTheSwitchOn_ThePageIsOnlyTheSchoolsOwnMarkup()
    {
        await SaveGalleryPageAsync(
            custom: true,
            html: "<section id=\"mine\"><h2>Our own gallery</h2></section>",
            items: new { items = new[] { new { album = "A", type = "image", url = "/uploads/x.jpg", caption = "Built in" } } });

        var page = await _client.GetStringAsync("/gallery");

        Assert.Contains("class=\"custom-page__frame\"", page);
        Assert.Contains("src=\"?handler=Html\"", page);
        // the built-in gallery must not be drawn underneath it, nor the title band above it
        Assert.DoesNotContain("gallery-grid", page);
        Assert.DoesNotContain("Built in", page);
        Assert.DoesNotContain("page-hero", page);

        var document = await _client.GetStringAsync("/gallery?handler=Html");

        Assert.StartsWith("<!doctype html>", document.TrimStart(), StringComparison.OrdinalIgnoreCase);
        Assert.Contains("id=\"mine\"", document);
        Assert.Contains("Our own gallery", document);
        // the design's stylesheet does not reach the school's page
        Assert.DoesNotContain("site.css", document);
    }

    /// <summary>
    /// A whole page pasted in — doctype, head, its own styles — is served as written, with only
    /// the bridge to the website added before the body closes.
    /// </summary>
    [Fact]
    public async Task AWholeDocument_IsServedAsWritten()
    {
        const string head = "<!DOCTYPE html><html><head><style>h1{color:rebeccapurple}</style></head><body><h1 class=\"big\">Ours</h1>";
        await SaveGalleryPageAsync(custom: true, html: head + "</body></html>", items: new { items = Array.Empty<object>() });

        var document = await _client.GetStringAsync("/gallery?handler=Html");

        Assert.StartsWith(head, document);
        Assert.EndsWith("</body></html>", document);
        Assert.Contains("cmsFrame", document);
    }

    /// <summary>
    /// The school's HTML is kept whole, scripts included, so it must not run as the website. The
    /// browser sandboxes it into an origin of its own — by response header, so opening the frame's
    /// address directly does not escape it — and never with allow-same-origin.
    /// </summary>
    [Fact]
    public async Task TheSchoolsPage_RunsSandboxedInAnOriginOfItsOwn()
    {
        await SaveGalleryPageAsync(custom: true, html: "<p>Ours</p>", items: new { items = Array.Empty<object>() });

        using var response = await _client.GetAsync("/gallery?handler=Html");
        var policy = string.Join(";", response.Headers.GetValues("Content-Security-Policy"));

        Assert.Contains("sandbox allow-scripts", policy);
        Assert.DoesNotContain("allow-same-origin", policy);
        // the site's own policy still applies alongside it
        Assert.Contains("frame-ancestors 'self'", policy);

        var page = await _client.GetStringAsync("/gallery");
        Assert.Contains("sandbox=\"allow-scripts", page);
        Assert.DoesNotContain("allow-same-origin", page);
    }

    /// <summary>
    /// Saved through the console, a designed page kept only its bare tags: the sanitiser took the
    /// head, the styles and every class, and the page rendered as unstyled text. With the switch on
    /// the HTML is kept whole; with it off it is sanitised, because it is then written into the
    /// website's own page.
    /// </summary>
    [Fact]
    public async Task SavingOwnHtml_KeepsItsStylesAndClasses_AndSwitchingOffSanitisesIt()
    {
        var (content, db) = await ArrangeSiteAsync();
        const string designed = "<!doctype html><html><head><style>.big{color:red}</style></head><body><h1 class=\"big\">Fees</h1><script>console.log(1)</script></body></html>";

        var kept = await content.SavePageAsync(null, new SavePageDto
        {
            Title = "Fees", Slug = "fees", PageType = PageType.Custom,
            Content = designed, UseCustomHtml = true, IsActive = true
        }, default);

        Assert.Equal(designed, (await db.Pages.IgnoreQueryFilters().SingleAsync(x => x.Id == kept.Id)).Content);

        await content.SavePageAsync(kept.Id, new SavePageDto
        {
            Title = "Fees", Slug = "fees", PageType = PageType.Custom,
            Content = designed, UseCustomHtml = false, IsActive = true
        }, default);

        db.ChangeTracker.Clear();
        var stored = (await db.Pages.IgnoreQueryFilters().SingleAsync(x => x.Id == kept.Id)).Content!;
        Assert.DoesNotContain("<script", stored);
        Assert.DoesNotContain("<style", stored);
        Assert.Contains("Fees", stored);
    }

    private async Task<(ISiteContentService Content, ApplicationDbContext Db)> ArrangeSiteAsync()
    {
        var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var tenantId = await db.Tenants.IgnoreQueryFilters().Select(x => x.Id).FirstAsync();
        var site = new Site
        {
            TenantId = tenantId,
            Name = "Own HTML Academy",
            SiteKey = $"h{Guid.NewGuid():N}"[..12],
            IsActive = true
        };
        db.Sites.Add(site);
        await db.SaveChangesAsync();

        scope.ServiceProvider.GetRequiredService<ITenantContext>().Set(tenantId, "test", "Test");
        scope.ServiceProvider.GetRequiredService<ISiteContext>().Set(site.Id, site.SiteKey, site.Name);

        return (scope.ServiceProvider.GetRequiredService<ISiteContentService>(), db);
    }

    [Fact]
    public async Task WithTheSwitchOff_TheBuiltInLayoutIsUsed()
    {
        await SaveGalleryPageAsync(
            custom: false,
            html: "<p>An introduction.</p>",
            items: new { items = new[] { new { album = "A", type = "image", url = "/uploads/x.jpg", caption = "Built in" } } });

        var page = await _client.GetStringAsync("/gallery");

        Assert.Contains("An introduction.", page);
        Assert.Contains("gallery-grid", page);
        Assert.Contains("Built in", page);
        Assert.DoesNotContain("class=\"custom-page__frame\"", page);

        // and there is no own-HTML document to frame
        using var frame = await _client.GetAsync("/gallery?handler=Html");
        Assert.Equal(System.Net.HttpStatusCode.NotFound, frame.StatusCode);
    }

    private async Task SaveGalleryPageAsync(bool custom, string html, object items)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var siteId = await db.Sites.IgnoreQueryFilters()
            .Where(x => x.SiteKey == "school").Select(x => x.Id).FirstAsync();

        var page = await db.Pages.IgnoreQueryFilters()
            .FirstAsync(x => x.SiteId == siteId && x.PageType == PageType.Gallery);

        page.Content = html;
        page.JsonData = System.Text.Json.JsonSerializer.Serialize(items);
        page.UseCustomHtml = custom;
        page.IsActive = true;
        await db.SaveChangesAsync();
    }
}
