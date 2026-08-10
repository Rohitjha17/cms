extern alias adminapp;

using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Cms.Domain.Constants;
using Cms.Infrastructure.Persistence;
using Cms.Infrastructure.Persistence.Seed;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AdminProgram = adminapp::Program;

namespace Cms.Api.IntegrationTests;

/// <summary>
/// Smoke coverage for the Admin console: every CMS screen must render for a signed-in
/// administrator, and the website preview must show the site's own stored data rather
/// than placeholder copy.
///
/// Authentication is injected through a test scheme, so no password is involved.
/// </summary>
public sealed class AdminConsoleTests : IClassFixture<AdminFactory>
{
    private readonly AdminFactory _factory;
    private readonly HttpClient _client;

    public AdminConsoleTests(AdminFactory factory)
    {
        _factory = factory;
        // Redirects must not be followed: an authorization bounce to the sign-in page also
        // returns 200 and would disguise a screen the role cannot actually open.
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
    }

    public static TheoryData<string> TenantAdminPages() =>
    [
        "/",
        "/CMS/Branding/Index",
        "/CMS/HomePage/Index",
        "/CMS/HomePage/Create",
        "/CMS/HomePage/Preview",
        "/CMS/Pages/Index",
        "/CMS/Navigation/Index",
        "/CMS/Media/Index",
        "/CMS/Contacts/Index",
        "/CMS/Seo/Index",
        "/CMS/Websites/Index",
        "/CMS/PageGallery/Index",
        "/CMS/Users/Index",
        "/CMS/People/Index",
        "/CMS/News/Index",
        "/CMS/Events/Index",
        "/CMS/Settings/Index",
        "/CMS/Departments/Index",
        "/CMS/Domains/Index",
        "/Account/ChangePassword"
    ];

    [Theory]
    [MemberData(nameof(TenantAdminPages))]
    public async Task EveryCmsPage_RendersForATenantAdministrator(string path)
    {
        using var response = await _client.GetAsync(path);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var html = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("An unhandled exception", html);
        // A sign-in form here would mean the page bounced instead of rendering.
        Assert.DoesNotContain("Sign in to CMS", html);
    }

    [Fact]
    public async Task PlatformTenantScreen_RendersForASuperAdministrator()
    {
        using var client = _factory.CreateClient(
            new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, AppRoles.SuperAdmin);

        using var response = await client.GetAsync("/CMS/Tenants/Index");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>
    /// The preview previously shipped an invented menu and invented marketing copy, so it
    /// showed a different institution than the one being edited.
    /// </summary>
    [Fact]
    public async Task Preview_ShowsTheEditedWebsiteNotPlaceholderCopy()
    {
        using var response = await _client.GetAsync("/CMS/HomePage/Preview?site=school");
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();

        // The preview header is generated chrome, so assert on it directly. Section bodies are
        // author-controlled content and may legitimately say anything.
        var navStart = html.IndexOf("site-preview__logo", StringComparison.Ordinal);
        Assert.True(navStart > 0, "Preview canvas was not rendered.");
        var nav = html[navStart..html.IndexOf("</nav>", navStart, StringComparison.Ordinal)];

        // Branding is the website being edited, not the tenant it belongs to.
        Assert.Contains("Cambridge High School", nav);
        Assert.DoesNotContain("Demo Academy", nav);

        // Menu entries come from the site's own navigation.
        Assert.Contains("Admission", nav);

        // Navigation that used to be hard-coded regardless of the site's real menu.
        Assert.DoesNotContain("Campus life", nav);
        Assert.DoesNotContain("Academics", nav);

        // Marketing copy that was invented by the view rather than stored by the author.
        var canvas = html[navStart..];
        Assert.DoesNotContain("Excellence in education", canvas);
        Assert.DoesNotContain("Shaping futures through education", canvas);
        Assert.DoesNotContain("Years of excellence", canvas);
    }

    [Fact]
    public async Task AdminPages_DoNotLinkToAHardCodedLocalhostSite()
    {
        using var response = await _client.GetAsync("/");
        response.EnsureSuccessStatusCode();
        Assert.DoesNotContain("http://localhost:5301", await response.Content.ReadAsStringAsync());
    }

    /// <summary>
    /// These screens used to be one generic "content entry" form with a raw JSON box. Each now
    /// has to present the fields the content actually has.
    /// </summary>
    [Theory]
    [InlineData("/CMS/People/Index", "Designation", "Qualification", "Years of experience")]
    [InlineData("/CMS/News/Index", "Headline", "Attachment (PDF)", "Feature this item")]
    [InlineData("/CMS/Events/Index", "Starts", "Venue", "Registration link")]
    [InlineData("/CMS/Settings/Index", "Announcement bar", "Admissions", "WhatsApp number")]
    [InlineData("/CMS/Departments/Index", "Head of department", "Programmes offered", "Overview")]
    public async Task RebuiltScreens_ExposeTypedFields(string path, string first, string second, string third)
    {
        using var response = await _client.GetAsync(path);
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();

        Assert.Contains(first, html);
        Assert.Contains(second, html);
        Assert.Contains(third, html);

        // The generic escape hatch these screens used to rely on.
        Assert.DoesNotContain("Additional JSON", html);
    }

    [Fact]
    public async Task PeopleScreen_ShowsTheSeededDirectory()
    {
        using var response = await _client.GetAsync("/CMS/People/Index?site=school");
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();

        Assert.Contains("Dr. Anita Rao", html);
        Assert.Contains("Teaching staff", html);
    }

    /// <summary>
    /// The generic six-tab content editor was removed once every type had a real screen;
    /// leaving it reachable meant two different ways to edit the same records.
    /// </summary>
    [Fact]
    public async Task GenericContentEditor_IsGone()
    {
        using var response = await _client.GetAsync("/CMS/Content/Index?type=news");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    /// <summary>
    /// "How do I add a site and choose a template?" has to be answerable from the screen:
    /// every home design is offered as a described choice, not an enum name in a dropdown.
    /// </summary>
    [Fact]
    public async Task WebsiteFactory_OffersEveryHomeDesignWithGuidance()
    {
        using var response = await _client.GetAsync("/CMS/Websites/Index");
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();

        foreach (var variant in Enum.GetNames<Cms.Domain.Enums.HomeVariant>())
        {
            Assert.Contains($"value=\"{variant}\"", html);
        }

        Assert.Contains("design-picker", html);
        Assert.Contains("Best for:", html);
        Assert.Contains("Starter pages", html);
        Assert.Contains("Manage this website", html);
    }

    /// <summary>
    /// Domains are how one deployment serves many institutions, so the screen has to show
    /// the binding, not just the host name.
    /// </summary>
    [Fact]
    public async Task DomainScreen_ShowsBindingAndResolutionFlow()
    {
        using var response = await _client.GetAsync("/CMS/Domains/Index");
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();

        Assert.Contains("localhost", html);          // seeded demo host
        Assert.Contains("All websites", html);       // unbound host serves every site by path
        Assert.Contains("How a visitor reaches the right website", html);
        Assert.Contains("Primary", html);
        Assert.Contains("DNS", html);
    }

    /// <summary>
    /// Domain editing must exist in exactly one place. It was previously possible to edit
    /// hosts from the Tenants form as well, where an absent row silently deactivated them.
    /// </summary>
    [Fact]
    public async Task TenantScreen_DoesNotAlsoEditDomains()
    {
        using var client = _factory.CreateClient(
            new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, AppRoles.SuperAdmin);

        using var response = await client.GetAsync("/CMS/Tenants/Index");
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();

        Assert.DoesNotContain("Input.Domains[0].DomainName", html);
        Assert.Contains("Open Domains", html);
    }

    /// <summary>
    /// SEO was five flat fields with no feedback. It has to show what a searcher would see
    /// and which pages fall short, otherwise it cannot be acted on.
    /// </summary>
    [Fact]
    public async Task SeoScreen_PreviewsResultsAndAuditsPages()
    {
        using var response = await _client.GetAsync("/CMS/Seo/Index?site=school");
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();

        Assert.Contains("Search result preview", html);
        Assert.Contains("serp__title", html);
        Assert.Contains("social-card", html);
        Assert.Contains("Page audit", html);
        Assert.Contains("Allow search engines to index", html);
        Assert.Contains("/robots.txt", html);
        Assert.Contains("/sitemap.xml", html);

        // The audit lists real pages from the site.
        Assert.Contains("Admission", html);
    }

    [Fact]
    public async Task Responses_CarrySecurityHeaders()
    {
        using var response = await _client.GetAsync("/");
        Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").Single());
        Assert.True(response.Headers.Contains("Content-Security-Policy"));
    }
}

public sealed class AdminFactory : WebApplicationFactory<AdminProgram>
{
    private readonly string _databaseName = $"cms-admin-tests-{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, configuration) =>
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Seed:EnableDemoData"] = "true",
                ["Seed:DemoAdminPassword"] = "Admin@12345",
                ["Database:ApplyMigrationsOnStartup"] = "false",
                ["ConnectionStrings:DefaultConnection"] = "Server=test-only",
                ["Storage:Provider"] = "Local",
                ["Storage:LocalRootPath"] = Path.Combine(Path.GetTempPath(), _databaseName, "uploads"),
                ["Tenancy:ResolutionCacheSeconds"] = "0"
            }));
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<ApplicationDbContext>();
            services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(_databaseName));

            // Sign every request in as a tenant administrator without going through a password.
            services.Configure<CookieAuthenticationOptions>(
                IdentityConstants.ApplicationScheme,
                options => options.ForwardAuthenticate = TestAuthHandler.SchemeName);
            services.AddAuthentication()
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });
        });
    }
}

public sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "TestAuth";

    /// <summary>Lets a test choose the role it acts as, so role boundaries can be exercised.</summary>
    public const string RoleHeader = "X-Test-Role";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder) : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var role = Request.Headers.TryGetValue(RoleHeader, out var requested)
            && !string.IsNullOrWhiteSpace(requested)
                ? requested.ToString()
                : AppRoles.TenantAdmin;

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "test-admin"),
            new(ClaimTypes.Name, "admin@demo.local"),
            new(ClaimTypes.Role, role)
        };

        // A super administrator intentionally spans tenants and carries no tenant claim.
        if (role != AppRoles.SuperAdmin)
        {
            claims.Add(new Claim(AppClaimTypes.TenantId, DatabaseSeeder.DemoTenantId.ToString()));
        }

        var identity = new ClaimsIdentity(claims, SchemeName);

        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
