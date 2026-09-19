using System.Net;
using System.Text.RegularExpressions;

namespace Cms.Web.Helpers;

/// <summary>
/// Turns what a school typed into its own-HTML box into a document a frame can load.
///
/// A school may paste a whole page — doctype, head, stylesheets, scripts — or only the part
/// that goes in the body. A whole page is served as written. A fragment is given the smallest
/// document that renders it faithfully: a charset, a viewport so it lays out at the phone's
/// width rather than a desktop's, and no margin, so it meets the edges of the frame instead of
/// sitting inside the browser's default eight pixels.
///
/// Either way one small script is added at the end: the bridge to the website around it.
/// </summary>
public static partial class CustomHtmlDocument
{
    /// <summary>
    /// What the school's page may do inside its sandbox: run its own scripts, open links and
    /// pop-ups, show dialogs, offer downloads, and take the whole window to another page when a
    /// visitor clicks. What it may not do is be the website: no <c>allow-same-origin</c>, so it
    /// runs in an origin of its own and nothing of the website's is within its reach.
    /// </summary>
    public const string SandboxFlags =
        "allow-scripts allow-forms allow-popups allow-popups-to-escape-sandbox " +
        "allow-modals allow-downloads allow-top-navigation-by-user-activation";

    /// <summary>
    /// The frame lives in an origin of its own, so the page around it cannot look inside to
    /// measure it or to find a section. The document tells the page instead: how tall it is,
    /// whenever that changes, and where a section is when a visitor follows a link to it —
    /// because the frame is as tall as its content and never scrolls, so a jump inside it goes
    /// nowhere, and it is the page that has to scroll. A link to anywhere else opens as the
    /// whole page rather than inside the frame.
    /// </summary>
    private const string Bridge = """
        <script>
        (() => {
          if (window.parent === window) return;
          const post = message => window.parent.postMessage(Object.assign({ cmsFrame: true }, message), "*");
          const height = () => Math.max(
            document.documentElement.offsetHeight,
            document.body ? document.body.scrollHeight : 0);
          let last = 0;
          const report = () => {
            const h = height();
            if (Math.abs(h - last) < 2) return;
            last = h;
            post({ type: "height", height: h });
          };
          addEventListener("load", report);
          if (document.fonts) document.fonts.ready.then(report);
          new ResizeObserver(report).observe(document.documentElement);
          report();

          document.addEventListener("click", event => {
            const link = event.target.closest && event.target.closest("a[href]");
            if (!link || link.hasAttribute("download")) return;
            const href = link.getAttribute("href") || "";
            if (href.startsWith("#") && href.length > 1) {
              const id = decodeURIComponent(href.slice(1));
              const section = document.getElementById(id) || document.getElementsByName(id)[0];
              if (!section) return;
              event.preventDefault();
              post({ type: "jump", top: section.getBoundingClientRect().top + scrollY });
              return;
            }
            if (link.target || href.toLowerCase().startsWith("javascript:")) return;
            link.target = "_top";
          }, true);
        })();
        </script>
        """;

    public static string Build(string? html, string? title)
    {
        var markup = html ?? string.Empty;
        if (IsWholeDocument().IsMatch(markup))
        {
            var close = markup.LastIndexOf("</body>", StringComparison.OrdinalIgnoreCase);
            return close < 0 ? markup + Bridge : markup.Insert(close, Bridge);
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
            {{Bridge}}
            </body>
            </html>
            """;
    }

    [GeneratedRegex(@"^\s*(<!--.*?-->\s*)*(<!doctype\s+html|<html[\s>])", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex IsWholeDocument();
}
