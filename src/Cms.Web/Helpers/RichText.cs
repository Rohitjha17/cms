using System.Net;
using System.Text.RegularExpressions;

namespace Cms.Web.Helpers;

/// <summary>
/// The hero's supporting line is a single sentence in a paragraph tag, but the field behind it is
/// a rich-text editor. Rendering its markup there would either show the tags to visitors or nest
/// a paragraph inside a paragraph, so the hero takes the words and leaves the formatting.
/// </summary>
public static partial class RichText
{
    public static string? ToPlainText(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return null;
        }

        // Block boundaries become spaces, so two paragraphs do not run their words together.
        var spaced = BlockBoundary().Replace(html, " ");
        var text = WebUtility.HtmlDecode(Tags().Replace(spaced, string.Empty));
        text = Whitespace().Replace(text, " ").Trim();

        return string.IsNullOrWhiteSpace(text) ? null : text;
    }

    [GeneratedRegex("</(p|div|li|h[1-6]|blockquote)>|<br\\s*/?>", RegexOptions.IgnoreCase)]
    private static partial Regex BlockBoundary();

    [GeneratedRegex("<[^>]+>")]
    private static partial Regex Tags();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();
}
