extern alias adminapp;

using System.Reflection;
using IReloadablePage = adminapp::Cms.Admin.Filters.IReloadablePage;

namespace Cms.Api.IntegrationTests;

/// <summary>
/// When a save or a removal is refused, the page comes back carrying the reason. If it comes
/// back with an empty table beside that reason, the operator reads it as the refused action
/// having destroyed everything — which is the opposite of what happened, and the one conclusion
/// that leads them to do something drastic.
///
/// Removing a website's last domain is exactly that case: it is refused, and the Domains page
/// used to redisplay itself saying "No domains yet" over the school's three live hosts.
/// </summary>
public sealed class RefusedActionKeepsTheListTests
{
    [Fact]
    public void EveryPageThatRefetchesAList_CanBeAskedToRefetchIt()
    {
        var pages = typeof(IReloadablePage).Assembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false }
                && type.Name == "IndexModel"
                && type.Namespace?.Contains(".CMS.Pages.") == true)
            .ToList();

        Assert.NotEmpty(pages);

        var missing = pages
            .Where(page => Handles(page, "OnPost") && Handles(page, "Load", "Refresh"))
            .Where(page => !typeof(IReloadablePage).IsAssignableFrom(page))
            .Select(page => page.Namespace!.Split('.')[^1])
            .OrderBy(name => name)
            .ToList();

        Assert.True(
            missing.Count == 0,
            "These pages would redisplay an empty list beside their error message: "
            + string.Join(", ", missing));
    }

    private static bool Handles(Type page, params string[] prefixes) =>
        page.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Any(method => prefixes.Any(prefix =>
                method.Name.StartsWith(prefix, StringComparison.Ordinal)));
}
