using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Cms.Api.IntegrationTests;

public sealed class HomePageApiTests : IClassFixture<CmsApiFactory>
{
    private readonly HttpClient _client;

    public HomePageApiTests(CmsApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PublicHomepage_ReturnsAllActiveSeededSections()
    {
        using var request = SiteRequest(HttpMethod.Get, "/api/homepage", "school");
        using var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal(21, json.RootElement.GetProperty("data").EnumerateObject().Count());
    }

    [Fact]
    public async Task Mutation_RequiresAuthentication()
    {
        using var request = SiteRequest(HttpMethod.Put, "/api/homepage/hero", "school");
        request.Content = JsonContent.Create(new { title = "Blocked anonymous update" });

        using var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AuthenticatedUpdate_RemainsIsolatedToSelectedSite()
    {
        var token = await LoginAsync();
        var uniqueTitle = $"School hero {Guid.NewGuid():N}";

        using (var update = SiteRequest(HttpMethod.Put, "/api/homepage/hero", "school"))
        {
            update.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            update.Content = JsonContent.Create(new
            {
                title = uniqueTitle,
                subTitle = "Tenant-isolated test",
                jsonData = """{"heading":"School only"}""",
                isActive = true,
                displayOrder = 1
            });
            using var updateResponse = await _client.SendAsync(update);
            Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        }

        var schoolTitle = await GetHeroTitleAsync("school");
        var collegeTitle = await GetHeroTitleAsync("college");
        Assert.Equal(uniqueTitle, schoolTitle);
        Assert.NotEqual(uniqueTitle, collegeTitle);
    }

    [Fact]
    public async Task UnknownSection_ReturnsWrappedNotFound()
    {
        using var request = SiteRequest(HttpMethod.Get, "/api/homepage/not-a-section", "school");
        using var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.False(json.RootElement.GetProperty("success").GetBoolean());
        Assert.Equal(404, json.RootElement.GetProperty("statusCode").GetInt32());
    }

    private async Task<string> LoginAsync()
    {
        using var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "admin@demo.local",
            password = "Admin@12345"
        });
        response.EnsureSuccessStatusCode();
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return json.RootElement.GetProperty("data").GetProperty("token").GetString()!;
    }

    private async Task<string> GetHeroTitleAsync(string site)
    {
        using var request = SiteRequest(HttpMethod.Get, "/api/homepage/hero", site);
        using var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return json.RootElement.GetProperty("data").GetProperty("title").GetString()!;
    }

    private static HttpRequestMessage SiteRequest(HttpMethod method, string uri, string site)
    {
        var request = new HttpRequestMessage(method, uri);
        request.Headers.Add("X-Site-Key", site);
        return request;
    }
}
