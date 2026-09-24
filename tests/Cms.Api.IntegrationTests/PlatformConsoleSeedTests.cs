using Cms.Application.Interfaces;
using Cms.Domain.Constants;
using Cms.Infrastructure.Identity;
using Cms.Infrastructure.Persistence;
using Cms.Infrastructure.Persistence.Seed;
using Cms.Infrastructure.Tenancy;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace Cms.Api.IntegrationTests;

/// <summary>
/// The console's own address, and what happens when it is switched off.
///
/// On the live server it was: the row existed and was inactive, so every request to the console
/// answered "No active website is configured for this domain". Nothing in the product could put
/// it right, because everything that could switch an address back on is behind the console that
/// address opens — and the seeding that runs at startup left an existing row alone, whatever
/// state it was in. The operator was locked out on every restart, for good.
/// </summary>
public sealed class PlatformConsoleSeedTests : IDisposable
{
    private const string Host = "cms.shubhsoftsolution.com";

    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    private readonly ServiceProvider _services;

    public PlatformConsoleSeedTests()
    {
        _connection.Open();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<ITenantContext, TenantContext>();
        services.AddSingleton<ISiteContext, SiteContext>();
        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(_connection));
        services.AddIdentityCore<ApplicationUser>()
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();
        _services = services.BuildServiceProvider();

        using var scope = _services.CreateScope();
        scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.EnsureCreated();

        // The roles exist before any of this runs on a real deployment; seeding assigns one.
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        foreach (var role in AppRoles.All)
        {
            roles.CreateAsync(new ApplicationRole { Name = role }).GetAwaiter().GetResult();
        }
    }

    [Fact]
    public async Task TheConsoleHost_IsSwitchedBackOn_WhenItHasBeenSwitchedOff()
    {
        await SeedAsync();

        // Switched off — by hand, by a migration, by anything.
        using (var scope = _services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var domain = await db.TenantDomains.IgnoreQueryFilters().SingleAsync(x => x.DomainName == Host);
            domain.IsActive = false;
            var tenant = await db.Tenants.IgnoreQueryFilters().SingleAsync(x => x.Id == domain.TenantId);
            tenant.IsActive = false;
            await db.SaveChangesAsync();
        }

        // A restart is all the operator has left.
        await SeedAsync();

        using (var scope = _services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var domain = await db.TenantDomains.IgnoreQueryFilters().SingleAsync(x => x.DomainName == Host);
            Assert.True(domain.IsActive, "the console's own address must be reachable again");
            Assert.True((await db.Tenants.IgnoreQueryFilters().SingleAsync(x => x.Id == domain.TenantId)).IsActive);
        }
    }

    /// <summary>
    /// The address of a school, though, is the school's to switch off. Seeding must never take a
    /// host that belongs to somebody else, nor switch one back on.
    /// </summary>
    [Fact]
    public async Task ASchoolsOwnAddress_IsLeftAlone()
    {
        await SeedAsync();

        using (var scope = _services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var school = new Cms.Domain.Entities.Tenant { Name = "Green Valley", Code = "green-valley" };
            db.Tenants.Add(school);
            db.TenantDomains.Add(new Cms.Domain.Entities.TenantDomain
            {
                TenantId = school.Id,
                DomainName = "greenvalley.edu.in",
                IsActive = false
            });
            await db.SaveChangesAsync();
        }

        await SeedAsync();

        using (var scope = _services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var school = await db.TenantDomains.IgnoreQueryFilters().SingleAsync(x => x.DomainName == "greenvalley.edu.in");
            Assert.False(school.IsActive);
        }
    }

    private async Task SeedAsync()
    {
        using var scope = _services.CreateScope();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Platform:Domain"] = Host,
                ["Platform:SuperAdminEmail"] = "admin@local.com",
                ["Platform:SuperAdminPassword"] = "Admin@2026"
            })
            .Build();

        await PlatformSeed.EnsureAsync(
            scope.ServiceProvider.GetRequiredService<ApplicationDbContext>(),
            scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>(),
            configuration,
            NullLogger.Instance);
    }

    public void Dispose()
    {
        _services.Dispose();
        _connection.Dispose();
    }
}
