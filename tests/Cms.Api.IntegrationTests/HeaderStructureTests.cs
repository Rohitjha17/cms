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

    /// <summary>
    /// There is no strip above the header any more. The phone and email live in the header
    /// itself, the portals belong in the menu, and the social accounts are in the footer and on
    /// the contact page — so nothing is left for a band across the top of every page to carry.
    /// </summary>
    [Fact]
    public async Task NothingSitsAboveTheHeader()
    {
        await SaveSettingsAsync(new() { ["headerContact"] = true });

        var html = await _client.GetStringAsync("/");

        Assert.DoesNotContain("top-meta", html);
        // The header is still where it always was, and still carries the contact details.
        Assert.Contains("site-header", html);
        Assert.Contains("header-contact", html);
    }

    /// <summary>
    /// A template that describes a header nobody gets when the site is created describes
    /// nothing. Both replica templates are checked, and what separates them is asserted too:
    /// only one carries the row of portals above the header.
    /// </summary>
    [Theory]
    [InlineData("notice-board-school", "NOTICE")]
    [InlineData("campus-prospectus", "LATEST")]
    public void ATemplatesHeader_TravelsWithIt(string key, string label)
    {
        var template = Cms.Application.Templates.SiteTemplateCatalog.Find(key);

        Assert.NotNull(template);
        Assert.Equal(label, template.Settings["noticeLabel"]);
        Assert.True((bool)template.Settings["headerContact"]!);
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
        Assert.True(settings.HeaderContact);
    }

    /// <summary>
    /// The bulletin template's banners are photographs of the school, so its hero keeps the
    /// heading over them; the prospectus template's are posters that carry their own words, so
    /// its hero prints nothing. That difference is the point of the two, and it is easy to lose
    /// by copying settings from one to the other.
    /// </summary>
    [Fact]
    public void EachTemplateTreatsItsBannersAsWhatTheyAre()
    {
        var bulletin = Cms.Application.Templates.SiteTemplateCatalog.Find("notice-board-school")!;
        var prospectus = Cms.Application.Templates.SiteTemplateCatalog.Find("campus-prospectus")!;

        Assert.False(bulletin.Settings.ContainsKey("heroPlainImages"));
        Assert.True((bool)prospectus.Settings["heroPlainImages"]!);

        // Both ship a slideshow rather than one still picture, and no two share a photograph.
        Assert.True(bulletin.HeroImages.Count > 1);
        Assert.True(prospectus.HeroImages.Count > 1);
        Assert.Empty(bulletin.HeroImages.Intersect(prospectus.HeroImages));
    }

    /// <summary>
    /// A banner that is finished artwork already carries its own headline and button. Ours laid
    /// over the top gives the frame two of each, and the scrim that keeps ours readable dims
    /// theirs. Off by default, so no existing site changes.
    /// </summary>
    [Fact]
    public async Task ABannerCarryingItsOwnText_IsLeftAlone()
    {
        await SaveSettingsAsync(new() { ["heroPlainImages"] = true });

        Assert.Contains("hero-plain", await _client.GetStringAsync("/"));
    }

    [Fact]
    public async Task ByDefault_TheHeroKeepsItsOwnWords()
    {
        await SaveSettingsAsync([]);

        Assert.DoesNotContain("hero-plain", await _client.GetStringAsync("/"));
    }

    /// <summary>
    /// The template's sections are the school's own words and pictures. A template that ships
    /// colours and type but leaves every section empty produces a site nobody can be shown.
    /// </summary>
    [Fact]
    public void TheProspectusTemplate_ShipsItsSectionsFilledIn()
    {
        var template = Cms.Application.Templates.SiteTemplateCatalog.Find("campus-prospectus")!;

        var keys = template.HomeSections.Select(x => x.Key).ToList();
        Assert.Contains("crest", keys);
        Assert.Contains("principal", keys);
        Assert.Contains("alumni", keys);
        Assert.Contains("gallery", keys);
        Assert.Contains("why_choose_us", keys);

        // Every payload must parse, or the section renders nothing at all.
        foreach (var section in template.HomeSections)
        {
            var parsed = System.Text.Json.Nodes.JsonNode.Parse(section.Json);
            Assert.NotNull(parsed);
        }

        var crest = template.HomeSections.First(x => x.Key == "crest");
        Assert.Contains("The torch", crest.Json);
        Assert.False(string.IsNullOrWhiteSpace(crest.ImageUrl));

        var photographs = template.HomeSections.First(x => x.Key == "gallery");
        Assert.True(
            System.Text.Json.Nodes.JsonNode.Parse(photographs.Json)!["items"]!.AsArray().Count >= 8,
            "the photograph wall needs enough pictures to be a wall");

        // The banner pictures are finished artwork, so the hero prints nothing over them.
        Assert.True((bool)template.Settings["heroPlainImages"]!);
        Assert.True(template.HeroImages.Count > 1);
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
