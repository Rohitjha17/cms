using Cms.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Cms.Infrastructure.Persistence.Seed;

/// <summary>
/// Four complete demo websites, one per design, for showing the product and for testing it
/// by hand.
///
/// Separate from <see cref="SchoolContentSeed"/> on purpose. That one gives a new website a
/// believable starting point — a few notices, a few staff. This one is the opposite: every
/// section switched on and filled, every page type present, and the appearance settings spread
/// across the four so that no option is left unshown. It is not what a real school's site
/// should look like on day one, and it is exactly what a demonstration needs.
///
/// Idempotent per site: a site whose key already exists is left alone, so running it again
/// after editing one of them in the console will not overwrite the editing.
/// </summary>
public static class DemoShowcaseSeed
{
    public static async Task EnsureAsync(
        ApplicationDbContext db,
        Guid tenantId,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        var created = 0;

        foreach (var spec in ShowcaseSites.All)
        {
            var exists = await db.Sites.IgnoreQueryFilters()
                .AnyAsync(x => x.Id == spec.SiteId || (x.TenantId == tenantId && x.SiteKey == spec.Key),
                    cancellationToken);
            if (exists)
            {
                continue;
            }

            db.Sites.Add(new Site
            {
                Id = spec.SiteId,
                TenantId = tenantId,
                Name = spec.Name,
                SiteKey = spec.Key,
                WebsiteType = spec.WebsiteType,
                HomeVariant = spec.Variant,
                IsDefault = false,
                IsActive = true,
                LogoUrl = spec.Crest,
                FaviconUrl = spec.Crest,
                Tagline = spec.Tagline,
                PrimaryColor = spec.Primary,
                SecondaryColor = spec.Secondary,
                HeaderImageUrl = spec.Banner,
                FooterText = $"© {DateTime.UtcNow.Year} {spec.Name}. All rights reserved. Affiliated to CBSE.",
                Address = spec.Address,
                Phone = spec.Phone,
                Email = spec.Email,
                MapEmbedUrl =
                    $"https://maps.google.com/maps?q={Uri.EscapeDataString(spec.City)}&t=&z=13&ie=UTF8&iwloc=&output=embed",
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "showcase"
            });

            db.HomePageSections.AddRange(ShowcaseSections.Build(tenantId, spec));
            db.ContentEntries.AddRange(ShowcaseContent.Build(tenantId, spec));

            var pages = ShowcasePages.Build(tenantId, spec);
            db.Pages.AddRange(pages);

            db.Menus.Add(new Menu
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                SiteId = spec.SiteId,
                Name = "Main navigation",
                Location = "header",
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "showcase",
                Items = BuildMenu(tenantId, spec.SiteId, pages)
            });

            db.SeoSettings.Add(new SeoSetting
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                SiteId = spec.SiteId,
                MetaTitle = $"{spec.Name} · {spec.City}",
                MetaDescription = spec.Tagline,
                MetaKeywords = $"school, {spec.City}, CBSE, admission, {spec.ShortName}",
                OgImageUrl = spec.Banner,
                AllowIndexing = false,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "showcase"
            });

            created++;
        }

        if (created > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Seeded {Count} showcase website(s).", created);
        }
    }

    private static List<MenuItem> BuildMenu(Guid tenantId, Guid siteId, List<Page> pages)
    {
        var items = new List<MenuItem>
        {
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                SiteId = siteId,
                Label = "Home",
                Url = "/",
                DisplayOrder = 0,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "showcase"
            }
        };

        items.AddRange(pages.Select((p, i) => new MenuItem
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            SiteId = siteId,
            Label = p.Title,
            Url = $"/{p.Slug}",
            DisplayOrder = i + 1,
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "showcase"
        }));

        return items;
    }
}
