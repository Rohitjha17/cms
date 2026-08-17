using Microsoft.AspNetCore.Http;

namespace Cms.Infrastructure.Http;

/// <summary>
/// Takes the application's mount point from the request rather than from configuration.
///
/// One deployment can serve the public website at two different places at once: under a prefix
/// on the platform's own host (<c>cms.example.com/site/school</c>), and at the root of a school's
/// own domain (<c>cambridge.edu.in/</c>). A fixed <c>PathBase</c> setting cannot express that,
/// because it applies to the whole process — so the proxy states the prefix per request with
/// <c>X-Forwarded-Prefix</c>, and this reads it.
///
/// The header is only believed when the proxy in front is trusted, since it decides what every
/// link on the page is prefixed with. <c>PathBase</c> remains as the fallback for running the
/// site directly, without a proxy.
/// </summary>
public sealed class ForwardedPrefixMiddleware
{
    private const string HeaderName = "X-Forwarded-Prefix";
    private readonly RequestDelegate _next;
    private readonly bool _trustProxy;
    private readonly string? _configuredPrefix;

    // configuredPrefix is not nullable: a null argument cannot be matched to a constructor
    // parameter when the middleware is activated, which fails at startup rather than at compile
    // time — and it is unset in exactly the deployment this exists for.
    public ForwardedPrefixMiddleware(RequestDelegate next, bool trustProxy, string configuredPrefix)
    {
        _next = next;
        _trustProxy = trustProxy;
        _configuredPrefix = Normalise(configuredPrefix);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var prefix = _trustProxy && context.Request.Headers.TryGetValue(HeaderName, out var header)
            ? Normalise(header.ToString())
            : _configuredPrefix;

        if (prefix is not null && context.Request.Path.StartsWithSegments(prefix, out var remainder))
        {
            context.Request.PathBase = context.Request.PathBase.Add(prefix);
            context.Request.Path = remainder.HasValue ? remainder : "/";

            // Endpoint matching runs ahead of this middleware, so anything already matched was
            // matched against the un-rewritten path. Drop it and let routing choose again.
            context.SetEndpoint(null);
        }

        await _next(context);
    }

    /// <summary>
    /// Accepts a single rooted segment such as <c>/site</c>. Anything else — traversal, a
    /// scheme, an absurd length — is discarded rather than trusted into every generated link.
    /// </summary>
    private static string? Normalise(string? value)
    {
        var trimmed = value?.Trim().TrimEnd('/');
        if (string.IsNullOrEmpty(trimmed) || trimmed.Length > 100)
        {
            return null;
        }

        if (!trimmed.StartsWith('/'))
        {
            trimmed = "/" + trimmed;
        }

        return trimmed.Contains("..", StringComparison.Ordinal)
            || trimmed.Contains("//", StringComparison.Ordinal)
            || trimmed.Contains(':', StringComparison.Ordinal)
            ? null
            : trimmed;
    }
}
