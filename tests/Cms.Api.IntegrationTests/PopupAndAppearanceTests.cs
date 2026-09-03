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
        // The copies that close the loop are made in the browser, against the measured width.
        // The page itself must carry the notice exactly once, or a reader without JavaScript —
        // and every screen reader — hears it twice.
        Assert.Equal(1, System.Text.RegularExpressions.Regex.Matches(html, "Admissions are open").Count);
    }

    /// <summary>
    /// A school runs more than one notice at a time. Separating them has to produce separate
    /// notices, not one line with a stray bar in the middle of it.
    /// </summary>
    [Fact]
    public async Task SeveralNotices_AreSeparateItems()
    {
        await SaveSettingsAsync(new Dictionary<string, object?>
        {
            ["noticeTicker"] = "Admissions open | Results on 12 May | Apply online",
            ["noticeTickerScrolls"] = true
        });

        var html = await _client.GetStringAsync("/");

        Assert.Contains("<span>Admissions open</span>", html);
        Assert.Contains("<span>Results on 12 May</span>", html);
        Assert.Contains("<span>Apply online</span>", html);
        Assert.DoesNotContain("Admissions open |", html);
    }

    /// <summary>
    /// A long notice at the pace that suited a short one runs past before it can be read, so the
    /// speed has to be the school's to set — and has to survive being set absurdly.
    /// </summary>
    [Theory]
    [InlineData(45, "--notice-speed:45s")]
    [InlineData(9999, "--notice-speed:120s")]
    [InlineData(1, "--notice-speed:5s")]
    public async Task TheChosenScrollSpeed_ReachesTheStrip(int seconds, string expected)
    {
        await SaveSettingsAsync(new Dictionary<string, object?>
        {
            ["noticeTicker"] = "Admissions are open",
            ["noticeTickerScrolls"] = true,
            ["noticeTickerSeconds"] = seconds
        });

        Assert.Contains(expected, await _client.GetStringAsync("/"));
    }

    [Fact]
    public async Task NoSpeedChosen_LeavesTheDefaultPaceAlone()
    {
        await SaveSettingsAsync(new Dictionary<string, object?>
        {
            ["noticeTicker"] = "Admissions are open",
            ["noticeTickerScrolls"] = true
        });

        var html = await _client.GetStringAsync("/");
        Assert.Contains("notice-marquee", html);
        Assert.DoesNotContain("--notice-speed", html);
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
