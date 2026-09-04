using Cms.Application.DTOs.Content;
using Cms.Application.Interfaces;
using Cms.Domain.Entities;
using Cms.Domain.Enums;
using Cms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Cms.Api.IntegrationTests;

/// <summary>
/// A school can take a page over completely: its own markup, the full width, and none of the
/// built-in layout underneath. Without the second half of that, a school that wrote its own
/// gallery got ours printed under it as well.
/// </summary>
public sealed class CustomHtmlPageTests : IClassFixture<PublicWebFactory>
{
    private readonly PublicWebFactory _factory;
    private readonly HttpClient _client;

    public CustomHtmlPageTests(PublicWebFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task WithTheSwitchOn_ThePageIsOnlyTheSchoolsOwnMarkup()
    {
        await SaveGalleryPageAsync(
            custom: true,
            html: "<section id=\"mine\"><h2>Our own gallery</h2></section>",
            items: new { items = new[] { new { album = "A", type = "image", url = "/uploads/x.jpg", caption = "Built in" } } });

        var page = await _client.GetStringAsync("/gallery");

        Assert.Contains("id=\"mine\"", page);
        Assert.Contains("Our own gallery", page);
        Assert.Contains("custom-html", page);
        // the built-in gallery must not be drawn underneath it
        Assert.DoesNotContain("gallery-grid", page);
        Assert.DoesNotContain("Built in", page);
    }

    [Fact]
    public async Task WithTheSwitchOff_TheBuiltInLayoutIsUsed()
    {
        await SaveGalleryPageAsync(
            custom: false,
            html: "<p>An introduction.</p>",
            items: new { items = new[] { new { album = "A", type = "image", url = "/uploads/x.jpg", caption = "Built in" } } });

        var page = await _client.GetStringAsync("/gallery");

        Assert.Contains("An introduction.", page);
        Assert.Contains("gallery-grid", page);
        Assert.Contains("Built in", page);
        Assert.DoesNotContain("custom-html", page);
    }

    private async Task SaveGalleryPageAsync(bool custom, string html, object items)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var siteId = await db.Sites.IgnoreQueryFilters()
            .Where(x => x.SiteKey == "school").Select(x => x.Id).FirstAsync();

        var page = await db.Pages.IgnoreQueryFilters()
            .FirstAsync(x => x.SiteId == siteId && x.PageType == PageType.Gallery);

        page.Content = html;
        page.JsonData = System.Text.Json.JsonSerializer.Serialize(items);
        page.UseCustomHtml = custom;
        page.IsActive = true;
        await db.SaveChangesAsync();
    }
}
