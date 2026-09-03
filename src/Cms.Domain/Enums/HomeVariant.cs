namespace Cms.Domain.Enums;

/// <summary>Public homepage design variants for school/college websites.</summary>
public enum HomeVariant
{
    Classic = 1,
    Modern = 2,
    Campus = 3,
    Academic = 4,
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
