namespace Cms.Web.Helpers;

/// <summary>
/// The text colour to put on a school's own colour.
///
/// Buttons were drawn with a dark ink hard-coded against the accent, on the assumption that an
/// accent is a bright one. A school whose second colour is navy, maroon or black therefore got
/// dark text on a dark button — the "Send message" button that reads as a solid black bar, with
/// the words only visible if you know they are there.
///
/// Which ink to use is decided from the colour itself: the relative luminance of the background
/// decides whether black or white sits legibly on it. The same arithmetic every contrast tool
/// uses, so the answer matches what a checker would say rather than what looked right to me.
/// </summary>
public static class ReadableInk
{
    /// <summary>Near-black rather than black, so a light button does not read as a hole.</summary>
    private const string Dark = "#131b2b";
    private const string Light = "#ffffff";

    /// <param name="background">A colour as #rgb or #rrggbb. Anything else gets the dark ink.</param>
    public static string On(string? background)
    {
        var rgb = Parse(background);
        if (rgb is not var (r, g, b))
        {
            return Dark;
        }

        // WCAG relative luminance: the channels are linearised first, because sRGB is not.
        static double Channel(int value)
        {
            var v = value / 255.0;
            return v <= 0.03928 ? v / 12.92 : Math.Pow((v + 0.055) / 1.055, 2.4);
        }

        var luminance = (0.2126 * Channel(r)) + (0.7152 * Channel(g)) + (0.0722 * Channel(b));

        // Contrast against white is (1.05 / (L + 0.05)); against black it is ((L + 0.05) / 0.05).
        // They cross at L ≈ 0.1791, which is the point where white starts being the better ink.
        return luminance > 0.1791 ? Dark : Light;
    }

    private static (int R, int G, int B)? Parse(string? value)
    {
        var hex = value?.Trim().TrimStart('#');
        if (string.IsNullOrEmpty(hex))
        {
            return null;
        }

        if (hex.Length == 3)
        {
            hex = string.Concat(hex[0], hex[0], hex[1], hex[1], hex[2], hex[2]);
        }

        if (hex.Length != 6 || !hex.All(Uri.IsHexDigit))
        {
            return null;
        }

        return (
            Convert.ToInt32(hex[..2], 16),
            Convert.ToInt32(hex.Substring(2, 2), 16),
            Convert.ToInt32(hex.Substring(4, 2), 16));
    }
}
