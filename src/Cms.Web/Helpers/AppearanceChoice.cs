namespace Cms.Web.Helpers;

/// <summary>
/// The appearance choices a school can make, and the guarantee that only those reach the page.
///
/// Each one becomes a class name or a colour in the layout's own markup. A value typed in the
/// console must never be able to become anything else, so an unknown name is nothing at all
/// rather than something passed through and hoped for.
/// </summary>
public static class AppearanceChoice
{
    private static readonly string[] ButtonStyles = ["solid", "outline", "soft", "gradient"];
    private static readonly string[] ButtonShapes = ["rounded", "pill", "square"];
    private static readonly string[] ButtonHovers = ["lift", "fill", "glow", "slide"];
    private static readonly string[] CardHovers = ["lift", "zoom", "glow", "tilt"];
    private static readonly string[] NoticeStyles = ["solid", "gradient", "dark", "outline"];

    public static string? ButtonStyle(string? value) => Pick(value, ButtonStyles, "btn-style-");
    public static string? ButtonShape(string? value) => Pick(value, ButtonShapes, "btn-shape-");
    public static string? ButtonHover(string? value) => Pick(value, ButtonHovers, "btn-hover-");
    public static string? CardHover(string? value) => Pick(value, CardHovers, "card-hover-");
    public static string? NoticeStyle(string? value) => Pick(value, NoticeStyles, "notice-bar--");

    /// <summary>A section's own hover, applied to the cards inside it.</summary>
    public static string? SectionHover(string? value) => Pick(value, CardHovers, "card-hover-");

    private static string? Pick(string? value, string[] allowed, string prefix)
    {
        var name = value?.Trim().ToLowerInvariant();
        return string.IsNullOrEmpty(name) || !allowed.Contains(name) ? null : prefix + name;
    }

    /// <summary>
    /// A colour, or nothing. Only the hex forms a school actually types are accepted — anything
    /// else would be going into a style attribute on trust.
    /// </summary>
    public static string? Color(string? value)
    {
        var text = value?.Trim();
        if (string.IsNullOrEmpty(text) || text.Length is not (4 or 7) || text[0] != '#')
        {
            return null;
        }

        for (var i = 1; i < text.Length; i++)
        {
            if (!Uri.IsHexDigit(text[i]))
            {
                return null;
            }
        }

        return text;
    }
}
