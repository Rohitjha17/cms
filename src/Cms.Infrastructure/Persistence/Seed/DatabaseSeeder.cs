using Cms.Domain.Constants;
using Cms.Domain.Entities;
using Cms.Domain.Enums;
using Cms.Infrastructure.Identity;
using Cms.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cms.Infrastructure.Persistence.Seed;

public static class DatabaseSeeder
{
    public static readonly Guid DemoTenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid DemoSchoolSiteId = Guid.Parse("22222222-2222-2222-2222-222222222221");
    public static readonly Guid DemoCollegeSiteId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var environment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        if (db.Database.IsSqlite())
        {
            await db.Database.EnsureCreatedAsync();
        }
        else if (configuration.GetValue("Database:ApplyMigrationsOnStartup", environment.IsDevelopment()))
        {
            // A database left half-built by an earlier attempt cannot be migrated forward, and
            // the error SQL Server gives for it explains nothing. Say what to do instead.
            await MigrationPreflight.EnsureCanMigrateAsync(db);
            await db.Database.MigrateAsync();
        }

        foreach (var role in AppRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new ApplicationRole { Name = role, Description = role });
            }
        }

        // Page gallery is platform catalog data — always ensure it exists.
        await PageTemplateSeed.EnsureAsync(db);

        // The platform console must exist in every environment, otherwise a clean
        // production database has no host that resolves and no account that can sign in.
        await PlatformSeed.EnsureAsync(db, userManager, configuration, logger);

        if (!environment.IsDevelopment() && !configuration.GetValue<bool>("Seed:EnableDemoData"))
        {
            return;
        }

        if (!await db.Tenants.IgnoreQueryFilters().AnyAsync(t => t.Id == DemoTenantId))
        {
            var tenant = new Tenant
            {
                Id = DemoTenantId,
                Name = "Demo Academy",
                Code = "demo",
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "seed"
            };

            db.Tenants.Add(tenant);
            db.TenantDomains.Add(new TenantDomain
            {
                Id = Guid.NewGuid(),
                TenantId = DemoTenantId,
                DomainName = "localhost",
                IsPrimary = true,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            });
            db.TenantDomains.Add(new TenantDomain
            {
                Id = Guid.NewGuid(),
                TenantId = DemoTenantId,
                DomainName = "127.0.0.1",
                IsPrimary = false,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            });

            db.Sites.Add(new Site
            {
                Id = DemoSchoolSiteId,
                TenantId = DemoTenantId,
                Name = "Cambridge High School",
                SiteKey = "school",
                WebsiteType = WebsiteType.School,
                HomeVariant = HomeVariant.Classic,
                IsDefault = true,
                IsActive = true,
                Tagline = "Excellence in schooling since day one",
                PrimaryColor = "#0f2d5c",
                SecondaryColor = "#c9a227",
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "seed"
            });

            db.Sites.Add(new Site
            {
                Id = DemoCollegeSiteId,
                TenantId = DemoTenantId,
                Name = "Cambridge College of Arts & Science",
                SiteKey = "college",
                WebsiteType = WebsiteType.College,
                HomeVariant = HomeVariant.Academic,
                IsDefault = false,
                IsActive = true,
                Tagline = "Where ambition meets opportunity",
                PrimaryColor = "#12263f",
                SecondaryColor = "#8b6b2e",
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "seed"
            });

            await db.SaveChangesAsync();
            logger.LogInformation("Seeded demo tenant and sites.");
        }

        await HomePageSeed.EnsureSectionsAsync(db, DemoTenantId, DemoSchoolSiteId);
        await HomePageSeed.EnsureSectionsAsync(db, DemoTenantId, DemoCollegeSiteId);
        await SchoolWebsiteSeed.EnsureAsync(
            db, DemoTenantId, DemoSchoolSiteId, HomeVariant.Classic,
            "Cambridge High School", "Excellence in schooling since day one");
        await SchoolWebsiteSeed.EnsureAsync(
            db, DemoTenantId, DemoCollegeSiteId, HomeVariant.Academic,
            "Cambridge College of Arts & Science", "Where ambition meets opportunity");
        await SchoolContentSeed.EnsureAsync(db, DemoTenantId, DemoSchoolSiteId);
        await SchoolContentSeed.EnsureAsync(db, DemoTenantId, DemoCollegeSiteId);

        // Demo domains intentionally host both /school and /college portals.
        var demoDomains = await db.TenantDomains.IgnoreQueryFilters()
            .Where(x => x.TenantId == DemoTenantId
                && (x.DomainName == "localhost" || x.DomainName == "127.0.0.1"))
            .ToListAsync();
        foreach (var demoDomain in demoDomains) demoDomain.SiteId = null;
        await db.SaveChangesAsync();

        var demoPassword = configuration["Seed:DemoAdminPassword"]
            ?? (environment.IsDevelopment()
                ? "Admin@12345"
                : throw new InvalidOperationException("Seed:DemoAdminPassword is required when demo data is enabled."));
        const string adminEmail = "admin@demo.local";
        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FullName = "Demo Admin",
                TenantId = DemoTenantId,
                IsActive = true
            };

            var result = await userManager.CreateAsync(admin, demoPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, AppRoles.TenantAdmin);
                logger.LogInformation("Seeded demo admin user {Email}", adminEmail);
            }
        }

        const string superAdminEmail = "superadmin@demo.local";
        var superAdmin = await userManager.FindByEmailAsync(superAdminEmail);
        if (superAdmin is null)
        {
            superAdmin = new ApplicationUser
            {
                UserName = superAdminEmail,
                Email = superAdminEmail,
                EmailConfirmed = true,
                FullName = "Platform Administrator",
                TenantId = null,
                IsActive = true
            };
            var result = await userManager.CreateAsync(superAdmin, demoPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(superAdmin, AppRoles.SuperAdmin);
                logger.LogInformation("Seeded development super administrator {Email}", superAdminEmail);
            }
        }
    }
}
