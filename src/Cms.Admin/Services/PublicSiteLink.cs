using Cms.Application.Interfaces;

namespace Cms.Admin.Services;

/// <summary>
/// URLs of the published websites for the workspace being edited.
///
/// Two settings, in precedence order:
///   1. <c>PublicSite:PathBase</c> — e.g. <c>/site</c>. Set by the deployment itself when the
///      public website is served by this same process, so links are built from whichever host
///      the operator is actually on. This wins, because "same host, this path" is a fact the
///      container knows about itself and cannot be wrong about.
///   2. <c>PublicSite:BaseUrl</c> — an absolute <c>https://…</c> URL, for when the public site
///      is a genuinely separate deployment.
///
/// The precedence matters: an operator who once set BaseUrl to a placeholder host would
/// otherwise keep sending editors to a dead address, and nothing in the product could
/// recover from it.
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
        var pathBase = configuration["PublicSite:PathBase"]?.Trim().TrimEnd('/');
        var configured = configuration["PublicSite:BaseUrl"]?.Trim().TrimEnd('/');

        // A same-host path base always wins over a configured external URL.
        var sameHostPath = !string.IsNullOrWhiteSpace(pathBase) && pathBase.StartsWith('/')
            ? pathBase
            : !string.IsNullOrWhiteSpace(configured) && configured.StartsWith('/')
                ? configured
                : null;

        if (sameHostPath is not null)
        {
            // Anchor to whichever host the operator is actually on, so links are correct on
            // localhost, on a preview URL and on a custom domain with no reconfiguration.
            var request = httpContextAccessor.HttpContext?.Request;
            if (request is null)
            {
                return;
            }

            _origin = $"{request.Scheme}://{request.Host}{sameHostPath}";
        }
        else if (string.IsNullOrWhiteSpace(configured))
        {
            return;
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
