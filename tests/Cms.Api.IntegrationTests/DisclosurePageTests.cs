using Cms.Domain.Enums;
using Cms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace Cms.Api.IntegrationTests;

/// <summary>
/// The board requires this page to state the affiliation number and the school code, and those
/// were the one thing it had no field for — they had to be written into the HTML by hand, where
/// nobody looking for them would think to check.
/// </summary>
public sealed class DisclosurePageTests : IClassFixture<PublicWebFactory>
{
    private readonly PublicWebFactory _factory;
    private readonly HttpClient _client;

    public DisclosurePageTests(PublicWebFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task TheAffiliationNumberAndSchoolCode_AppearAboveTheDocuments()
    {
        await SaveAsync(new
        {
            intro = "Published as required by the board.",
            affiliationNumber = "2130070",
            schoolCode = "60023",
            updatedOn = "1 April 2026",
            documents = new object[]
            {
                new { title = "Affiliation certificate", category = "General", description = "Valid to 2030", fileUrl = "/uploads/aff.pdf" }
            }
        });

        var html = await _client.GetStringAsync("/mandatory-disclosure");

        Assert.Contains("2130070", html);
        Assert.Contains("60023", html);
        Assert.Contains("1 April 2026", html);
        Assert.Contains("Published as required by the board.", html);
        Assert.Contains("Affiliation certificate", html);
        Assert.Contains("Valid to 2030", html);
        Assert.Contains("/uploads/aff.pdf", html);
    }

    [Fact]
    public async Task TheBuiltInTable_CanBeTurnedOffInFavourOfTheSchoolsOwnHtml()
    {
        await SaveAsync(
            new
            {
                showBuiltIn = false,
                documents = new object[]
                {
                    new { title = "Affiliation certificate", category = "General", fileUrl = "/uploads/aff.pdf" }
                }
            },
            html: "<table class=\"my-own-table\"><tr><td>Our own layout</td></tr></table>");

        var html = await _client.GetStringAsync("/mandatory-disclosure");

        Assert.Contains("my-own-table", html);
        Assert.DoesNotContain("/uploads/aff.pdf", html);
    }

    /// <summary>A page saved before the switch existed keeps the table it has always had.</summary>
    [Fact]
    public async Task APageSavedBeforeTheSwitchExisted_KeepsItsTable()
    {
        await SaveAsync(new
        {
            documents = new object[]
            {
                new { title = "Affiliation certificate", category = "General", fileUrl = "/uploads/aff.pdf" }
            }
        });

        Assert.Contains("/uploads/aff.pdf", await _client.GetStringAsync("/mandatory-disclosure"));
    }

    private async Task SaveAsync(object data, string? html = null)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var siteId = await db.Sites.IgnoreQueryFilters()
            .Where(x => x.SiteKey == "school").Select(x => x.Id).FirstAsync();

        var page = await db.Pages.IgnoreQueryFilters()
            .FirstAsync(x => x.SiteId == siteId && x.PageType == PageType.Disclosure);

        page.JsonData = JsonSerializer.Serialize(data);
        page.Content = html;
        page.IsActive = true;
        await db.SaveChangesAsync();
    }
}
