using Cms.Domain.Constants;
using Cms.Domain.Entities;
using Cms.Domain.Enums;
using Cms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Cms.Infrastructure.Persistence.Seed;

/// <summary>
/// Ensures demo school/college websites have branding, starter pages, menus and SEO.
/// </summary>
public static class SchoolWebsiteSeed
{
    public static async Task EnsureAsync(
        ApplicationDbContext db,
        Guid tenantId,
        Guid siteId,
        HomeVariant homeVariant,
        string name,
        string tagline,
        CancellationToken cancellationToken = default)
    {
        var site = await db.Sites.IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == siteId && x.TenantId == tenantId, cancellationToken);
        if (site is null)
        {
            return;
        }

        if (site.UpdatedDate is null && (site.CreatedBy is null || site.CreatedBy == "seed"))
        {
            site.HomeVariant = homeVariant;
            site.Tagline ??= tagline;
            site.PrimaryColor ??= "#0f2d5c";
            site.SecondaryColor ??= "#c9a227";
            site.FooterText ??= $"© {DateTime.UtcNow.Year} {name}. All rights reserved.";
            site.Address ??= "123 Education Avenue, Knowledge City";
            site.Phone ??= "+91 98765 43210";
            site.Email ??= "admissions@demo.local";
            site.MapEmbedUrl ??= "https://maps.google.com/maps?q=New%20Delhi&t=&z=13&ie=UTF8&iwloc=&output=embed";
            if (string.IsNullOrWhiteSpace(site.Name) || site.Name.StartsWith("Demo"))
            {
                site.Name = name;
            }
        }

        var templates = await db.PageTemplates.AsNoTracking()
            .Where(x => x.IsActive && x.IsStarter)
            .OrderBy(x => x.DisplayOrder)
            .ToListAsync(cancellationToken);

        var existingPages = await db.Pages.IgnoreQueryFilters()
            .Where(x => x.TenantId == tenantId && x.SiteId == siteId)
            .ToListAsync(cancellationToken);
        var existingSlugs = existingPages.Select(x => x.Slug).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var order = 1;
        foreach (var template in templates)
        {
            if (existingSlugs.Contains(template.DefaultSlug))
            {
                continue;
            }

            db.Pages.Add(new Page
            {
                TenantId = tenantId,
                SiteId = siteId,
                PageType = template.PageType,
                TemplateKey = template.TemplateKey,
                Title = template.DefaultTitle ?? template.Name,
                Slug = template.DefaultSlug,
                Content = template.DefaultContent,
                JsonData = template.DefaultJsonData,
                ShowInMenu = true,
                MenuOrder = order++,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "seed"
            });
        }

        await db.SaveChangesAsync(cancellationToken);

        if (!await db.Menus.IgnoreQueryFilters()
                .AnyAsync(x => x.TenantId == tenantId && x.SiteId == siteId && x.Location == "header", cancellationToken))
        {
            var pages = await db.Pages.IgnoreQueryFilters()
                .Where(x => x.TenantId == tenantId && x.SiteId == siteId && x.IsActive && x.ShowInMenu)
                .OrderBy(x => x.MenuOrder)
                .ToListAsync(cancellationToken);

            var items = new List<MenuItem>
            {
                new()
                {
                    TenantId = tenantId,
                    SiteId = siteId,
                    Label = "Home",
                    Url = "/",
                    DisplayOrder = 0,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "seed"
                }
            };
            items.AddRange(pages.Select((p, index) => new MenuItem
            {
                TenantId = tenantId,
                SiteId = siteId,
                Label = p.Title,
                Url = $"/{p.Slug}",
                DisplayOrder = index + 1,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "seed"
            }));

            db.Menus.Add(new Menu
            {
                TenantId = tenantId,
                SiteId = siteId,
                Name = "Main navigation",
                Location = "header",
                IsActive = true,
                Items = items,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "seed"
            });
        }

        if (!await db.SeoSettings.IgnoreQueryFilters()
                .AnyAsync(x => x.TenantId == tenantId && x.SiteId == siteId, cancellationToken))
        {
            db.SeoSettings.Add(new SeoSetting
            {
                TenantId = tenantId,
                SiteId = siteId,
                MetaTitle = name,
                MetaDescription = tagline,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "seed"
            });
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
