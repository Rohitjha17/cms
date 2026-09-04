using System.Text.RegularExpressions;

namespace Cms.Web.Helpers;

/// <summary>
/// Turns whatever a school puts in the map field into a map that actually draws.
///
/// The field used to be handed straight to an iframe, which only works for one thing: the
/// address hidden inside Google's "Embed a map" HTML. Everything else a person naturally
/// reaches for — the link in the address bar, the Share button's short link, the whole iframe
/// tag copied wholesale, or simply the school's address — produced an empty grey box, because
/// Google refuses to let its ordinary map pages be framed. Nothing on the screen said so.
///
/// Only ever returns a Google Maps address built here, never the school's text passed through,
/// so nothing typed into this field can become a frame pointing somewhere else.
/// </summary>
public static class MapEmbed
{
    /// <summary>Keyless embed endpoint. "output=embed" is what makes it framable.</summary>
    private const string Query = "https://maps.google.com/maps?output=embed&q=";

    private static readonly Regex IframeSrc =
        new("""src\s*=\s*["']([^"']+)["']""", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <summary>Google writes the centre into the path as "@lat,lng,17z".</summary>
    private static readonly Regex Centre =
        new(@"@(-?\d+\.\d+),(-?\d+\.\d+)(?:,(\d+(?:\.\d+)?)z)?", RegexOptions.Compiled);

    /// <param name="value">What was typed into the map field.</param>
    /// <param name="address">
    /// The school's postal address, used when the map field holds something that cannot be
    /// turned into a map — or nothing at all. A school that filled in its address and never
    /// touched the map field still gets a map.
    /// </param>
    public static string? Read(string? value, string? address = null)
    {
        var text = value?.Trim();

        if (!string.IsNullOrEmpty(text))
        {
            // Someone pasted the whole <iframe …> tag rather than the address inside it.
            var tag = IframeSrc.Match(text);
            if (tag.Success)
            {
                text = System.Net.WebUtility.HtmlDecode(tag.Groups[1].Value).Trim();
            }

            var built = FromLink(text);
            if (built is not null)
            {
                return built;
            }

            // Not a link at all — treat it as a place to look up.
            if (!text.Contains("://", StringComparison.Ordinal))
            {
                return Query + Uri.EscapeDataString(text);
            }
        }

        return string.IsNullOrWhiteSpace(address) ? null : Query + Uri.EscapeDataString(address.Trim());
    }

    private static string? FromLink(string text)
    {
        if (!Uri.TryCreate(text, UriKind.Absolute, out var url)
            || (url.Scheme != Uri.UriSchemeHttps && url.Scheme != Uri.UriSchemeHttp))
        {
            return null;
        }

        var host = url.Host.ToLowerInvariant().Replace("www.", string.Empty);
        var isGoogleMap = host is "google.com" or "maps.google.com" or "google.co.in"
            || host.StartsWith("google.", StringComparison.Ordinal)
            || host.StartsWith("maps.google.", StringComparison.Ordinal);

        if (!isGoogleMap)
        {
            // Short links (maps.app.goo.gl, goo.gl/maps) cannot be expanded without asking
            // Google, and a page must not make an outbound request to draw itself. The caller
            // falls back to the school's address, which is the same place anyway.
            return null;
        }

        // Already the embed address, from "Embed a map". Keep it exactly as it is.
        if (url.AbsolutePath.StartsWith("/maps/embed", StringComparison.OrdinalIgnoreCase))
        {
            return url.ToString();
        }

        // The address bar's link carries the centre of the map in it.
        var centre = Centre.Match(url.AbsolutePath);
        if (centre.Success)
        {
            var point = $"{centre.Groups[1].Value},{centre.Groups[2].Value}";
            var zoom = centre.Groups[3].Success ? centre.Groups[3].Value.Split('.')[0] : "16";
            return $"{Query}{Uri.EscapeDataString(point)}&z={zoom}";
        }

        // ".../maps/place/Cambridge+School+Noida/..." — the name is enough to find it.
        var segments = url.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var place = Array.IndexOf(segments, "place");
        if (place >= 0 && place + 1 < segments.Length)
        {
            var name = Uri.UnescapeDataString(segments[place + 1]).Replace('+', ' ');
            return Query + Uri.EscapeDataString(name);
        }

        // ".../maps?q=…" — whatever they searched for.
        foreach (var pair in url.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = pair.Split('=', 2);
            if (parts.Length == 2 && parts[0] is "q" or "query" && parts[1].Length > 0)
            {
                return Query + Uri.EscapeDataString(Uri.UnescapeDataString(parts[1]).Replace('+', ' '));
            }
        }

        return null;
    }
}
