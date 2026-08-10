using Cms.Domain.Constants;
using Cms.Domain.Entities;
using Cms.Domain.Enums;
using Cms.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Cms.Infrastructure.Persistence.Seed;

/// <summary>
/// Bootstraps the platform console so a real deployment has somewhere for the operator
/// to sign in before any institution exists.
///
/// Without this, tenant resolution 404s every host and no account can be created —
/// the CMS would be unreachable on a clean production database.
///
/// Driven entirely by configuration and skipped when unset:
///   Platform:Domain              admin host, e.g. admin.yourcompany.com
///   Platform:SuperAdminEmail     first super administrator
///   Platform:SuperAdminPassword  initial password (change after first sign-in)
///   Platform:TenantName          optional display name
/// </summary>
public static class PlatformSeed
{
    private const string PlatformTenantCode = "platform";
    private const string PlatformSiteKey = "platform";

    public static async Task EnsureAsync(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        var domain = configuration["Platform:Domain"]?.Trim().ToLowerInvariant();
        var email = configuration["Platform:SuperAdminEmail"]?.Trim().ToLowerInvariant();
        var password = configuration["Platform:SuperAdminPassword"];

        if (string.IsNullOrWhiteSpace(domain) || string.IsNullOrWhiteSpace(email))
        {
            logger.LogInformation(
                "Platform console seeding skipped: set Platform:Domain and Platform:SuperAdminEmail to enable it.");
            return;
        }

        var tenant = await db.Tenants.IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Code == PlatformTenantCode, cancellationToken);

        if (tenant is null)
        {
            tenant = new Tenant
            {
                Id = Guid.NewGuid(),
                Name = configuration["Platform:TenantName"]?.Trim() is { Length: > 0 } name
                    ? name
                    : "Platform Console",
                Code = PlatformTenantCode,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "platform-seed"
            };
            db.Tenants.Add(tenant);
            await db.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Created platform tenant for the administration console.");
        }

        // A site keeps the site-scoped CMS screens usable for the operator; without one
        // every site-scoped page would fail to resolve a site and return an error.
        var site = await db.Sites.IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.TenantId == tenant.Id && x.SiteKey == PlatformSiteKey, cancellationToken);

        if (site is null)
        {
            site = new Site
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                Name = tenant.Name,
                SiteKey = PlatformSiteKey,
                WebsiteType = WebsiteType.Other,
                HomeVariant = HomeVariant.Classic,
                IsDefault = true,
                IsActive = true,
                Tagline = "Platform administration",
                PrimaryColor = "#0f2d5c",
                SecondaryColor = "#c9a227",
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "platform-seed"
            };
            db.Sites.Add(site);
            await db.SaveChangesAsync(cancellationToken);
            await HomePageSeed.EnsureSectionsAsync(db, tenant.Id, site.Id, cancellationToken);
        }

        var existingDomain = await db.TenantDomains.IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.DomainName == domain, cancellationToken);

        if (existingDomain is null)
        {
            db.TenantDomains.Add(new TenantDomain
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                SiteId = site.Id,
                DomainName = domain,
                IsPrimary = true,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "platform-seed"
            });
            await db.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Bound platform console host {Domain}.", domain);
        }
        else if (existingDomain.TenantId != tenant.Id)
        {
            // Never steal a host from a live school website.
            logger.LogWarning(
                "Platform:Domain '{Domain}' is already bound to another tenant; leaving it unchanged.", domain);
        }

        var superAdmin = await userManager.FindByEmailAsync(email);
        if (superAdmin is not null)
        {
            if (superAdmin.TenantId is null)
            {
                // Pin the operator to the platform tenant so self-service password reset
                // works from the console host.
                superAdmin.TenantId = tenant.Id;
                await userManager.UpdateAsync(superAdmin);
            }

            if (!await userManager.IsInRoleAsync(superAdmin, AppRoles.SuperAdmin))
            {
                await userManager.AddToRoleAsync(superAdmin, AppRoles.SuperAdmin);
            }

            return;
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning(
                "Super administrator {Email} does not exist and Platform:SuperAdminPassword is not set, "
                + "so no account was created.", email);
            return;
        }

        superAdmin = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FullName = configuration["Platform:SuperAdminName"]?.Trim() ?? "Platform Administrator",
            TenantId = tenant.Id,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        var created = await userManager.CreateAsync(superAdmin, password);
        if (!created.Succeeded)
        {
            logger.LogError(
                "Failed to create super administrator {Email}: {Errors}",
                email,
                string.Join("; ", created.Errors.Select(x => x.Description)));
            return;
        }

        await userManager.AddToRoleAsync(superAdmin, AppRoles.SuperAdmin);
        logger.LogInformation(
            "Created platform super administrator {Email}. Sign in at https://{Domain} and change the password.",
            email, domain);
    }
}
