namespace Cms.Domain.Enums;

/// <summary>
/// Public homepage design variants for school/college websites.
///
/// The numbers are stored in <c>Sites.HomeVariant</c>, so they are fixed: a design that is
/// withdrawn leaves its number behind rather than letting a later design inherit it and
/// silently restyle every website that had chosen the old one. 1 (Classic), 2 (Modern) and
/// 4 (Academic) are retired and must not be reused.
/// </summary>
public enum HomeVariant
{
    Campus = 3,

    /// <summary>
    /// Formal and centred. A framed hero and a leadership quote, for an institution that
    /// trades on its name. The default for a new website.
    /// </summary>
    Prestige = 5,

    /// <summary>
    /// Dense and utilitarian. Notices first, tables over cards, small type, a great deal on
    /// screen at once — the way a school with timetables, circulars and downloads to publish
    /// actually uses its website.
    /// </summary>
    Bulletin = 6,

    /// <summary>
    /// Spacious and editorial. Large display type, generous whitespace, photography given room.
    /// For an institution whose website is a prospectus rather than a noticeboard.
    /// </summary>
    Atrium = 7
}
