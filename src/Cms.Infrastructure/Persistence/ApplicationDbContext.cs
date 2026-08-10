using Cms.Application.Interfaces;
using Cms.Domain.Entities;
using Cms.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Cms.Domain.Common;
using System.Text.Json;

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
    public DbSet<PageTemplate> PageTemplates => Set<PageTemplate>();
    public DbSet<ContactSubmission> ContactSubmissions => Set<ContactSubmission>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();

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
        builder.Entity<ContactSubmission>().HasQueryFilter(e =>
            _tenantContext.TenantId.HasValue && e.TenantId == _tenantContext.TenantId
            && _siteContext.SiteId.HasValue && e.SiteId == _siteContext.SiteId);
        builder.Entity<TenantDomain>().HasQueryFilter(e =>
            _tenantContext.TenantId.HasValue && e.TenantId == _tenantContext.TenantId);
        builder.Entity<Site>().HasQueryFilter(e =>
            _tenantContext.TenantId.HasValue && e.TenantId == _tenantContext.TenantId);
        builder.Entity<ActivityLog>().HasQueryFilter(e =>
            _tenantContext.TenantId.HasValue && e.TenantId == _tenantContext.TenantId);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        AddActivityLogs();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        AddActivityLogs();
        return base.SaveChanges();
    }

    private void AddActivityLogs()
    {
        if (!_tenantContext.TenantId.HasValue) return;

        var entries = ChangeTracker.Entries<BaseEntity>()
            .Where(x => x.Entity is not ActivityLog
                && x.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();
        foreach (var entry in entries)
        {
            var changed = entry.State == EntityState.Modified
                ? entry.Properties.Where(p => p.IsModified).Select(p => p.Metadata.Name).OrderBy(x => x).ToArray()
                : Array.Empty<string>();
            var siteId = entry.Entity is ISiteEntity siteEntity ? siteEntity.SiteId : _siteContext.SiteId;
            var actor = entry.Entity.UpdatedBy ?? entry.Entity.CreatedBy ?? "system";
            ActivityLogs.Add(new ActivityLog
            {
                TenantId = _tenantContext.TenantId.Value,
                SiteId = siteId,
                ActorId = actor,
                Action = entry.State.ToString(),
                EntityType = entry.Metadata.ClrType.Name,
                EntityId = entry.Entity.Id.ToString(),
                ChangedProperties = changed.Length == 0 ? null : JsonSerializer.Serialize(changed),
                CreatedBy = actor
            });
        }
    }
}
