using Cms.Domain.Enums;
using Cms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace Cms.Api.IntegrationTests;

/// <summary>
/// The gallery page groups its media into albums and takes photographs and film side by side.
///
/// The video half had a trap in it: a frame needs YouTube's *embed* address, and what a person
/// copies out of their address bar is the *watch* address, which YouTube refuses to be framed
/// with. A pasted link therefore produced an empty black box on the page and no clue why.
/// </summary>
public sealed class GalleryPageTests : IClassFixture<PublicWebFactory>
{
    private readonly PublicWebFactory _factory;
    private readonly HttpClient _client;

    public GalleryPageTests(PublicWebFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PhotographsAndFilmAreGroupedIntoTheAlbumsTheyWereGiven()
    {
        await SaveGalleryAsync(new
        {
            items = new object[]
            {
                new { album = "Annual Day", type = "image", url = "/uploads/prize.jpg", caption = "Prize giving" },
                new { album = "Sports", type = "image", url = "/uploads/race.jpg", caption = "Sports day" }
            }
        });

        var html = await _client.GetStringAsync("/gallery");

        Assert.Contains("<h2>Annual Day</h2>", html);
        Assert.Contains("<h2>Sports</h2>", html);
        Assert.Contains("/uploads/prize.jpg", html);
        Assert.Contains("Prize giving", html);
    }

    /// <summary>
    /// The address a person actually pastes, turned into one that plays.
    /// </summary>
    [Fact]
    public async Task AnOrdinaryYouTubeLink_BecomesAPlayerRatherThanAnEmptyBox()
    {
        await SaveGalleryAsync(new
        {
            items = new object[]
            {
                new { album = "Films", type = "video", url = "https://www.youtube.com/watch?v=dQw4w9WgXcQ", caption = "Annual day film" }
            }
        });

        var html = await _client.GetStringAsync("/gallery");

        Assert.Contains("youtube-nocookie.com/embed/dQw4w9WgXcQ", html);
        Assert.DoesNotContain("watch?v=", html);
    }

    /// <summary>
    /// A link this cannot play is left out rather than drawn as a broken frame.
    /// </summary>
    [Fact]
    public async Task ALinkThatIsNotAVideo_IsSkipped()
    {
        await SaveGalleryAsync(new
        {
            items = new object[]
            {
                new { album = "Films", type = "video", url = "https://example.org/not-a-video", caption = "Nothing" }
            }
        });

        var html = await _client.GetStringAsync("/gallery");

        Assert.DoesNotContain("example.org/not-a-video", html);
    }

    private async Task SaveGalleryAsync(object data)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var siteId = await db.Sites.IgnoreQueryFilters()
            .Where(x => x.SiteKey == "school").Select(x => x.Id).FirstAsync();

        var page = await db.Pages.IgnoreQueryFilters()
            .FirstAsync(x => x.SiteId == siteId && x.PageType == PageType.Gallery);

        page.JsonData = JsonSerializer.Serialize(data);
        page.IsActive = true;
        await db.SaveChangesAsync();
    }
}
