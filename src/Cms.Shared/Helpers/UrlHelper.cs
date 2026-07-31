namespace Cms.Shared.Helpers;

public static class UrlHelper
{
    public static bool IsValidUrl(string? url, bool allowRelative = true)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return true;
        }

        if (Uri.TryCreate(url, UriKind.Absolute, out var absolute)
            && (absolute.Scheme == Uri.UriSchemeHttp || absolute.Scheme == Uri.UriSchemeHttps))
        {
            return true;
        }

        return allowRelative && Uri.TryCreate(url, UriKind.Relative, out _);
    }
}
