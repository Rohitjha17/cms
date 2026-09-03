using Cms.Domain.Enums;

namespace Cms.Web.Helpers;

/// <summary>The typefaces one design asks for, and the Google Fonts query that fetches them.</summary>
public sealed record DesignFonts(string Query, string Display, string Body);

/// <summary>
/// What actually makes one template look unlike another.
///
/// Every design used to share a single typeface pairing, so five templates differed only in
/// which sections each laid out — the same page with the blocks shuffled, which is exactly how
/// it read. Type is the largest part of a design's character, and it costs nothing to vary.
///
/// The families are fetched per design rather than all at once: seven pairings in one request
/// would be most of a megabyte of fonts on every page, to use two of them.
/// </summary>
public static class DesignIdentity
{
    private const string Fallback = "Georgia, 'Times New Roman', serif";
    private const string Sans = "'Segoe UI', system-ui, -apple-system, sans-serif";

    public static DesignFonts Fonts(HomeVariant variant) => variant switch
    {
        // Sharp, tight, high contrast — a school that wants to look current.
        HomeVariant.Modern => new(
            "family=Archivo:wght@600;700;800&family=Public+Sans:wght@400;500;600",
            $"Archivo, {Sans}", $"'Public Sans', {Sans}"),

        // Soft and warm, for a campus that photographs well.
        HomeVariant.Campus => new(
            "family=Fraunces:opsz,wght@9..144,500;9..144,600&family=Nunito+Sans:wght@400;600;700",
            $"Fraunces, {Fallback}", $"'Nunito Sans', {Sans}"),

        // Bookish and restrained, for a college.
        HomeVariant.Academic => new(
            "family=Libre+Baskerville:wght@400;700&family=IBM+Plex+Sans:wght@400;500;600",
            $"'Libre Baskerville', {Fallback}", $"'IBM Plex Sans', {Sans}"),

        // High contrast and formal, for an institution that trades on its name.
        HomeVariant.Prestige => new(
            "family=Playfair+Display:wght@500;600;700&family=Lato:wght@400;700",
            $"'Playfair Display', {Fallback}", $"Lato, {Sans}"),

        // Condensed and utilitarian — a noticeboard, not a prospectus.
        HomeVariant.Bulletin => new(
            "family=Oswald:wght@500;600;700&family=IBM+Plex+Sans:wght@400;500;600",
            $"Oswald, {Sans}", $"'IBM Plex Sans', {Sans}"),

        // Large, quiet, editorial.
        HomeVariant.Atrium => new(
            "family=Cormorant+Garamond:wght@300;500;600&family=Jost:wght@300;400;500",
            $"'Cormorant Garamond', {Fallback}", $"Jost, {Sans}"),

        // Classic keeps the pairing every design used to share.
        _ => new(
            "family=Cormorant+Garamond:wght@500;600;700&family=Source+Sans+3:wght@400;500;600;700",
            $"'Cormorant Garamond', {Fallback}", $"'Source Sans 3', {Sans}")
    };
}
