using Cms.Application.Interfaces;
using Cms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace Cms.Infrastructure.Tenancy;

/// <summary>
/// Invalidates every cached host lookup at once by moving the generation that cache keys are
/// built from. Superseded entries are never read again and fall out on their own expiry.
/// </summary>
public sealed class TenantHostCache : ITenantHostCache
{
    private long _generation;

    public long Generation => Interlocked.Read(ref _generation);

    public void Invalidate() => Interlocked.Increment(ref _generation);
}

public sealed record ResolvedSite(Guid Id, string SiteKey, string Name, bool IsDefault);

public sealed record ResolvedHost(
    Guid TenantId,
    string TenantCode,
    string TenantName,
    Guid? BoundSiteId,
    IReadOnlyList<ResolvedSite> Sites);

public interface ITenantHostResolver
{
    /// <param name="refresh">
    /// Skips the cache and re-reads from the database. Used when a caller names a website the
    /// cached answer does not contain — a site created seconds ago, or created by the other
    /// process sharing this database — so that it is found rather than quietly swapped for
    /// whichever website the host serves by default.
    /// </param>
    Task<ResolvedHost?> ResolveAsync(
        string host, bool refresh = false, CancellationToken cancellationToken = default);
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
    private readonly TenantHostCache _generation;
    private readonly TimeSpan _cacheDuration;
    private readonly bool _demoFallback;

    public TenantHostResolver(
        ApplicationDbContext db,
        IMemoryCache cache,
        TenantHostCache generation,
        IConfiguration configuration)
    {
        _db = db;
        _cache = cache;
        _generation = generation;
        _cacheDuration = TimeSpan.FromSeconds(configuration.GetValue("Tenancy:ResolutionCacheSeconds", 30));
        _demoFallback = configuration.GetValue<bool>("DemoMode:Enabled");
    }

    public async Task<ResolvedHost?> ResolveAsync(
        string host, bool refresh = false, CancellationToken cancellationToken = default)
    {
        // The generation is part of the key, so creating a website or binding a domain makes the
        // change visible on the next request instead of after the cache window.
        var cacheKey = $"tenant-host::{_generation.Generation}::{host}";
        if (!refresh
            && _cacheDuration > TimeSpan.Zero
            && _cache.TryGetValue<ResolvedHost?>(cacheKey, out var cached))
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
