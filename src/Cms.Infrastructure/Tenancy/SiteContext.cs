using Cms.Application.Interfaces;

namespace Cms.Infrastructure.Tenancy;

public class SiteContext : ISiteContext
{
    public Guid? SiteId { get; private set; }
    public string? SiteKey { get; private set; }
    public string? SiteName { get; private set; }
    public bool IsResolved => SiteId.HasValue;

    public void Set(Guid siteId, string siteKey, string siteName)
    {
        SiteId = siteId;
        SiteKey = siteKey;
        SiteName = siteName;
    }
}
