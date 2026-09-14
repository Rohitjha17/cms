extern alias webapp;

using Cms.Application.DTOs.SchoolContent;
using Cms.Domain.Constants;
using Cms.Domain.Entities;
using Cms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using ReadableInk = webapp::Cms.Web.Helpers.ReadableInk;

namespace Cms.Api.IntegrationTests;

/// <summary>
/// Sections without a renderer of their own — courses, departments, events, achievements,
/// testimonials, partners — were drawn by one generic block that printed a heading and a
/// paragraph and nothing else. So the picture a school uploaded against each row never
/// appeared, a testimonial showed no name because testimonials carry "name" and not "title",
/// and the page address on a course led nowhere because no link was drawn.
/// </summary>
public sealed class CardContentTests : IClassFixture<PublicWebFactory>
{
    private readonly PublicWebFactory _factory;
    private readonly HttpClient _client;

    public CardContentTests(PublicWebFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ACoursePicture_ReachesThePage()
    {
        await SaveAsync(HomePageSectionKeys.Courses, "Courses", new
        {
            items = new[]
            {
                new { title = "Science", description = "Inquiry-led.", imageUrl = "/uploads/science.jpg", url = "/courses/science" }
            }
        });

        var html = await _client.GetStringAsync("/");

        Assert.Contains("/uploads/science.jpg", html);
        Assert.Contains("Science", html);
    }

    /// <summary>A testimonial has a name, not a title. Reading only "title" left it blank.</summary>
    [Fact]
    public async Task ATestimonialShowsWhoSaidIt()
    {
        await SaveAsync(HomePageSectionKeys.Testimonials, "What families say", new
        {
            items = new[]
            {
                new { name = "Meena Sharma", role = "Parent", quote = "The teachers know my daughter." }
            }
        });

        var html = await _client.GetStringAsync("/");

        Assert.Contains("Meena Sharma", html);
        Assert.Contains("The teachers know my daughter.", html);
        Assert.Contains("Parent", html);
    }

    /// <summary>
    /// A link typed as "/departments" has to keep the path this website is served under, or it
    /// lands on another school's page — or on nothing at all.
    /// </summary>
    [Fact]
    public async Task ACardsLink_KeepsThePathBase()
    {
        await SaveAsync(HomePageSectionKeys.Departments, "Departments", new
        {
            items = new[] { new { title = "Science", url = "/departments" } }
        });

        var html = await _client.GetStringAsync("/school");

        Assert.Contains("href=\"/school/departments\"", html);
        Assert.DoesNotContain("href=\"/departments\"", html);
    }

    /// <summary>
    /// Buttons were drawn with a dark ink hard-coded against the school's accent. A school whose
    /// second colour is navy or maroon got dark text on a dark button — the "Send message"
    /// button that reads as a solid black bar with the words invisible inside it.
    /// </summary>
    [Theory]
    [InlineData("#0a438d", "#ffffff")]   // navy
    [InlineData("#991f22", "#ffffff")]   // maroon
    [InlineData("#000000", "#ffffff")]
    [InlineData("#ff3115", "#131b2b")]   // the bright accent it was written for
    [InlineData("#c9a227", "#131b2b")]   // gold
    [InlineData("#ffffff", "#131b2b")]
    [InlineData("#fff", "#131b2b")]      // short form
    [InlineData("not a colour", "#131b2b")]
    [InlineData(null, "#131b2b")]
    public void TheInkOnAButton_IsLegibleOnTheSchoolsOwnColour(string? background, string expected)
        => Assert.Equal(expected, ReadableInk.On(background));

    /// <summary>
    /// An event carries a picture, and the school uploads one against every event it runs. The
    /// home page drew the date, the title and the venue, and dropped the picture on the floor —
    /// so a school that had filled in all three saw a wall of text where its photographs were.
    /// The same held for news.
    /// </summary>
    [Fact]
    public async Task AnEventsPicture_ReachesTheHomePage()
    {
        await SaveEntryAsync(SchoolContentTypes.Event, "sports-day", "Sports Day",
            "/uploads/sports-day.jpg", "Track and field on the main ground.");

        var html = await _client.GetStringAsync("/");

        Assert.Contains("/uploads/sports-day.jpg", html);
        Assert.Contains("Sports Day", html);
        Assert.Contains("Track and field on the main ground.", html);
    }

    [Fact]
    public async Task ANewsPicture_ReachesTheHomePage()
    {
        await SaveEntryAsync(SchoolContentTypes.News, "prize-day", "Prize Day",
            "/uploads/prize-day.jpg", "Certificates were given out on Friday.");

        var html = await _client.GetStringAsync("/");

        Assert.Contains("/uploads/prize-day.jpg", html);
        Assert.Contains("Prize Day", html);
    }

    private async Task SaveEntryAsync(string contentType, string key, string title, string imageUrl, string summary)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var site = await db.Sites.IgnoreQueryFilters().FirstAsync(x => x.SiteKey == "school");

        var entry = await db.ContentEntries.IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.SiteId == site.Id && x.ContentType == contentType && x.Key == key);

        if (entry is null)
        {
            entry = new ContentEntry
            {
                TenantId = site.TenantId,
                SiteId = site.Id,
                ContentType = contentType,
                Key = key
            };
            db.ContentEntries.Add(entry);
        }

        entry.Title = title;
        entry.Summary = summary;
        entry.ImageUrl = imageUrl;
        entry.IsActive = true;
        entry.DisplayOrder = 0;
        entry.PublishDate = DateTime.UtcNow.AddDays(30);

        await db.SaveChangesAsync();
    }

    private async Task SaveAsync(string key, string title, object json)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var site = await db.Sites.IgnoreQueryFilters().FirstAsync(x => x.SiteKey == "school");

        var section = await db.HomePageSections.IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.SiteId == site.Id && x.SectionKey == key);

        var payload = JsonSerializer.Serialize(json);
        if (section is null)
        {
            db.HomePageSections.Add(new HomePageSection
            {
                TenantId = site.TenantId,
                SiteId = site.Id,
                SectionKey = key,
                Title = title,
                JsonData = payload,
                IsActive = true,
                DisplayOrder = 60
            });
        }
        else
        {
            section.Title = title;
            section.JsonData = payload;
            section.IsActive = true;
        }

        await db.SaveChangesAsync();
    }
}
