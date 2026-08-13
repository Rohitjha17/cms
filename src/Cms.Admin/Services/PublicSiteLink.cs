using Cms.Application.Interfaces;

namespace Cms.Admin.Services;

/// <summary>
/// URLs of the published websites for the workspace being edited.
///
/// <c>PublicSite:BaseUrl</c> accepts either shape:
///   * a rooted path such as <c>/site</c> — the public website is served by this same
///     deployment, so links are built from the current request's own scheme and host;
///   * an absolute <c>https://…</c> URL — the public website is a separate deployment.
///
/// Anything else is ignored and the links are hidden, because a malformed value sends
/// editors to a dead address that looks like a broken product rather than a mis-set variable.
/// </summary>
public interface IPublicSiteLink
{
    /// <summary>The website currently selected in the console, or null when unavailable.</summary>
    string? Url { get; }

    /// <summary>
    /// The URL of one specific website. A website bound to its own domain is reached there;
    /// otherwise it is reached by its site-key path on the shared host.
    /// </summary>
    string? ForSite(string? siteKey, IEnumerable<string>? boundDomains = null);
}

public sealed class PublicSiteLink : IPublicSiteLink
{
    private readonly string? _origin;

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

        if (configured.StartsWith('/'))
        {
            // Same deployment: anchor the path to whichever host the operator is actually on,
            // so links are correct on localhost, on a preview URL and on a custom domain
            // without being reconfigured for each.
            var request = httpContextAccessor.HttpContext?.Request;
            if (request is null)
            {
                return;
            }

            _origin = $"{request.Scheme}://{request.Host}{configured}";
        }
        else if (Uri.TryCreate(configured, UriKind.Absolute, out var absolute)
                 && (absolute.Scheme == Uri.UriSchemeHttp || absolute.Scheme == Uri.UriSchemeHttps))
        {
            _origin = configured;
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

        Url = _origin + basePath;
    }

    public string? Url { get; }

    public string? ForSite(string? siteKey, IEnumerable<string>? boundDomains = null)
    {
        // A website with its own domain is reached there, whatever this console is served from.
        var domain = boundDomains?.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
        if (!string.IsNullOrWhiteSpace(domain))
        {
            return $"https://{domain.Trim().TrimEnd('/')}";
        }

        if (_origin is null)
        {
            return null;
        }

        return string.IsNullOrWhiteSpace(siteKey) ? _origin : $"{_origin}/{siteKey.Trim('/')}";
    }
}
