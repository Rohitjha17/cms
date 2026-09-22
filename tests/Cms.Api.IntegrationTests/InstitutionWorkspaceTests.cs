extern alias adminapp;

using System.Net;
using System.Text.RegularExpressions;
using Cms.Domain.Constants;
using Cms.Domain.Entities;
using Cms.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cms.Api.IntegrationTests;

/// <summary>
/// The console is one address for every institution. On production that meant every account,
/// and every website created from the console, landed in whichever institution owned the address:
/// a new school's staff saw every other school, and a second institution could not be created at
/// all, because creating one demanded a domain on a form that had nowhere to type it.
///
/// Now the account decides the institution: a school's own user always works in theirs and cannot
/// choose another; a SuperAdmin chooses. And a website is deleted in two steps — closed, then, if
/// wanted, deleted for good.
/// </summary>
public sealed class InstitutionWorkspaceTests : IClassFixture<AdminFactory>
{
    private readonly AdminFactory _factory;

    public InstitutionWorkspaceTests(AdminFactory factory) => _factory = factory;

    [Fact]
    public async Task ASchoolsOwnUser_WorksInTheirInstitution_OnTheSharedConsoleAddress()
    {
        var (tenantId, _) = await SeedInstitutionAsync("Green Valley", "green-valley", "gv-school", "Green Valley School");

        var page = await GetAsync(Client(AppRoles.TenantAdmin, tenantId), "/CMS/Websites/Index");

        Assert.Contains("Green Valley School", page);
        // Not the institution whose address the console is open on.
        Assert.DoesNotContain("href=\"?site=school\"", page);
    }

    [Fact]
    public async Task ASchoolsOwnUser_CannotChooseAnotherInstitution()
    {
        var (tenantId, _) = await SeedInstitutionAsync("Blue Hills", "blue-hills", "bh-school", "Blue Hills School");

        var page = await GetAsync(Client(AppRoles.TenantAdmin), $"/CMS/Websites/Index?tenant={tenantId}");

        Assert.DoesNotContain("Blue Hills School", page);
    }

    [Fact]
    public async Task ASuperAdmin_ChoosesAnInstitution_AndStaysInIt()
    {
        var (tenantId, _) = await SeedInstitutionAsync("Red Rock", "red-rock", "rr-school", "Red Rock School");
        var client = Client(AppRoles.SuperAdmin);

        var switched = await GetAsync(client, $"/CMS/Websites/Index?tenant={tenantId}");
        Assert.Contains("Red Rock School", switched);
        Assert.Contains("Switch institution", switched);

        var nextPage = await GetAsync(client, "/CMS/Branding/Index");
        Assert.Contains("Red Rock School", nextPage);
    }

    [Fact]
    public async Task ANewInstitution_CanBeCreated_WithoutADomain()
    {
        var saved = await PostFormAsync(Client(AppRoles.SuperAdmin), "/CMS/Tenants/Index", "Save", new()
        {
            ["Input.Name"] = "Lotus Academy",
            ["Input.Code"] = "lotus-academy",
            ["Input.IsActive"] = "true",
            ["Input.Sites[0].Name"] = "Lotus Academy",
            ["Input.Sites[0].SiteKey"] = "lotus",
            ["Input.Sites[0].WebsiteType"] = "School",
            ["Input.Sites[0].HomeVariant"] = "Campus",
            ["Input.Sites[0].IsDefault"] = "true",
            ["Input.Sites[0].IsActive"] = "true"
        });

        Assert.DoesNotContain("must not be empty", saved);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var tenant = await db.Tenants.IgnoreQueryFilters().SingleAsync(x => x.Code == "lotus-academy");
        Assert.True(await db.Sites.IgnoreQueryFilters().AnyAsync(x => x.TenantId == tenant.Id && x.SiteKey == "lotus"));
    }

    [Fact]
    public async Task ANewInstitution_CanBeGivenItsDomain_OnTheTenantForm()
    {
        var form = await GetAsync(Client(AppRoles.SuperAdmin), "/CMS/Tenants/Index");
        Assert.Contains("name=\"Input.Domains[0].DomainName\"", form);

        await PostFormAsync(Client(AppRoles.SuperAdmin), "/CMS/Tenants/Index", "Save", new()
        {
            ["Input.Name"] = "Oak Tree School",
            ["Input.Code"] = "oak-tree",
            ["Input.IsActive"] = "true",
            ["Input.Sites[0].Name"] = "Oak Tree School",
            ["Input.Sites[0].SiteKey"] = "school",
            ["Input.Sites[0].WebsiteType"] = "School",
            ["Input.Sites[0].HomeVariant"] = "Bulletin",
            ["Input.Sites[0].IsDefault"] = "true",
            ["Input.Sites[0].IsActive"] = "true",
            ["Input.Domains[0].DomainName"] = "oaktree.edu.in",
            ["Input.Domains[0].IsPrimary"] = "true",
            ["Input.Domains[0].IsActive"] = "true"
        });

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var tenant = await db.Tenants.IgnoreQueryFilters().SingleAsync(x => x.Code == "oak-tree");
        Assert.True(await db.TenantDomains.IgnoreQueryFilters()
            .AnyAsync(x => x.TenantId == tenant.Id && x.DomainName == "oaktree.edu.in" && x.IsPrimary));
    }

    /// <summary>
    /// Editing an institution used to be refused outright: its domains were shown but not posted
    /// back, and the form then failed for having none. Saving must work, and must leave every
    /// existing address exactly as it was.
    /// </summary>
    [Fact]
    public async Task EditingAnInstitution_Saves_AndKeepsItsDomains()
    {
        var (tenantId, _) = await SeedInstitutionAsync("Maple Leaf", "maple-leaf", "school", "Maple Leaf School", withDomain: "mapleleaf.edu.in");

        await PostFormAsync(Client(AppRoles.SuperAdmin), $"/CMS/Tenants/Index", "Save", new()
        {
            ["EditId"] = tenantId.ToString(),
            ["Input.Name"] = "Maple Leaf International",
            ["Input.Code"] = "maple-leaf",
            ["Input.IsActive"] = "true",
            ["Input.Sites[0].Name"] = "Maple Leaf School",
            ["Input.Sites[0].SiteKey"] = "school",
            ["Input.Sites[0].WebsiteType"] = "School",
            ["Input.Sites[0].HomeVariant"] = "Campus",
            ["Input.Sites[0].IsDefault"] = "true",
            ["Input.Sites[0].IsActive"] = "true",
            ["Input.Domains[0].DomainName"] = "mapleleaf.edu.in",
            ["Input.Domains[0].SiteKey"] = "school",
            ["Input.Domains[0].IsPrimary"] = "false",
            ["Input.Domains[0].IsActive"] = "true"
        }, $"&edit={tenantId}");

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.Equal("Maple Leaf International", (await db.Tenants.IgnoreQueryFilters().SingleAsync(x => x.Id == tenantId)).Name);
        var domain = await db.TenantDomains.IgnoreQueryFilters().SingleAsync(x => x.TenantId == tenantId);
        Assert.Equal("mapleleaf.edu.in", domain.DomainName);
        Assert.True(domain.IsActive);
    }

    /// <summary>
    /// "View live" for another institution's website must never open the console address owner's
    /// website. With the same key on both — "school" — a path link on the console's own address
    /// opened the wrong school. Without an address of its own there is nothing to link to; with one,
    /// the link goes there.
    /// </summary>
    [Fact]
    public async Task ViewLive_ForAnotherInstitution_NeverOpensTheConsoleOwnersWebsite()
    {
        var (tenantId, _) = await SeedInstitutionAsync("Birch Wood", "birch-wood", "school", "Birch Wood School");
        var factory = _factory.WithWebHostBuilder(builder =>
            builder.ConfigureAppConfiguration((_, config) =>
                config.AddInMemoryCollection(new Dictionary<string, string?> { ["PublicSite:PathBase"] = "/site" })));
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = true });
        client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, AppRoles.SuperAdmin);

        var withoutAddress = await GetAsync(client, $"/CMS/Websites/Index?tenant={tenantId}");
        Assert.Contains("Birch Wood School", withoutAddress);
        Assert.DoesNotContain("/site/school", withoutAddress);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.TenantDomains.Add(new TenantDomain { TenantId = tenantId, DomainName = "birchwood.edu.in", IsPrimary = true });
            await db.SaveChangesAsync();
        }

        var withAddress = await GetAsync(client, "/CMS/Websites/Index");
        Assert.Contains("https://birchwood.edu.in/school", withAddress);
        Assert.DoesNotContain("/site/school", withAddress);
    }

    /// <summary>
    /// An institution with no open website — just created with its only site switched off, or
    /// with every website closed — must still be workable: the console opens, says so, and offers
    /// to create one, rather than failing on the first page that expects a website.
    /// </summary>
    [Fact]
    public async Task AnInstitutionWithNoOpenWebsite_StillOpensEveryConsolePage()
    {
        var (tenantId, siteId) = await SeedInstitutionAsync("Willow Bay", "willow-bay", "wb-school", "Willow Bay School");
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var site = await db.Sites.IgnoreQueryFilters().SingleAsync(x => x.Id == siteId);
            site.IsActive = false;
            site.IsDefault = false;
            await db.SaveChangesAsync();
        }

        var client = Client(AppRoles.SuperAdmin);
        await GetAsync(client, $"/CMS/Websites/Index?tenant={tenantId}");

        var failures = new List<string>();
        foreach (var path in new[]
        {
            "/", "/CMS/Websites/Index", "/CMS/HomePage/Index", "/CMS/Pages/Index", "/CMS/Branding/Index",
            "/CMS/Navigation/Index", "/CMS/Media/Index", "/CMS/News/Index", "/CMS/Events/Index",
            "/CMS/People/Index", "/CMS/Domains/Index", "/CMS/Users/Index", "/CMS/Templates/Index",
            "/CMS/Seo/Index", "/CMS/Contacts/Index", "/CMS/Settings/Index"
        })
        {
            using var response = await client.GetAsync(path);
            var body = await response.Content.ReadAsStringAsync();
            if ((int)response.StatusCode >= 500 || body.Contains("An unhandled exception", StringComparison.Ordinal))
            {
                failures.Add($"{path} → {(int)response.StatusCode}");
            }
        }

        Assert.True(failures.Count == 0, "Pages that failed with no open website: " + string.Join(", ", failures));
    }

    [Fact]
    public async Task AClosedWebsite_IsTakenOffline_AndCanBeRestored()
    {
        var (tenantId, siteId) = await SeedInstitutionAsync("Pine Ridge", "pine-ridge", "pr-junior", "Pine Ridge Junior", withDomain: "junior.pineridge.in");
        var other = await AddSiteAsync(tenantId, "pr-senior", "Pine Ridge Senior");
        var client = Client(AppRoles.TenantAdmin, tenantId);

        await PostFormAsync(client, "/CMS/Websites/Index", "Close", new(), $"&id={siteId}");

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var closed = await db.Sites.IgnoreQueryFilters().Include(x => x.Domains).SingleAsync(x => x.Id == siteId);
            Assert.False(closed.IsActive);
            Assert.False(closed.IsDefault);
            // Its address stops answering rather than starting to serve another website.
            Assert.All(closed.Domains, d => Assert.False(d.IsActive));
            // The institution's default passes to the website still open.
            Assert.True((await db.Sites.IgnoreQueryFilters().SingleAsync(x => x.Id == other)).IsDefault);
        }

        var page = await GetAsync(client, "/CMS/Websites/Index");
        Assert.Contains("Closed websites", page);

        await PostFormAsync(client, "/CMS/Websites/Index", "Restore", new(), $"&id={siteId}");

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var restored = await db.Sites.IgnoreQueryFilters().Include(x => x.Domains).SingleAsync(x => x.Id == siteId);
            Assert.True(restored.IsActive);
            Assert.All(restored.Domains, d => Assert.True(d.IsActive));
        }
    }

    [Fact]
    public async Task AnOpenWebsite_OrAWrongKey_IsNeverDeletedForGood()
    {
        var (tenantId, siteId) = await SeedInstitutionAsync("Elm Park", "elm-park", "ep-school", "Elm Park School");
        await AddSiteAsync(tenantId, "ep-college", "Elm Park College");
        var client = Client(AppRoles.TenantAdmin, tenantId);

        var open = await PostFormAsync(client, "/CMS/Websites/Index", "Delete", new() { ["confirmation"] = "ep-school" }, $"&id={siteId}");
        Assert.Contains("Close this website before deleting it permanently.", open);

        await PostFormAsync(client, "/CMS/Websites/Index", "Close", new(), $"&id={siteId}");
        var wrongKey = await PostFormAsync(client, "/CMS/Websites/Index", "Delete", new() { ["confirmation"] = "something-else" }, $"&id={siteId}");
        Assert.Contains("to confirm deleting it permanently", wrongKey);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.True(await db.Sites.IgnoreQueryFilters().AnyAsync(x => x.Id == siteId));
    }

    [Fact]
    public async Task AnEditor_CannotCloseAWebsite()
    {
        var (tenantId, siteId) = await SeedInstitutionAsync("Cedar Hall", "cedar-hall", "ch-school", "Cedar Hall School");
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, AppRoles.Editor);
        client.DefaultRequestHeaders.Add(TestAuthHandler.TenantHeader, tenantId.ToString());
        var token = TokenFrom(await GetAsync(client, "/CMS/HomePage/Index"));

        using var response = await client.PostAsync($"/CMS/Websites/Index?handler=Close&id={siteId}",
            new FormUrlEncodedContent(new Dictionary<string, string> { ["__RequestVerificationToken"] = token }));

        // Refused: the console answers with a 403 or a redirect away from the action, never by
        // carrying it out.
        Assert.True(
            response.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.Redirect,
            $"Expected the editor to be refused, got {(int)response.StatusCode}");
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.True((await db.Sites.IgnoreQueryFilters().SingleAsync(x => x.Id == siteId)).IsActive);
    }

    private HttpClient Client(string role, Guid? tenantId = null)
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = true });
        client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, role);
        if (tenantId is Guid id)
        {
            client.DefaultRequestHeaders.Add(TestAuthHandler.TenantHeader, id.ToString());
        }

        return client;
    }

    private async Task<(Guid TenantId, Guid SiteId)> SeedInstitutionAsync(
        string name, string code, string siteKey, string siteName, string? withDomain = null)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var tenant = new Tenant { Name = name, Code = code };
        var site = new Site { TenantId = tenant.Id, Name = siteName, SiteKey = siteKey, IsDefault = true };
        db.Tenants.Add(tenant);
        db.Sites.Add(site);
        if (withDomain is not null)
        {
            db.TenantDomains.Add(new TenantDomain { TenantId = tenant.Id, SiteId = site.Id, DomainName = withDomain });
        }

        await db.SaveChangesAsync();
        return (tenant.Id, site.Id);
    }

    private async Task<Guid> AddSiteAsync(Guid tenantId, string siteKey, string siteName)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var site = new Site { TenantId = tenantId, Name = siteName, SiteKey = siteKey };
        db.Sites.Add(site);
        await db.SaveChangesAsync();
        return site.Id;
    }

    private static async Task<string> GetAsync(HttpClient client, string path)
    {
        using var response = await client.GetAsync(path);
        return await response.Content.ReadAsStringAsync();
    }

    private static async Task<string> PostFormAsync(
        HttpClient client, string path, string handler, Dictionary<string, string> values, string query = "")
    {
        values["__RequestVerificationToken"] = TokenFrom(await GetAsync(client, path));
        using var response = await client.PostAsync($"{path}?handler={handler}{query}", new FormUrlEncodedContent(values));
        return await response.Content.ReadAsStringAsync();
    }

    private static string TokenFrom(string html) =>
        Regex.Match(html, @"name=""__RequestVerificationToken""[^>]*value=""([^""]+)""").Groups[1].Value;
}
