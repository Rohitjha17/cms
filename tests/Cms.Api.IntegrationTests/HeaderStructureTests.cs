extern alias webapp;

using System.Text.Json;
using Cms.Application.DTOs.SchoolContent;
using Cms.Domain.Constants;
using Cms.Domain.Entities;
using Cms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Cms.Api.IntegrationTests;

/// <summary>
/// The parts of a school header that are not navigation: the label on the notice strip, the
/// phone and email a parent came for, one call to action, and the row of portals above it all.
///
/// Each is optional, and the interesting case for every one of them is the empty case — a school
/// that leaves the field blank must get nothing at all, not an empty box or a dead link.
/// </summary>
public sealed class HeaderStructureTests : IClassFixture<PublicWebFactory>
{
    private readonly PublicWebFactory _factory;
    private readonly HttpClient _client;

    public HeaderStructureTests(PublicWebFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task TheNoticeStrip_CarriesTheLabelASchoolChose()
    {
        await SaveSettingsAsync(new()
        {
            ["noticeTicker"] = "Admissions close on 28 February.",
            ["noticeLabel"] = "NOTICE"
        });

        var html = await _client.GetStringAsync("/");

        Assert.Contains("notice-bar__tag", html);
        Assert.Contains(">NOTICE<", html);
    }

    [Fact]
    public async Task NoLabel_LeavesTheStripWithoutOne()
    {
        await SaveSettingsAsync(new() { ["noticeTicker"] = "Admissions close on 28 February." });

        var html = await _client.GetStringAsync("/");

        Assert.Contains("notice-bar", html);
        Assert.DoesNotContain("notice-bar__tag", html);
    }

    [Fact]
    public async Task TheHeader_ShowsThePhoneAndEmailWhenAsked()
    {
        await SaveSettingsAsync(new() { ["headerContact"] = true });

        var html = await _client.GetStringAsync("/");

        Assert.Contains("header-contact", html);
        Assert.Contains("Call us", html);
        Assert.Contains("Mail us", html);
    }

    [Fact]
    public async Task WithoutThatSwitch_TheHeaderStaysAsItWas()
    {
        await SaveSettingsAsync([]);

        Assert.DoesNotContain("header-contact", await _client.GetStringAsync("/"));
    }

    [Fact]
    public async Task TheHeaderButton_UsesTheTextAndLinkGiven()
    {
        await SaveSettingsAsync(new()
        {
            ["headerCtaText"] = "Admission Enquiry",
            ["headerCtaLink"] = "/admission-2027"
        });

        var html = await _client.GetStringAsync("/");

        Assert.Contains("header-cta", html);
        Assert.Contains("Admission Enquiry", html);
        Assert.Contains("/admission-2027", html);
    }

    [Fact]
    public async Task NoButtonText_MeansNoButton()
    {
        await SaveSettingsAsync(new() { ["headerCtaLink"] = "/admission" });

        Assert.DoesNotContain("header-cta", await _client.GetStringAsync("/"));
    }

    [Fact]
    public async Task TheStripAboveTheHeader_ListsThePortalsGiven()
    {
        await SaveSettingsAsync(new()
        {
            ["topBarLinks"] = "Alumni|/alumni\nParent Portal|https://portal.example.in\nCBSE Disclosures|/disclosure"
        });

        var html = await _client.GetStringAsync("/");

        Assert.Contains("top-meta__utility", html);
        Assert.Contains("https://portal.example.in", html);
        Assert.Contains("CBSE Disclosures", html);
    }

    /// <summary>
    /// A label with no link behind it looks like a link and does nothing, which is worse than
    /// not being there. It is dropped rather than rendered.
    /// </summary>
    [Fact]
    public async Task ALineWithNoLink_IsDroppedRatherThanRenderedDead()
    {
        await SaveSettingsAsync(new() { ["topBarLinks"] = "Alumni|/alumni\nComing soon" });

        var html = await _client.GetStringAsync("/");

        Assert.Contains("/alumni", html);
        Assert.DoesNotContain("Coming soon", html);
    }

    [Fact]
    public void TheLinkList_KeepsOnlyWellFormedLines()
    {
        var settings = new SiteSettingsDto
        {
            TopBarLinks = "  Alumni | /alumni  \n\nNo link here\n|/orphan\nLabel|\nPortal|https://a.b"
        };

        Assert.Equal(
            [("Alumni", "/alumni"), ("Portal", "https://a.b")],
            settings.TopBarLinkList);
    }

    /// <summary>
    /// A template that describes a header nobody gets when the site is created describes
    /// nothing. Both replica templates are checked, because the point of them is that their
    /// headers differ: one leads with the phone, the other with a row of portals.
    /// </summary>
    [Theory]
    [InlineData("notice-board-school", "NOTICE", true, false)]
    [InlineData("campus-prospectus", "LATEST", false, true)]
    public void ATemplatesHeader_TravelsWithIt(
        string key, string label, bool contactInHeader, bool portalsAbove)
    {
        var template = Cms.Application.Templates.SiteTemplateCatalog.Find(key);

        Assert.NotNull(template);
        Assert.Equal(label, template.Settings["noticeLabel"]);
        Assert.Equal(contactInHeader, template.Settings.TryGetValue("headerContact", out var c) && (bool)c!);
        Assert.Equal(portalsAbove, template.Settings.ContainsKey("topBarLinks"));
        Assert.Equal("Admission Enquiry", template.Settings["headerCtaText"]);
    }

    /// <summary>
    /// Settings written as a JSON object have to survive being read back through the same
    /// parser the console uses, or the template's header is a string nothing renders.
    /// </summary>
    [Theory]
    [InlineData("notice-board-school")]
    [InlineData("campus-prospectus")]
    public void ATemplatesSettings_ReadBackAsTheSettingsTheyDescribe(string key)
    {
        var template = Cms.Application.Templates.SiteTemplateCatalog.Find(key)!;

        var json = new System.Text.Json.Nodes.JsonObject();
        foreach (var (name, value) in template.Settings)
        {
            json[name] = value switch
            {
                bool flag => System.Text.Json.Nodes.JsonValue.Create(flag),
                int number => System.Text.Json.Nodes.JsonValue.Create(number),
                _ => System.Text.Json.Nodes.JsonValue.Create(value?.ToString())
            };
        }

        var settings = System.Text.Json.JsonSerializer.Deserialize<SiteSettingsDto>(
            json.ToJsonString(),
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        Assert.False(string.IsNullOrWhiteSpace(settings.NoticeLabel));
        Assert.Equal("Admission Enquiry", settings.HeaderCtaText);
        if (key == "campus-prospectus")
        {
            Assert.Equal(4, settings.TopBarLinkList.Count);
            Assert.Contains(("Parent Portal", "/parent-portal"), settings.TopBarLinkList);
        }
    }

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
}
