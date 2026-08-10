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
        db.Pages.Add(new Page
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            SiteId = siteId,
            PageType = PageType.About,
            Title = "About us",
            Slug = "about",
            Content = "<p>About the Noida campus.</p>",
            ShowInMenu = true,
            MenuOrder = 1,
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "test"
        });

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

    private Task<HttpResponseMessage> GetAsync(string host, string path)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"http://{host}{path}");
        return _client.SendAsync(request);
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
