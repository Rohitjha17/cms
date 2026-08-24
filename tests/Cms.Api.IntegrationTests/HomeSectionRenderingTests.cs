extern alias webapp;

using System.Net;
using Cms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Cms.Api.IntegrationTests;

/// <summary>
/// Everything the console can edit has to appear on the website.
///
/// Each home design used to lay out four to six sections and quietly ignore the rest, so most of
/// what an editor filled in was never rendered anywhere: the principal's message, the chairman's
/// message, announcements, testimonials and more. Saving worked perfectly and the website simply
/// never changed, which is indistinguishable from the save being broken.
/// </summary>
public sealed class HomeSectionRenderingTests : IClassFixture<PublicWebFactory>, IAsyncLifetime
{
    private const string Marker = "ZZSECTION";
    private readonly PublicWebFactory _factory;
    private readonly HttpClient _client;

    public HomeSectionRenderingTests(PublicWebFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    /// <summary>Stamps every one of the school's sections with a mark naming itself.</summary>
    public async Task InitializeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var siteId = await db.Sites.IgnoreQueryFilters()
            .Where(x => x.SiteKey == "school").Select(x => x.Id).FirstAsync();

        var sections = await db.HomePageSections.IgnoreQueryFilters()
            .Where(x => x.SiteId == siteId).ToListAsync();

        foreach (var section in sections)
        {
            section.IsActive = true;
            section.Description = $"<p>{Marker}-{section.SectionKey}</p>";
        }

        await db.SaveChangesAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task EveryActiveSection_AppearsOnTheWebsite()
    {
        using var response = await _client.GetAsync("/school");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var siteId = await db.Sites.IgnoreQueryFilters()
            .Where(x => x.SiteKey == "school").Select(x => x.Id).FirstAsync();
        var keys = await db.HomePageSections.IgnoreQueryFilters()
            .Where(x => x.SiteId == siteId && x.IsActive)
            .Select(x => x.SectionKey)
            .ToListAsync();

        Assert.NotEmpty(keys);

        // Presence is what matters, not the description: statistics show numbers, a gallery shows
        // pictures, announcements show the school's notices. Each rendered section names itself,
        // so a section that stops appearing is caught rather than quietly vanishing.
        var missing = keys
            .Where(key => !html.Contains($"data-section=\"{key}\"", StringComparison.Ordinal))
            .ToList();

        Assert.True(
            missing.Count == 0,
            $"{missing.Count} of {keys.Count} sections never reach the website: {string.Join(", ", missing)}");
    }

    /// <summary>
    /// A notice added under News and notices belongs on the home page too, not only on /news.
    /// The home page used to show a list typed into the section's own configuration instead.
    /// </summary>
    [Fact]
    public async Task ANoticeAddedToTheSchool_ShowsOnTheHomePage()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var site = await db.Sites.IgnoreQueryFilters().FirstAsync(x => x.SiteKey == "school");

        db.ContentEntries.Add(new Cms.Domain.Entities.ContentEntry
        {
            Id = Guid.NewGuid(),
            TenantId = site.TenantId,
            SiteId = site.Id,
            ContentType = "news",
            Key = "zz-fresh-notice",
            Title = "ZZNOTICE school closed on Friday",
            Summary = "A one-day closure for the staff conference.",
            PublishDate = DateTime.UtcNow,
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "test"
        });
        await db.SaveChangesAsync();

        var home = await _client.GetStringAsync("/school");
        Assert.Contains("ZZNOTICE", home);
    }
}
