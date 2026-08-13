namespace Cms.Application.Interfaces;

/// <summary>
/// Host → tenant/website resolution is cached per request-host for a short window, because every
/// single public request depends on it. Anything that changes which websites a host serves — a new
/// website, a domain bound or removed — must call <see cref="Invalidate"/>, otherwise the operator
/// creates a site, opens its link straight away and gets someone else's website with a 404 on it.
/// </summary>
public interface ITenantHostCache
{
    void Invalidate();
}
