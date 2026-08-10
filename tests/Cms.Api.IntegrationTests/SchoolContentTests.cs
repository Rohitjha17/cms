extern alias webapp;

using System.Net;

namespace Cms.Api.IntegrationTests;

/// <summary>
/// Faculty, news, events and site settings must reach the public website. Previously this
/// content could be entered in the CMS and was never rendered anywhere.
/// </summary>
public sealed class SchoolContentTests : IClassFixture<PublicWebFactory>
{
    private readonly HttpClient _client;

    public SchoolContentTests(PublicWebFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task FacultyPage_ListsStaffGroupedByRole()
    {
        using var response = await _client.GetAsync("/school/faculty");
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();

        Assert.Contains("Dr. Anita Rao", html);
        Assert.Contains("Principal", html);
        Assert.Contains("Leadership", html);
        Assert.Contains("Teaching faculty", html);
        Assert.Contains("Vikram Mehta", html);
    }

    [Fact]
    public async Task NewsPage_ListsPublishedItemsFeaturedFirst()
    {
        using var response = await _client.GetAsync("/school/news");
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();

        Assert.Contains("Admissions open for 2026", html);
        Assert.Contains("Winter break circular", html);

        // The featured notice must lead the listing.
        Assert.True(
            html.IndexOf("Admissions open for 2026", StringComparison.Ordinal)
            < html.IndexOf("Winter break circular", StringComparison.Ordinal),
            "Featured items should be listed first.");
    }

    [Fact]
    public async Task NewsArticle_HasItsOwnPage()
    {
        using var response = await _client.GetAsync("/school/news/winter-break-circular");
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();

        Assert.Contains("Winter break circular", html);
        Assert.Contains("reopens on 2 January", html);
    }

    [Fact]
    public async Task UnknownNewsKey_Returns404()
    {
        using var response = await _client.GetAsync("/school/news/no-such-notice");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task EventsPage_SeparatesUpcomingFromPast()
    {
        using var response = await _client.GetAsync("/school/events");
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();

        Assert.Contains("Upcoming", html);
        Assert.Contains("Annual Day 2026", html);
        Assert.Contains("Admissions open day", html);

        Assert.Contains("Past events", html);
        Assert.Contains("Inter-house sports meet", html);

        // A finished event must not be promoted above the upcoming ones.
        Assert.True(
            html.IndexOf("Admissions open day", StringComparison.Ordinal)
            < html.IndexOf("Inter-house sports meet", StringComparison.Ordinal),
            "Upcoming events should precede past events.");
    }

    [Fact]
    public async Task Event_HasItsOwnPageWithScheduleAndVenue()
    {
        using var response = await _client.GetAsync("/school/events/annual-day-2026");
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();

        Assert.Contains("Annual Day 2026", html);
        Assert.Contains("School auditorium", html);
    }

    [Fact]
    public async Task SiteSettings_DriveTheAnnouncementBarAndSocialLinks()
    {
        using var response = await _client.GetAsync("/school");
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();

        Assert.Contains("notice-bar", html);
        Assert.Contains("Admissions for 2026", html);
        Assert.Contains("https://instagram.com/demoacademy", html);
    }

    [Fact]
    public async Task DepartmentsPage_ListsProgrammesOffered()
    {
        using var response = await _client.GetAsync("/school/departments");
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();

        Assert.Contains("Science", html);
        Assert.Contains("Vikram Mehta", html);
        Assert.Contains("Computer Science", html);
        Assert.Contains("Commerce", html);
    }

    [Fact]
    public async Task IndexableSite_AllowsCrawlersAndAdvertisesItsSitemap()
    {
        using var robots = await _client.GetAsync("/school/robots.txt");
        robots.EnsureSuccessStatusCode();
        var body = await robots.Content.ReadAsStringAsync();

        Assert.Contains("Allow: /", body);
        Assert.Contains("sitemap.xml", body);

        using var page = await _client.GetAsync("/school");
        Assert.DoesNotContain("noindex", await page.Content.ReadAsStringAsync());
    }

    /// <summary>
    /// Output caching keys on host + path, and the site prefix lives in PathBase — which is
    /// not part of the default key. If that is ever dropped, one school would be served
    /// another school's cached page. This asserts it cannot happen.
    /// </summary>
    [Fact]
    public async Task CachedPages_AreNeverSharedBetweenWebsites()
    {
        // Prime the cache for one site, then read the other on the same host.
        using var schoolFirst = await _client.GetAsync("/school/faculty");
        schoolFirst.EnsureSuccessStatusCode();

        using var college = await _client.GetAsync("/college/faculty");
        college.EnsureSuccessStatusCode();
        var collegeHtml = await college.Content.ReadAsStringAsync();

        Assert.Contains("Cambridge College of Arts", collegeHtml);
        Assert.DoesNotContain("Cambridge High School", collegeHtml);

        // And the first site is still itself on a repeat (cached) read.
        using var schoolAgain = await _client.GetAsync("/school/faculty");
        var schoolHtml = await schoolAgain.Content.ReadAsStringAsync();
        Assert.Contains("Cambridge High School", schoolHtml);
        Assert.DoesNotContain("Cambridge College of Arts", schoolHtml);
    }

    /// <summary>Content is per-website, so one site's entries must not leak into another's.</summary>
    [Fact]
    public async Task ContentIsScopedToTheWebsiteBeingViewed()
    {
        using var school = await _client.GetAsync("/school/faculty");
        using var college = await _client.GetAsync("/college/faculty");
        school.EnsureSuccessStatusCode();
        college.EnsureSuccessStatusCode();

        Assert.Contains("Cambridge High School", await school.Content.ReadAsStringAsync());
        Assert.Contains("Cambridge College of Arts", await college.Content.ReadAsStringAsync());
    }
}
