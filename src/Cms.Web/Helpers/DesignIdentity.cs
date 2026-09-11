using Cms.Domain.Enums;

namespace Cms.Web.Helpers;

/// <summary>The typefaces one design asks for, and the Google Fonts query that fetches them.</summary>
public sealed record DesignFonts(string Query, string Display, string Body);

/// <summary>
/// What actually makes one template look unlike another.
///
/// Every design used to share a single typeface pairing, so the templates differed only in
/// which sections each laid out — the same page with the blocks shuffled, which is exactly how
/// it read. Type is the largest part of a design's character, and it costs nothing to vary.
///
/// The families are fetched per design rather than all at once: every pairing in one request
/// would be most of a megabyte of fonts on every page, to use two of them.
/// </summary>
public static class DesignIdentity
{
    private const string Fallback = "Georgia, 'Times New Roman', serif";
    private const string Sans = "'Segoe UI', system-ui, -apple-system, sans-serif";

    /// <summary>
    /// The design to actually draw for a stored value.
    ///
    /// Withdrawn designs leave their numbers in <c>Sites.HomeVariant</c> until the migration
    /// moves them, and a website is served in the meantime. Resolving the stray value once,
    /// here, means the layout, the typefaces and the body class all agree on Prestige rather
    /// than the page taking the Prestige layout under a <c>variant-1</c> class no stylesheet
    /// answers to.
    /// </summary>
    public static HomeVariant Resolve(HomeVariant variant) =>
        Enum.IsDefined(variant) ? variant : HomeVariant.Prestige;

    public static DesignFonts Fonts(HomeVariant variant) => variant switch
    {
        // Soft and warm, for a campus that photographs well.
        HomeVariant.Campus => new(
            "family=Fraunces:opsz,wght@9..144,500;9..144,600&family=Nunito+Sans:wght@400;600;700",
            $"Fraunces, {Fallback}", $"'Nunito Sans', {Sans}"),

        // Condensed and utilitarian — a noticeboard, not a prospectus.
        HomeVariant.Bulletin => new(
            "family=Montserrat:wght@500;600;700&family=Roboto+Condensed:wght@400;500;700",
            $"Montserrat, {Sans}", $"'Roboto Condensed', {Sans}"),

        // Large, quiet, editorial.
        HomeVariant.Atrium => new(
            "family=Libre+Baskerville:wght@400;700&family=Open+Sans:wght@300;400;600;700",
            $"'Libre Baskerville', {Fallback}", $"'Open Sans', {Sans}"),

        // High contrast and formal, for an institution that trades on its name. Also the
        // fallback, so a website still holding a withdrawn design's number is typeset rather
        // than left with the browser's defaults.
        _ => new(
            "family=Playfair+Display:wght@500;600;700&family=Lato:wght@400;700",
            $"'Playfair Display', {Fallback}", $"Lato, {Sans}")
    };
}
