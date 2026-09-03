namespace Cms.Web.Helpers;

/// <summary>How a backdrop is drawn: the image itself, how big one tile is, and whether it moves.</summary>
public sealed record BackdropStyle(string Image, string Size, string? Animation = null);

/// <summary>
/// Turns a section's chosen backdrop into CSS, safely.
///
/// The values come from the console, so they reach a stylesheet the school did not write and
/// cannot be trusted into one unedited: a quote or a brace would close the rule early and let
/// whatever follows through. Patterns are chosen from a fixed list; a photograph's address is
/// stripped of anything that could end the url().
///
/// Everything here is drawn with gradients rather than downloaded. A decorative background that
/// costs a request and a megabyte is a background that makes the page worse.
/// </summary>
public static class SectionBackdrop
{
    public static IReadOnlyDictionary<string, BackdropStyle> Patterns { get; } =
        new Dictionary<string, BackdropStyle>(StringComparer.OrdinalIgnoreCase)
        {
            // Still
            ["dots"] = new("radial-gradient(currentColor 1px, transparent 1px)", "22px 22px"),
            ["grid"] = new(
                "linear-gradient(currentColor 1px, transparent 1px),"
                + "linear-gradient(90deg, currentColor 1px, transparent 1px)", "28px 28px"),
            ["diagonal"] = new(
                "repeating-linear-gradient(45deg, currentColor 0 1px, transparent 1px 12px)", "auto"),
            ["rings"] = new(
                "repeating-radial-gradient(circle at 50% 50%, currentColor 0 1px, transparent 1px 22px)",
                "auto"),
            ["waves"] = new(
                "repeating-linear-gradient(-45deg, currentColor 0 1px, transparent 1px 16px),"
                + "repeating-linear-gradient(45deg, currentColor 0 1px, transparent 1px 16px)",
                "auto"),

            // Moving
            ["drift"] = new(
                "radial-gradient(currentColor 1.5px, transparent 1.5px)", "26px 26px",
                "backdrop-drift 30s linear infinite"),
            ["bubbles"] = new(
                "radial-gradient(circle at 20% 100%, currentColor 6px, transparent 7px),"
                + "radial-gradient(circle at 60% 100%, currentColor 4px, transparent 5px),"
                + "radial-gradient(circle at 85% 100%, currentColor 8px, transparent 9px)",
                "180px 180px, 140px 140px, 220px 220px",
                "backdrop-rise 22s linear infinite"),
            ["shimmer"] = new(
                "linear-gradient(115deg, transparent 30%, currentColor 45%, transparent 60%)",
                "300% 100%",
                "backdrop-sweep 9s ease-in-out infinite"),
            ["twinkle"] = new(
                "radial-gradient(currentColor 1.5px, transparent 2px),"
                + "radial-gradient(currentColor 1px, transparent 1.5px)",
                "70px 70px, 110px 110px",
                "backdrop-twinkle 6s ease-in-out infinite")
        };

    public static BackdropStyle? Style(string? name) =>
        name is not null && Patterns.TryGetValue(name.Trim(), out var style) ? style : null;

    /// <summary>
    /// An address that cannot break out of <c>url("…")</c>. Anything that could — a quote, a
    /// bracket, a newline, a stylesheet comment — means the value is refused rather than
    /// sanitised into something the school did not intend.
    /// </summary>
    public static string SafeUrl(string? url)
    {
        var trimmed = url?.Trim();
        if (string.IsNullOrEmpty(trimmed) || trimmed.Length > 500)
        {
            return string.Empty;
        }

        foreach (var forbidden in new[] { '"', '\'', '(', ')', '{', '}', ';', '\\', '\n', '\r', '<', '>' })
        {
            if (trimmed.Contains(forbidden))
            {
                return string.Empty;
            }
        }

        return trimmed.Contains("/*", StringComparison.Ordinal) ? string.Empty : trimmed;
    }
}
