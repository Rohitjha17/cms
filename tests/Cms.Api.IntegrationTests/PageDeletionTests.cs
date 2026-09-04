using Cms.Application.DTOs.Content;
using Cms.Application.Interfaces;
using Cms.Domain.Entities;
using Cms.Domain.Enums;
using Cms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Cms.Api.IntegrationTests;

/// <summary>
/// Deleting a page took it off the site and left its link in the top bar, pointing at a 404,
/// with no way to take it out except editing the menu by hand.
///
/// The menu is kept in step with the pages by a rule that deliberately leaves alone any link
/// that is not a page — /news, an external site, anything somebody typed in themselves. A
/// deleted page's link is indistinguishable from one of those the moment the row is gone, so
/// nothing could ever withdraw it.
/// </summary>
public sealed class PageDeletionTests : IClassFixture<PublicWebFactory>
{
    private readonly PublicWebFactory _factory;

    public PageDeletionTests(PublicWebFactory factory) => _factory = factory;

    [Fact]
    public async Task DeletingAPage_AlsoTakesItOutOfTheTopBar()
    {
        var (content, website) = await ArrangeAsync();

        var page = await content.SavePageAsync(null, new SavePageDto
        {
            Title = "Transport",
            Slug = "transport",
            PageType = PageType.Custom,
            Content = "<p>Bus routes.</p>",
            IsActive = true,
            ShowInMenu = true
        }, default);

        var linked = await website.GetPublicWebsiteAsync(default);
        Assert.Contains(linked.Navigation, x => x.Url.EndsWith("/transport", StringComparison.Ordinal));

        await content.DeletePageAsync(page.Id, default);

        var after = await website.GetPublicWebsiteAsync(default);
        Assert.DoesNotContain(after.Navigation, x => x.Url.EndsWith("/transport", StringComparison.Ordinal));
    }

    /// <summary>
    /// The rule that protects hand-added links still has to hold, or deleting one page quietly
    /// strips the menu of every link that is not a page.
    /// </summary>
    [Fact]
    public async Task DeletingAPage_LeavesLinksThatAreNotPagesAlone()
    {
        var (content, website) = await ArrangeAsync();

        // Saving a page is what creates the header menu on a new website, so the page comes
        // first and the hand-added link goes in beside it.
        var doomed = await content.SavePageAsync(null, new SavePageDto
        {
            Title = "Scratch",
            Slug = "scratch",
            PageType = PageType.Custom,
            IsActive = true,
            ShowInMenu = true
        }, default);

        var header = await content.GetMenuByLocationAsync("header", default);
        var items = header.Items.ToList();
        items.Add(new MenuItemDto
        {
            Label = "Alumni portal",
            Url = "https://alumni.example.edu",
            DisplayOrder = 50,
            IsActive = true
        });

        await content.SaveMenuAsync(header.Id, new SaveMenuDto
        {
            Name = header.Name,
            Location = header.Location,
            IsActive = true,
            Items = items
        }, default);

        await content.DeletePageAsync(doomed.Id, default);

        var after = await website.GetPublicWebsiteAsync(default);
        Assert.Contains(after.Navigation, x => x.Url == "https://alumni.example.edu");
        Assert.DoesNotContain(after.Navigation, x => x.Url.EndsWith("/scratch", StringComparison.Ordinal));
    }

    /// <summary>A website of its own, so one test's menu is never another's.</summary>
    private async Task<(ISiteContentService Content, IWebsiteService Websites)> ArrangeAsync()
    {
        var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var tenantId = await db.Tenants.IgnoreQueryFilters().Select(x => x.Id).FirstAsync();
        var site = new Site
        {
            TenantId = tenantId,
            Name = "Menu Test Academy",
            SiteKey = $"m{Guid.NewGuid():N}"[..12],
            IsActive = true
        };
        db.Sites.Add(site);
        await db.SaveChangesAsync();

        scope.ServiceProvider.GetRequiredService<ITenantContext>().Set(tenantId, "test", "Test");
        scope.ServiceProvider.GetRequiredService<ISiteContext>().Set(site.Id, site.SiteKey, site.Name);

        return (
            scope.ServiceProvider.GetRequiredService<ISiteContentService>(),
            scope.ServiceProvider.GetRequiredService<IWebsiteService>());
    }
}
