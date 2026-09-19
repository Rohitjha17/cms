using System.Net;
using System.Text.RegularExpressions;

namespace Cms.Web.Helpers;

/// <summary>
/// Turns what a school typed into its own-HTML box into a document a frame can load.
///
/// A school may paste a whole page — doctype, head, stylesheets, scripts — or only the part
/// that goes in the body. A whole page is served exactly as written. A fragment is given the
/// smallest document that renders it faithfully: a charset, a viewport so it lays out at the
/// phone's width rather than a desktop's, and no margin, so it meets the edges of the frame
/// instead of sitting inside the browser's default eight pixels.
/// </summary>
public static partial class CustomHtmlDocument
{
    public static string Build(string? html, string? title)
    {
        var markup = html ?? string.Empty;
        if (IsWholeDocument().IsMatch(markup))
        {
            return markup;
        }

        return $$"""
            <!doctype html>
            <html lang="en">
            <head>
            <meta charset="utf-8" />
            <meta name="viewport" content="width=device-width, initial-scale=1" />
            <title>{{WebUtility.HtmlEncode(title ?? string.Empty)}}</title>
            <style>html,body{margin:0}</style>
            </head>
            <body>
            {{markup}}
            </body>
            </html>
            """;
    }

    [GeneratedRegex(@"^\s*(<!--.*?-->\s*)*(<!doctype\s+html|<html[\s>])", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex IsWholeDocument();
}
