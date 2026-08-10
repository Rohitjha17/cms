using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Cms.Domain.Entities;
using Cms.Domain.Enums;
using Cms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Cms.Api.IntegrationTests;

/// <summary>
/// The whole product rests on one institution never seeing another's data, so that
/// promise is asserted directly rather than inferred from the middleware source.
///
/// A second tenant is inserted alongside the seeded demo tenant and every check is
/// driven through the HTTP boundary using the Host header, exactly as production does.
/// </summary>
public sealed class TenantIsolationTests : IClassFixture<CmsApiFactory>, IAsyncLifetime
{
    private const string RivalHost = "rival-academy.test";
    private const string RivalMarker = "RIVAL-ONLY-CONTENT-MARKER";
    private const string RivalTenantCode = "rival";

    private readonly CmsApiFactory _factory;
    private readonly HttpClient _client;

    public TenantIsolationTests(CmsApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (await db.Tenants.IgnoreQueryFilters().AnyAsync(x => x.Code == RivalTenantCode))
        {
            return;
        }

        var tenantId = Guid.NewGuid();
        var siteId = Guid.NewGuid();

        db.Tenants.Add(new Tenant
        {
            Id = tenantId,
            Name = "Rival Academy",
            Code = RivalTenantCode,
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "test"
        });

        db.Sites.Add(new Site
        {
            Id = siteId,
            TenantId = tenantId,
            Name = "Rival Academy",
            SiteKey = "school",
            WebsiteType = WebsiteType.School,
            HomeVariant = HomeVariant.Modern,
            IsDefault = true,
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "test"
        });

        db.TenantDomains.Add(new TenantDomain
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            SiteId = siteId,
            DomainName = RivalHost,
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
            Title = "About Rival Academy",
            Slug = "about",
            Content = $"<p>{RivalMarker}</p>",
            ShowInMenu = true,
            MenuOrder = 1,
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "test"
        });

        db.ContactSubmissions.Add(new ContactSubmission
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            SiteId = siteId,
            Name = "Rival Parent",
            Email = "parent@rival-academy.test",
            Message = RivalMarker,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "test"
        });

        await db.SaveChangesAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task SameSlugOnTwoHosts_ServesEachTenantsOwnPage()
    {
        var demo = await GetStringAsync("http://localhost/api/pages/about");
        var rival = await GetStringAsync($"http://{RivalHost}/api/pages/about");

        Assert.Contains(RivalMarker, rival);
        Assert.DoesNotContain(RivalMarker, demo);
    }

    [Fact]
    public async Task PageList_IsScopedToTheResolvedHost()
    {
        var rivalSlugs = await GetPageSlugsAsync($"http://{RivalHost}/api/pages");
        var demoSlugs = await GetPageSlugsAsync("http://localhost/api/pages");

        Assert.Equal(new[] { "about" }, rivalSlugs);

        // The demo tenant is seeded with the full starter set; none of it may bleed across.
        Assert.True(demoSlugs.Count > 1, "Expected the seeded demo tenant to expose starter pages.");
        Assert.Contains("admission", demoSlugs);
        Assert.DoesNotContain("admission", rivalSlugs);
    }

    [Fact]
    public async Task TokenFromOneTenant_IsRejectedOnAnotherTenantsHost()
    {
        var token = await LoginAsync("http://localhost", "admin@demo.local", "Admin@12345");

        using var request = new HttpRequestMessage(HttpMethod.Get, $"http://{RivalHost}/api/websites/contacts");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        using var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task TokenFromOneTenant_CannotWriteIntoAnotherTenant()
    {
        var token = await LoginAsync("http://localhost", "admin@demo.local", "Admin@12345");

        using var request = new HttpRequestMessage(HttpMethod.Post, $"http://{RivalHost}/api/pages");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = JsonContent.Create(new
        {
            title = "Injected page",
            slug = "injected-page",
            pageType = (int)PageType.Custom,
            isActive = true
        });

        using var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var leaked = await db.Pages.IgnoreQueryFilters().AnyAsync(x => x.Slug == "injected-page");
        Assert.False(leaked, "A cross-tenant write reached the database.");
    }

    [Fact]
    public async Task ContactSubmissions_AreNeverVisibleToAnotherTenantsAdministrator()
    {
        var token = await LoginAsync("http://localhost", "admin@demo.local", "Admin@12345");

        using var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/websites/contacts");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Add("X-Site-Key", "school");
        using var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain(RivalMarker, body);
    }

    [Fact]
    public async Task UnmappedHost_IsRefusedRatherThanFallingBackToAnyTenant()
    {
        using var response = await _client.GetAsync("http://not-a-configured-host.test/api/pages");

        // DemoMode is off in tests, so an unknown host must not resolve to a tenant at all.
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<string> GetStringAsync(string url)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("X-Site-Key", "school");
        using var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    private async Task<List<string>> GetPageSlugsAsync(string url)
    {
        using var document = JsonDocument.Parse(await GetStringAsync(url));
        return document.RootElement
            .GetProperty("data")
            .EnumerateArray()
            .Select(x => x.GetProperty("slug").GetString()!)
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToList();
    }

    private async Task<string> LoginAsync(string origin, string email, string password)
    {
        using var response = await _client.PostAsJsonAsync($"{origin}/api/auth/login", new { email, password });
        response.EnsureSuccessStatusCode();
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return json.RootElement.GetProperty("data").GetProperty("token").GetString()!;
    }
}
