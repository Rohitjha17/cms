extern alias webapp;

using System.Net;
using Cms.Domain.Entities;
using Cms.Domain.Enums;
using Cms.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WebProgram = webapp::Program;

namespace Cms.Api.IntegrationTests;

/// <summary>
/// The two public URL shapes a school CMS has to serve — one domain shared by several
/// websites, and a domain dedicated to a single school — plus the guarantee that reading
/// pages is never rate limited.
/// </summary>
public sealed class PublicRoutingTests : IClassFixture<PublicWebFactory>, IAsyncLifetime
{
    private const string DedicatedHost = "noida.cambridge.test";
    private const string DedicatedSiteKey = "noida-campus";
    private const string DedicatedBrand = "Cambridge School Noida";

    private readonly PublicWebFactory _factory;
    private readonly HttpClient _client;

    public PublicRoutingTests(PublicWebFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var tenantId = await db.Tenants.IgnoreQueryFilters()
            .Where(x => x.Code == "demo").Select(x => x.Id).FirstAsync();

        if (await db.Sites.IgnoreQueryFilters().AnyAsync(x => x.SiteKey == DedicatedSiteKey))
        {
            return;
        }

        var siteId = Guid.NewGuid();
        db.Sites.Add(new Site
        {
            Id = siteId,
            TenantId = tenantId,
            Name = DedicatedBrand,
            SiteKey = DedicatedSiteKey,
            WebsiteType = WebsiteType.School,
            HomeVariant = HomeVariant.Modern,
            IsDefault = false,
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "test"
        });
        db.TenantDomains.Add(new TenantDomain
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            SiteId = siteId,
            DomainName = DedicatedHost,
            IsPrimary = true,
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "test"
        });
        // A website provisioned through the console always gets the standard page set; the
        // fixture mirrors that so the crawl exercises a realistic site.
        db.Pages.AddRange(
            NewPage(tenantId, siteId, PageType.About, "About us", "about", 1),
            NewPage(tenantId, siteId, PageType.Admission, "Admission", "admission", 2),
            NewPage(tenantId, siteId, PageType.Contact, "Contact", "contact", 3));

        await db.SaveChangesAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Theory]
    [InlineData("/school/about", "Cambridge High School")]
    [InlineData("/college/about", "Cambridge College of Arts")]
    public async Task SharedDomain_SitePrefixResolvesCorrectWebsite(string path, string expectedBrand)
    {
        using var response = await _client.GetAsync(path);
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains(expectedBrand, html);
    }

    [Fact]
    public async Task SharedDomain_NavigationKeepsSitePrefix()
    {
        using var response = await _client.GetAsync("/school/about");
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("href=\"/school/", html);
    }

    /// <summary>
    /// A school on its own domain must work with any site key — not only "school"/"college" —
    /// and must serve clean root URLs rather than a redundant prefix.
    /// </summary>
    [Fact]
    public async Task DedicatedDomain_ServesRootUrlsForAnySiteKey()
    {
        using var response = await GetAsync(DedicatedHost, "/about");
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();

        Assert.Contains(DedicatedBrand, html);
        Assert.DoesNotContain($"href=\"/{DedicatedSiteKey}/", html);
    }

    [Fact]
    public async Task DedicatedDomain_ServesHomePage()
    {
        using var response = await GetAsync(DedicatedHost, "/");
        response.EnsureSuccessStatusCode();
        Assert.Contains(DedicatedBrand, await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task UnknownSlug_ReturnsBrandedNotFoundPage()
    {
        using var response = await GetAsync(DedicatedHost, "/no-such-page");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains("Page not found", await response.Content.ReadAsStringAsync());
    }

    /// <summary>
    /// The contact form limiter shares an endpoint with page reads, so a visitor browsing
    /// several pages must never be throttled.
    /// </summary>
    [Fact]
    public async Task BrowsingManyPages_IsNotRateLimited()
    {
        for (var i = 0; i < 12; i++)
        {
            using var response = await _client.GetAsync("/school/about");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }

    [Fact]
    public async Task Sitemap_IsServedPerWebsite()
    {
        using var response = await GetAsync(DedicatedHost, "/sitemap.xml");
        response.EnsureSuccessStatusCode();
        var xml = await response.Content.ReadAsStringAsync();
        Assert.Contains("<urlset", xml);
        Assert.Contains($"http://{DedicatedHost}/about", xml);
    }

    /// <summary>
    /// Views emit asset links relative to the path base, so a shared-domain request must be able
    /// to fetch them through the site prefix as well as at the root.
    /// </summary>
    [Theory]
    [InlineData("/school/css/site.css")]
    [InlineData("/css/site.css")]
    public async Task StyleSheet_IsServedWithAndWithoutSitePrefix(string path)
    {
        using var response = await _client.GetAsync(path);
        response.EnsureSuccessStatusCode();
        Assert.Equal("text/css", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task UnmappedHost_IsRefused()
    {
        using var response = await GetAsync("not-a-tenant.test", "/");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Health_IsReachableWithoutATenant()
    {
        using var response = await GetAsync("not-a-tenant.test", "/health");
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Responses_CarrySecurityHeaders()
    {
        using var response = await GetAsync(DedicatedHost, "/");
        Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").Single());
        Assert.True(response.Headers.Contains("Content-Security-Policy"));
        Assert.True(response.Headers.Contains("Referrer-Policy"));
    }

    /// <summary>
    /// The bare site-key URL — the address the console hands an operator the moment a website is
    /// created, and the target of the site's own "home" link. It regressed once because endpoint
    /// matching runs ahead of the tenant middleware, so "/school" was matched as a page slug and
    /// answered 404 while every deeper URL kept working.
    /// </summary>
    [Theory]
    [InlineData("/school", "Cambridge High School")]
    [InlineData("/school/", "Cambridge High School")]
    [InlineData("/college", "Cambridge College of Arts")]
    [InlineData("/college/", "Cambridge College of Arts")]
    public async Task SharedDomain_BareSiteKeyServesThatWebsitesHomePage(string path, string expectedBrand)
    {
        using var response = await _client.GetAsync(path);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(expectedBrand, await response.Content.ReadAsStringAsync());
    }

    /// <summary>
    /// A domain bound to one website must not hide the tenant's other websites: the platform's
    /// own host has a domain row against it, and binding it once made every /{siteKey} URL serve
    /// the bound site and 404.
    /// </summary>
    [Theory]
    [InlineData("/school", "Cambridge High School")]
    [InlineData("/college/about", "Cambridge College of Arts")]
    public async Task DedicatedDomain_StillServesOtherWebsitesByPrefix(string path, string expectedBrand)
    {
        using var response = await GetAsync(DedicatedHost, path);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(expectedBrand, await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task DedicatedDomain_ServesItsOwnSiteKeyPrefix()
    {
        using var response = await GetAsync(DedicatedHost, $"/{DedicatedSiteKey}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(DedicatedBrand, await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task UnknownSiteKey_ReturnsNotFound()
    {
        using var response = await _client.GetAsync("/no-such-website");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    /// <summary>
    /// Every link every public page renders must lead somewhere. This is the check that would
    /// have caught the 404 loop, where the header, the footer and the 404 page's own "back home"
    /// button all pointed at a URL that answered 404 — and it catches seeded call-to-action
    /// buttons aimed at pages that were never created.
    /// </summary>
    [Theory]
    [InlineData("/school")]
    [InlineData("/school/about")]
    [InlineData("/school/admission")]
    [InlineData("/school/gallery")]
    [InlineData("/school/contact")]
    [InlineData("/school/news")]
    [InlineData("/school/events")]
    [InlineData("/school/faculty")]
    [InlineData("/school/departments")]
    [InlineData("/college")]
    [InlineData("/college/about")]
    [InlineData("/no-such-website")]
    public async Task SharedDomain_EveryLinkOnEveryPageResolves(string path)
    {
        await AssertNoBrokenLinksAsync(_client.BaseAddress!.Host, path);
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/about")]
    [InlineData("/no-such-page")]
    public async Task DedicatedDomain_EveryLinkOnEveryPageResolves(string path)
    {
        await AssertNoBrokenLinksAsync(DedicatedHost, path);
    }

    private async Task AssertNoBrokenLinksAsync(string host, string path)
    {
        using var page = await GetAsync(host, path);
        var html = await page.Content.ReadAsStringAsync();

        var links = System.Text.RegularExpressions.Regex
            .Matches(html, "href=\"(/[^\"#?]*)\"")
            .Select(match => match.Groups[1].Value)
            .Distinct()
            .ToList();

        Assert.NotEmpty(links);

        var broken = new List<string>();
        foreach (var link in links)
        {
            using var response = await GetAsync(host, link);
            if (!response.IsSuccessStatusCode)
            {
                broken.Add($"{link} -> {(int)response.StatusCode}");
            }
        }

        Assert.True(broken.Count == 0, $"Broken links on {host}{path}: " + string.Join(", ", broken));
    }

    private static Page NewPage(Guid tenantId, Guid siteId, PageType type, string title, string slug, int order) =>
        new()
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            SiteId = siteId,
            PageType = type,
            Title = title,
            Slug = slug,
            Content = $"<p>{title} for the Noida campus.</p>",
            ShowInMenu = true,
            MenuOrder = order,
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "test"
        };

    private Task<HttpResponseMessage> GetAsync(string host, string path)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"http://{host}{path}");
        return _client.SendAsync(request);
    }
}

/// <summary>
/// The deployed shape: the public website is mounted under a path base (<c>/site</c>) while the
/// console sits at the root of the same host. Links that omit the path base leave the
/// application altogether and hit whatever else the host serves — which is how the site came to
/// render correctly and then break the moment a visitor clicked anything in the header.
/// </summary>
public sealed class PathBasedRoutingTests : IClassFixture<PathBasedWebFactory>
{
    private readonly HttpClient _client;

    public PathBasedRoutingTests(PathBasedWebFactory factory) => _client = factory.CreateClient();

    [Theory]
    [InlineData("/site/school", "Cambridge High School")]
    [InlineData("/site/college", "Cambridge College of Arts")]
    [InlineData("/site/school/about", "Cambridge High School")]
    public async Task SiteIsServedUnderThePathBase(string path, string expectedBrand)
    {
        using var response = await _client.GetAsync(path);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(expectedBrand, await response.Content.ReadAsStringAsync());
    }

    [Theory]
    [InlineData("/site/school")]
    [InlineData("/site/college")]
    [InlineData("/site/school/about")]
    [InlineData("/site/school/contact")]
    public async Task EveryLinkKeepsThePathBaseAndResolves(string path)
    {
        using var page = await _client.GetAsync(path);
        page.EnsureSuccessStatusCode();
        var html = await page.Content.ReadAsStringAsync();

        var links = System.Text.RegularExpressions.Regex
            .Matches(html, "href=\"(/[^\"#?]*)\"")
            .Select(match => match.Groups[1].Value)
            .Distinct()
            .ToList();

        Assert.NotEmpty(links);

        var escaped = links.Where(link => !link.StartsWith("/site/", StringComparison.Ordinal)).ToList();
        Assert.True(escaped.Count == 0, "Links that drop the path base: " + string.Join(", ", escaped));

        var broken = new List<string>();
        foreach (var link in links)
        {
            using var response = await _client.GetAsync(link);
            if (!response.IsSuccessStatusCode)
            {
                broken.Add($"{link} -> {(int)response.StatusCode}");
            }
        }

        Assert.True(broken.Count == 0, $"Broken links on {path}: " + string.Join(", ", broken));
    }
}

public sealed class PathBasedWebFactory : WebApplicationFactory<WebProgram>
{
    private readonly string _databaseName = $"cms-web-pathbase-{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, configuration) =>
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["PathBase"] = "/site",
                ["Seed:EnableDemoData"] = "true",
                ["Seed:DemoAdminPassword"] = "Admin@12345",
                ["Database:ApplyMigrationsOnStartup"] = "false",
                ["ConnectionStrings:DefaultConnection"] = "Server=test-only",
                ["Storage:Provider"] = "Local",
                ["Storage:LocalRootPath"] = Path.Combine(Path.GetTempPath(), _databaseName, "uploads"),
                ["Tenancy:ResolutionCacheSeconds"] = "0"
            }));
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<ApplicationDbContext>();
            services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(_databaseName));
        });
    }
}

public sealed class PublicWebFactory : WebApplicationFactory<WebProgram>
{
    private readonly string _databaseName = $"cms-web-tests-{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, configuration) =>
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Seed:EnableDemoData"] = "true",
                ["Seed:DemoAdminPassword"] = "Admin@12345",
                ["Database:ApplyMigrationsOnStartup"] = "false",
                ["ConnectionStrings:DefaultConnection"] = "Server=test-only",
                ["Storage:Provider"] = "Local",
                ["Storage:LocalRootPath"] = Path.Combine(Path.GetTempPath(), _databaseName, "uploads"),
                // Hosts added mid-test must resolve immediately.
                ["Tenancy:ResolutionCacheSeconds"] = "0"
            }));
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<ApplicationDbContext>();
            services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(_databaseName));
        });
    }
}
