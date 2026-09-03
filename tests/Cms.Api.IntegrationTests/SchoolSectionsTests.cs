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
/// Sections a school actually needs, drawn from studying what real school websites publish and
/// this CMS could not express: opening hours, the crest read symbol by symbol, alumni, the staff
/// list, named facilities, the founder's history, and a downloads table.
///
/// Each is checked the whole way through — written as a section, read back off the public page —
/// because a section that saves and renders nothing is the failure hardest to notice.
/// </summary>
public sealed class SchoolSectionsTests : IClassFixture<PublicWebFactory>
{
    private readonly PublicWebFactory _factory;
    private readonly HttpClient _client;

    public SchoolSectionsTests(PublicWebFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task SchoolTimings_RenderAsATableOfHours()
    {
        await SaveSectionAsync(HomePageSectionKeys.Timings, "School timings", new
        {
            firstTerm = "Summer",
            secondTerm = "Winter",
            items = new[]
            {
                new { wing = "Pre-School", summer = "7:45 to 11:45", winter = "8:45 to 12:45" },
                new { wing = "Senior School", summer = "7:00 to 13:10", winter = "8:00 to 14:10" }
            }
        });

        var html = await _client.GetStringAsync("/");

        Assert.Contains("timings-table", html);
        Assert.Contains("Pre-School", html);
        Assert.Contains("7:45 to 11:45", html);
        Assert.Contains("8:00 to 14:10", html);
        // The term names are the school's, not ours: not every school calls them summer and winter.
        Assert.Contains(">Summer<", html);
    }

    [Fact]
    public async Task TheCrest_IsReadSymbolBySymbol()
    {
        await SaveSectionAsync(HomePageSectionKeys.Crest, "Our crest", new
        {
            intro = "We learn to serve",
            items = new[]
            {
                new { symbol = "The book", meaning = "Learning" },
                new { symbol = "The torch", meaning = "Courage and leadership" }
            }
        });

        var html = await _client.GetStringAsync("/");

        Assert.Contains("crest-meanings", html);
        Assert.Contains("The torch", html);
        Assert.Contains("Courage and leadership", html);
        Assert.Contains("We learn to serve", html);
    }

    [Fact]
    public async Task Alumni_AppearWithWhatTheyDoNow()
    {
        await SaveSectionAsync(HomePageSectionKeys.Alumni, "Notable alumni", new
        {
            items = new[]
            {
                new { name = "Aniket Bera", role = "Professor of Computer Science", batch = "1998", imageUrl = "/uploads/aniket.jpg" }
            }
        });

        var html = await _client.GetStringAsync("/");

        Assert.Contains("alumni-grid", html);
        Assert.Contains("Aniket Bera", html);
        Assert.Contains("Professor of Computer Science", html);
        Assert.Contains("Class of 1998", html);
    }

    /// <summary>
    /// Forty names as cards is four screens of scrolling and cannot be searched with the
    /// browser's own find. A table is one screen, and it can.
    /// </summary>
    [Fact]
    public async Task TheStaffList_RendersAsATableWithRowNumbers()
    {
        await SaveSectionAsync(HomePageSectionKeys.StaffList, "Staff list", new
        {
            items = new[]
            {
                new { name = "Ms. Surabhi Bhargav", designation = "Principal", qualification = "M.Sc., M.Ed." },
                new { name = "Ms. Sheetal Kapoor", designation = "Vice-Principal", qualification = "M.A., B.Ed." }
            }
        });

        var html = await _client.GetStringAsync("/");

        Assert.Contains("staff-table", html);
        Assert.Contains("Ms. Surabhi Bhargav", html);
        Assert.Contains("Vice-Principal", html);
    }

    [Fact]
    public async Task Facilities_AreNamedAndDescribed()
    {
        await SaveSectionAsync(HomePageSectionKeys.Facilities, "Facilities", new
        {
            items = new[]
            {
                new { title = "Chemistry laboratory", description = "Used by the senior classes.", imageUrl = "/uploads/chem.jpg" }
            }
        });

        var html = await _client.GetStringAsync("/");

        Assert.Contains("facility-list", html);
        Assert.Contains("Chemistry laboratory", html);
        Assert.Contains("Used by the senior classes.", html);
    }

    [Fact]
    public async Task TheFoundersHistory_RendersAsADatedSequence()
    {
        await SaveSectionAsync(HomePageSectionKeys.Founder, "Founder & history", new
        {
            name = "Mr. Alok Chandra Deb",
            lifespan = "1898 – 1971",
            items = new[]
            {
                new { year = "1931", title = "The school opens", description = "In a small flat on Qutub Road." },
                new { year = "1950", title = "A boarding school follows", description = "Set up in Mussoorie." }
            }
        });

        var html = await _client.GetStringAsync("/");

        Assert.Contains("founder-timeline", html);
        Assert.Contains("Mr. Alok Chandra Deb", html);
        // Razor encodes the en dash to a numeric entity, so the dates are checked either side
        // of it rather than as the literal string a school typed.
        Assert.Contains("1898", html);
        Assert.Contains("1971", html);
        Assert.Contains("The school opens", html);
        Assert.Contains("<time>1931</time>", html);
    }

    /// <summary>
    /// Format and size are stated because a parent on a phone deserves to know what a link is
    /// about to cost them before they tap it.
    /// </summary>
    [Fact]
    public async Task Downloads_StateTheirFormatAndSize()
    {
        await SaveSectionAsync(HomePageSectionKeys.Downloads, "Downloads", new
        {
            items = new[]
            {
                new { title = "ICSE timetable 2026", fileUrl = "/uploads/icse.pdf", format = "PDF", size = "240 KB" }
            }
        });

        var html = await _client.GetStringAsync("/");

        Assert.Contains("downloads-table", html);
        Assert.Contains("ICSE timetable 2026", html);
        Assert.Contains("240 KB", html);
        Assert.Contains("/uploads/icse.pdf", html);
    }

    /// <summary>
    /// A form that cannot tell an admissions question from a job application sends both to the
    /// same person, who then sorts the inbox by reading it.
    /// </summary>
    [Fact]
    public async Task EnquiryTypes_BecomeAChooserOnThePopupForm()
    {
        await SaveSettingsAsync(new Dictionary<string, object?>
        {
            ["popupEnabled"] = true,
            ["popupShowEnquiryForm"] = true,
            ["enquiryTypes"] = "Admission enquiry\nJob application\nFeedback"
        });

        var html = await _client.GetStringAsync("/");

        Assert.Contains("Enquiry type", html);
        Assert.Contains("<option value=\"Admission enquiry\">", html);
        Assert.Contains("<option value=\"Job application\">", html);
    }

    [Fact]
    public async Task NoEnquiryTypes_LeavesThePlainSubjectBox()
    {
        await SaveSettingsAsync(new Dictionary<string, object?>
        {
            ["popupEnabled"] = true,
            ["popupShowEnquiryForm"] = true
        });

        Assert.DoesNotContain("Enquiry type", await _client.GetStringAsync("/"));
    }

    // ---------------------------------------------------------------- helpers

    private async Task SaveSectionAsync(string key, string title, object json)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var siteId = await db.Sites.IgnoreQueryFilters()
            .Where(x => x.SiteKey == "school").Select(x => x.Id).FirstAsync();

        var section = await db.HomePageSections.IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.SiteId == siteId && x.SectionKey == key);

        var payload = JsonSerializer.Serialize(json);
        if (section is null)
        {
            var tenantId = await db.Sites.IgnoreQueryFilters()
                .Where(x => x.Id == siteId).Select(x => x.TenantId).FirstAsync();
            db.HomePageSections.Add(new HomePageSection
            {
                TenantId = tenantId,
                SiteId = siteId,
                SectionKey = key,
                Title = title,
                JsonData = payload,
                IsActive = true,
                DisplayOrder = 90
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
