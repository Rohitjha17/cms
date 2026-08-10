using Cms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace Cms.Infrastructure.Tenancy;

public sealed record ResolvedSite(Guid Id, string SiteKey, string Name, bool IsDefault);

public sealed record ResolvedHost(
    Guid TenantId,
    string TenantCode,
    string TenantName,
    Guid? BoundSiteId,
    IReadOnlyList<ResolvedSite> Sites);

public interface ITenantHostResolver
{
    Task<ResolvedHost?> ResolveAsync(string host, CancellationToken cancellationToken = default);
}

/// <summary>
/// Resolves an incoming host to its tenant and websites.
///
/// Every request needs this lookup, so results are cached for a short, configurable window
/// (<c>Tenancy:ResolutionCacheSeconds</c>, 0 disables). A newly bound domain therefore
/// becomes reachable within that window rather than instantly.
/// </summary>
public sealed class TenantHostResolver : ITenantHostResolver
{
    private readonly ApplicationDbContext _db;
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheDuration;
    private readonly bool _demoFallback;

    public TenantHostResolver(ApplicationDbContext db, IMemoryCache cache, IConfiguration configuration)
    {
        _db = db;
        _cache = cache;
        _cacheDuration = TimeSpan.FromSeconds(configuration.GetValue("Tenancy:ResolutionCacheSeconds", 30));
        _demoFallback = configuration.GetValue<bool>("DemoMode:Enabled");
    }

    public async Task<ResolvedHost?> ResolveAsync(string host, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"tenant-host::{host}";
        if (_cacheDuration > TimeSpan.Zero && _cache.TryGetValue<ResolvedHost?>(cacheKey, out var cached))
        {
            return cached;
        }

        var resolved = await LoadAsync(host, cancellationToken);
        if (_cacheDuration > TimeSpan.Zero)
        {
            _cache.Set(cacheKey, resolved, _cacheDuration);
        }

        return resolved;
    }

    private async Task<ResolvedHost?> LoadAsync(string host, CancellationToken cancellationToken)
    {
        var domain = await _db.TenantDomains
            .IgnoreQueryFilters()
            .Include(d => d.Tenant)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.DomainName == host && d.IsActive && d.Tenant.IsActive, cancellationToken);

        var tenant = domain?.Tenant;
        if (tenant is null && _demoFallback)
        {
            tenant = await _db.Tenants
                .IgnoreQueryFilters()
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.Code == "demo" && item.IsActive, cancellationToken);
        }

        if (tenant is null)
        {
            return null;
        }

        var sites = await _db.Sites
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(s => s.TenantId == tenant.Id && s.IsActive)
            .OrderByDescending(s => s.IsDefault)
            .ThenBy(s => s.Name)
            .Select(s => new ResolvedSite(s.Id, s.SiteKey, s.Name, s.IsDefault))
            .ToListAsync(cancellationToken);

        // A domain may still point at a site that was since deactivated.
        Guid? boundSiteId = domain?.SiteId is Guid bound && sites.Any(s => s.Id == bound) ? bound : null;

        return new ResolvedHost(tenant.Id, tenant.Code, tenant.Name, boundSiteId, sites);
    }
}
