using Cms.Application.Interfaces;

namespace Cms.Admin.Services;

/// <summary>
/// Absolute URL of the published website for the workspace currently being edited.
///
/// The Admin console and the public site are separate deployments, so the public origin
/// has to be supplied by configuration (<c>PublicSite:BaseUrl</c>). When it is absent the
/// link is hidden rather than guessed — a hard-coded localhost address would ship to
/// production and send editors nowhere.
/// </summary>
public interface IPublicSiteLink
{
    string? Url { get; }
}

public sealed class PublicSiteLink : IPublicSiteLink
{
    public PublicSiteLink(IConfiguration configuration, ISiteContext siteContext)
    {
        var baseUrl = configuration["PublicSite:BaseUrl"]?.Trim().TrimEnd('/');
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return;
        }

        var basePath = siteContext.IsResolved && !string.IsNullOrEmpty(siteContext.BasePath)
            ? siteContext.BasePath
            : "/";
        Url = baseUrl + basePath;
    }

    public string? Url { get; }
}
