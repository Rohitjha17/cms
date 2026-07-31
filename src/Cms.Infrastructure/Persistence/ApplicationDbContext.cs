using Cms.Application.Interfaces;
using Cms.Domain.Entities;
using Cms.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Cms.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    private readonly ITenantContext _tenantContext;
    private readonly ISiteContext _siteContext;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ITenantContext tenantContext,
        ISiteContext siteContext) : base(options)
    {
        _tenantContext = tenantContext;
        _siteContext = siteContext;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<TenantDomain> TenantDomains => Set<TenantDomain>();
    public DbSet<Site> Sites => Set<Site>();
    public DbSet<HomePageSection> HomePageSections => Set<HomePageSection>();
    public DbSet<MediaFile> MediaFiles => Set<MediaFile>();
    public DbSet<Page> Pages => Set<Page>();
    public DbSet<Menu> Menus => Set<Menu>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<SeoSetting> SeoSettings => Set<SeoSetting>();
    public DbSet<ContentEntry> ContentEntries => Set<ContentEntry>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Tenant isolation — evaluates current scoped tenant at query time.
        builder.Entity<HomePageSection>().HasQueryFilter(e =>
            _tenantContext.TenantId.HasValue && e.TenantId == _tenantContext.TenantId
            && _siteContext.SiteId.HasValue && e.SiteId == _siteContext.SiteId);
        builder.Entity<MediaFile>().HasQueryFilter(e =>
            _tenantContext.TenantId.HasValue && e.TenantId == _tenantContext.TenantId
            && _siteContext.SiteId.HasValue && e.SiteId == _siteContext.SiteId);
        builder.Entity<Page>().HasQueryFilter(e =>
            _tenantContext.TenantId.HasValue && e.TenantId == _tenantContext.TenantId
            && _siteContext.SiteId.HasValue && e.SiteId == _siteContext.SiteId);
        builder.Entity<Menu>().HasQueryFilter(e =>
            _tenantContext.TenantId.HasValue && e.TenantId == _tenantContext.TenantId
            && _siteContext.SiteId.HasValue && e.SiteId == _siteContext.SiteId);
        builder.Entity<MenuItem>().HasQueryFilter(e =>
            _tenantContext.TenantId.HasValue && e.TenantId == _tenantContext.TenantId
            && _siteContext.SiteId.HasValue && e.SiteId == _siteContext.SiteId);
        builder.Entity<SeoSetting>().HasQueryFilter(e =>
            _tenantContext.TenantId.HasValue && e.TenantId == _tenantContext.TenantId
            && _siteContext.SiteId.HasValue && e.SiteId == _siteContext.SiteId);
        builder.Entity<ContentEntry>().HasQueryFilter(e =>
            _tenantContext.TenantId.HasValue && e.TenantId == _tenantContext.TenantId
            && _siteContext.SiteId.HasValue && e.SiteId == _siteContext.SiteId);
        builder.Entity<TenantDomain>().HasQueryFilter(e =>
            _tenantContext.TenantId.HasValue && e.TenantId == _tenantContext.TenantId);
        builder.Entity<Site>().HasQueryFilter(e =>
            _tenantContext.TenantId.HasValue && e.TenantId == _tenantContext.TenantId);
    }
}
