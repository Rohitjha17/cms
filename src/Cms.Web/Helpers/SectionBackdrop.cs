namespace Cms.Web.Helpers;

/// <summary>
/// Turns a section's chosen backdrop into CSS, safely.
///
/// The values come from the console, so they reach a stylesheet the school did not write and
/// cannot be trusted into one unedited: a quote or a brace would close the rule early and let
/// whatever follows through. Patterns are chosen from a fixed list; a photograph's address is
/// stripped of anything that could end the url().
/// </summary>
public static class SectionBackdrop
{
    /// <summary>The patterns a section may choose, drawn rather than downloaded.</summary>
    public static IReadOnlyDictionary<string, string> Patterns { get; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["dots"] = "radial-gradient(currentColor 1px, transparent 1px)",
            ["grid"] = "linear-gradient(currentColor 1px, transparent 1px),"
                + "linear-gradient(90deg, currentColor 1px, transparent 1px)",
            ["diagonal"] = "repeating-linear-gradient(45deg, currentColor 0 1px, transparent 1px 12px)",
            ["rings"] = "repeating-radial-gradient(circle at 50% 50%, currentColor 0 1px, transparent 1px 22px)",
            ["waves"] = "repeating-linear-gradient(-45deg, currentColor 0 1px, transparent 1px 16px),"
                + "repeating-linear-gradient(45deg, currentColor 0 1px, transparent 1px 16px)"
        };

    public static string Pattern(string? name) =>
        name is not null && Patterns.TryGetValue(name.Trim(), out var css) ? css : "none";

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
