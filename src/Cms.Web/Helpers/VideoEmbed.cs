namespace Cms.Web.Helpers;

/// <summary>What a school's video link turns into on the page.</summary>
/// <param name="EmbedUrl">A player URL for an iframe, or null when the file plays directly.</param>
/// <param name="FileUrl">A media file to hand to a &lt;video&gt; element, or null.</param>
/// <param name="PosterUrl">A thumbnail the provider publishes, when there is one.</param>
public sealed record VideoSource(string? EmbedUrl, string? FileUrl, string? PosterUrl);

/// <summary>
/// Turns the link a school pastes in — a YouTube page, a share link, a Vimeo address, or an
/// uploaded file — into something the page can play.
///
/// Only hosts named here are ever built into an iframe. A link this cannot read is not guessed
/// at and not passed through: an unknown address in a frame is somebody else's page running
/// inside the school's.
/// </summary>
public static class VideoEmbed
{
    private static readonly string[] PlayableFiles = [".mp4", ".webm", ".ogv", ".ogg", ".mov", ".m4v"];

    public static VideoSource? Read(string? link)
    {
        if (string.IsNullOrWhiteSpace(link)) return null;

        var trimmed = link.Trim();

        // A path is the school's own upload, and plays directly. This is checked before parsing
        // as a URI because on Unix "/uploads/tour.mp4" parses as an absolute file:// address,
        // which then fails the scheme check and loses the video.
        if (trimmed.StartsWith('/'))
        {
            return !trimmed.StartsWith("//", StringComparison.Ordinal) && HasPlayableExtension(trimmed)
                ? new VideoSource(null, trimmed, null)
                : null;
        }

        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var url)) return null;
        if (url.Scheme != Uri.UriSchemeHttps && url.Scheme != Uri.UriSchemeHttp) return null;

        var host = url.Host.ToLowerInvariant();
        if (host.StartsWith("www.", StringComparison.Ordinal)) { host = host[4..]; }

        var id = host switch
        {
            "youtu.be" => url.AbsolutePath.Trim('/'),
            "youtube.com" or "m.youtube.com" or "youtube-nocookie.com" => YouTubeId(url),
            _ => null
        };

        if (!string.IsNullOrEmpty(id) && IsSafeId(id))
        {
            // nocookie so a visitor who only looked at the page is not tracked by it.
            return new VideoSource(
                $"https://www.youtube-nocookie.com/embed/{id}?rel=0",
                null,
                $"https://i.ytimg.com/vi/{id}/hqdefault.jpg");
        }

        if (host is "vimeo.com" or "player.vimeo.com")
        {
            var segment = url.AbsolutePath.Trim('/').Split('/').LastOrDefault();
            if (!string.IsNullOrEmpty(segment) && segment.All(char.IsDigit))
            {
                return new VideoSource($"https://player.vimeo.com/video/{segment}", null, null);
            }
        }

        return HasPlayableExtension(url.AbsolutePath)
            ? new VideoSource(null, url.ToString(), null)
            : null;
    }

    private static string? YouTubeId(Uri url)
    {
        if (url.AbsolutePath.StartsWith("/embed/", StringComparison.Ordinal))
        {
            return url.AbsolutePath["/embed/".Length..].Trim('/');
        }

        if (url.AbsolutePath.StartsWith("/shorts/", StringComparison.Ordinal))
        {
            return url.AbsolutePath["/shorts/".Length..].Trim('/');
        }

        foreach (var pair in url.Query.TrimStart('?').Split('&'))
        {
            var parts = pair.Split('=', 2);
            if (parts.Length == 2 && parts[0] == "v") return Uri.UnescapeDataString(parts[1]);
        }

        return null;
    }

    /// <summary>A video id is letters, digits, dash and underscore. Anything else is not one.</summary>
    private static bool IsSafeId(string id) =>
        id.Length is > 0 and <= 24 && id.All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_');

    private static bool HasPlayableExtension(string path) =>
        PlayableFiles.Any(x => path.EndsWith(x, StringComparison.OrdinalIgnoreCase));
}
