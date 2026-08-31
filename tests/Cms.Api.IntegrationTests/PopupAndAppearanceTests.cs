extern alias webapp;

using System.Text.Json;
using Cms.Application.DTOs.SchoolContent;
using Cms.Domain.Entities;
using Cms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Cms.Api.IntegrationTests;

/// <summary>
/// The opening poster, the scrolling notice, the logo size and the entrance animations are all
/// settings, and a setting that saves but changes nothing on the website is the failure that is
/// hardest to notice: the console says "saved", so the natural conclusion is that the request
/// was wrong rather than that the website ignored it.
///
/// These check the whole way through — written as settings, read back on the public page.
/// </summary>
public sealed class PopupAndAppearanceTests : IClassFixture<PublicWebFactory>
{
    private readonly PublicWebFactory _factory;
    private readonly HttpClient _client;

    public PopupAndAppearanceTests(PublicWebFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task WithNothingConfigured_ThePageIsExactlyWhatItWas()
    {
        await SaveSettingsAsync(new Dictionary<string, object?>());
        var html = await _client.GetStringAsync("/");

        Assert.DoesNotContain("data-site-popup", html);
        Assert.DoesNotContain("notice-bar--scrolling", html);
        Assert.DoesNotContain("--logo-size", html);
        // Absent is not off: sites that already fade their sections in must keep doing so.
        Assert.Contains("data-scroll-animations=\"on\"", html);
    }

    [Fact]
    public async Task SwitchedOnWithAPoster_ThePopupIsOnThePage()
    {
        await SaveSettingsAsync(new Dictionary<string, object?>
        {
            ["popupEnabled"] = true,
            ["popupImageUrl"] = "/uploads/admissions-poster.jpg",
            ["popupHeading"] = "Admissions open 2027-28"
        });

        var html = await _client.GetStringAsync("/");

        Assert.Contains("data-site-popup", html);
        Assert.Contains("/uploads/admissions-poster.jpg", html);
        Assert.Contains("Admissions open 2027-28", html);
    }

    /// <summary>
    /// Switched on with nothing in it would open an empty white box over the website on every
    /// visit — worse than leaving it off.
    /// </summary>
    [Fact]
    public async Task SwitchedOnButEmpty_ShowsNothing()
    {
        await SaveSettingsAsync(new Dictionary<string, object?> { ["popupEnabled"] = true });
        Assert.DoesNotContain("data-site-popup", await _client.GetStringAsync("/"));
    }

    [Fact]
    public async Task TheEnquiryForm_ReachesTheContactInbox()
    {
        await SaveSettingsAsync(new Dictionary<string, object?>
        {
            ["popupEnabled"] = true,
            ["popupShowEnquiryForm"] = true
        });

        var page = await _client.GetStringAsync("/");
        Assert.Contains("/enquiry", page);

        // The form must carry an antiforgery token, or every real submission is rejected with a
        // 400 the visitor sees as the form simply not working.
        var token = System.Text.RegularExpressions.Regex
            .Match(page, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"")
            .Groups[1].Value;
        Assert.False(string.IsNullOrWhiteSpace(token), "The enquiry form carried no antiforgery token.");

        var before = await CountContactsAsync();
        using var response = await _client.PostAsync("/enquiry", new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["Input.Name"] = "Anita Rao",
                ["Input.Email"] = "anita@example.com",
                ["Input.Phone"] = "+91 98765 43210",
                ["Input.Message"] = "Please call about nursery admission."
            }));

        // The client follows the redirect back to the page the visitor was reading.
        Assert.True(response.IsSuccessStatusCode, $"The enquiry was refused: {response.StatusCode}.");
        Assert.Equal(before + 1, await CountContactsAsync());
    }

    [Fact]
    public async Task AScrollingNotice_MovesAndIsNotAnnouncedTwice()
    {
        await SaveSettingsAsync(new Dictionary<string, object?>
        {
            ["noticeTicker"] = "Admissions are open",
            ["noticeTickerScrolls"] = true
        });

        var html = await _client.GetStringAsync("/");

        Assert.Contains("notice-bar--scrolling", html);
        // The text is repeated to close the loop; the copy must be hidden from screen readers.
        Assert.Contains("aria-hidden=\"true\"", html);
    }

    [Fact]
    public async Task TheChosenLogoHeight_ReachesTheStylesheet()
    {
        await SaveSettingsAsync(new Dictionary<string, object?> { ["logoHeight"] = 96 });
        Assert.Contains("--logo-size:96px", await _client.GetStringAsync("/"));
    }

    /// <summary>An absurd value must not be able to push the header off the page.</summary>
    [Theory]
    [InlineData(4000, "--logo-size:160px")]
    [InlineData(2, "--logo-size:28px")]
    public async Task AnImpossibleLogoHeight_IsBroughtBackIntoRange(int height, string expected)
    {
        await SaveSettingsAsync(new Dictionary<string, object?> { ["logoHeight"] = height });
        Assert.Contains(expected, await _client.GetStringAsync("/"));
    }

    [Fact]
    public async Task AnimationsSwitchedOff_AreSaidSoOnThePage()
    {
        await SaveSettingsAsync(new Dictionary<string, object?> { ["scrollAnimations"] = false });
        Assert.Contains("data-scroll-animations=\"off\"", await _client.GetStringAsync("/"));
    }

    // ---------------------------------------------------------------- helpers

    private async Task SaveSettingsAsync(Dictionary<string, object?> values)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var site = await db.Sites.IgnoreQueryFilters().FirstAsync(x => x.SiteKey == "school");
        var entry = await db.ContentEntries.IgnoreQueryFilters().FirstOrDefaultAsync(
            x => x.SiteId == site.Id && x.ContentType == SchoolContentTypes.Setting
                && x.Key == SchoolContentTypes.SettingsKey);

        var json = JsonSerializer.Serialize(values);
        if (entry is null)
        {
            db.ContentEntries.Add(new ContentEntry
            {
                TenantId = site.TenantId,
                SiteId = site.Id,
                ContentType = SchoolContentTypes.Setting,
                Key = SchoolContentTypes.SettingsKey,
                Title = "Site settings",
                JsonData = json,
                IsActive = true
            });
        }
        else
        {
            entry.JsonData = json;
            entry.IsActive = true;
        }

        await db.SaveChangesAsync();
    }

    private async Task<int> CountContactsAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await db.ContactSubmissions.IgnoreQueryFilters().CountAsync();
    }
}
