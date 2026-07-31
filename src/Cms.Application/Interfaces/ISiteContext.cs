namespace Cms.Application.Interfaces;

public interface ISiteContext
{
    Guid? SiteId { get; }
    string? SiteKey { get; }
    string? SiteName { get; }
    bool IsResolved { get; }
    void Set(Guid siteId, string siteKey, string siteName);
}
