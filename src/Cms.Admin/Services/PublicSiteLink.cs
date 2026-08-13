using Cms.Application.Interfaces;

namespace Cms.Admin.Services;

/// <summary>
/// URL of the published website for the workspace currently being edited.
///
/// <c>PublicSite:BaseUrl</c> accepts either shape:
///   * a rooted path such as <c>/site</c> — the public website is served by this same
///     deployment, so the link is built from the current request's own scheme and host;
///   * an absolute <c>https://…</c> URL — the public website is a separate deployment.
///
/// Anything else is ignored and the link is hidden, because a malformed value sends editors
/// to a dead address that looks like a broken product rather than a mis-set variable.
/// </summary>
public interface IPublicSiteLink
{
    string? Url { get; }
}

public sealed class PublicSiteLink : IPublicSiteLink
{
    public PublicSiteLink(
        IConfiguration configuration,
        ISiteContext siteContext,
        IHttpContextAccessor httpContextAccessor,
        ILogger<PublicSiteLink> logger)
    {
        var configured = configuration["PublicSite:BaseUrl"]?.Trim();
        if (string.IsNullOrWhiteSpace(configured))
        {
            return;
        }

        configured = configured.TrimEnd('/');

        string origin;
        if (configured.StartsWith('/'))
        {
            // Same deployment: anchor the path to whichever host the operator is actually on,
            // so the link is correct on localhost, on a preview URL and on a custom domain
            // without needing to be reconfigured for each.
            var request = httpContextAccessor.HttpContext?.Request;
            if (request is null)
            {
                return;
            }

            origin = $"{request.Scheme}://{request.Host}{configured}";
        }
        else if (Uri.TryCreate(configured, UriKind.Absolute, out var absolute)
                 && (absolute.Scheme == Uri.UriSchemeHttp || absolute.Scheme == Uri.UriSchemeHttps))
        {
            origin = configured;
        }
        else
        {
            logger.LogWarning(
                "PublicSite:BaseUrl is '{Value}', which is neither a rooted path such as '/site' "
                + "nor an absolute http(s) URL. The 'view live site' links are hidden.",
                configured);
            return;
        }

        var basePath = siteContext.IsResolved && !string.IsNullOrEmpty(siteContext.BasePath)
            ? siteContext.BasePath
            : "/";

        Url = origin + basePath;
    }

    public string? Url { get; }
}
