namespace Cms.Application.Interfaces;

public interface ISiteContext
{
    Guid? SiteId { get; }
    string? SiteKey { get; }
    string? SiteName { get; }

    /// <summary>
    /// URL prefix the public website is served under for the current host.
    /// Empty when the host is bound to a single website (e.g. <c>school.example.edu/about</c>);
    /// <c>/{siteKey}</c> when one host serves several websites (e.g. <c>example.edu/school/about</c>).
    /// </summary>
    string BasePath { get; }

    bool IsResolved { get; }
    void Set(Guid siteId, string siteKey, string siteName, string basePath = "");
}
