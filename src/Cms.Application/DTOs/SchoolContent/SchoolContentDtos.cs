namespace Cms.Application.DTOs.SchoolContent;

/// <summary>
/// Purpose-built shapes for the school content an institution actually maintains.
///
/// These persist through <c>ContentEntry</c>, whose typed columns carry the common fields
/// (title, summary, body, image, date, order, status) while the type-specific fields live in
/// <c>JsonData</c> under the documented keys below. That keeps one storage table and the
/// existing <c>/api/content/{type}</c> contract while giving each kind of content a real
/// model instead of a free-text JSON box.
/// </summary>
public static class SchoolContentTypes
{
    public const string Faculty = "person";
    public const string News = "news";
    public const string Event = "event";
    public const string Department = "department";
    public const string Setting = "setting";

    /// <summary>Single settings record per website.</summary>
    public const string SettingsKey = "site";
}

// ---------------------------------------------------------------------------
// Departments
// ---------------------------------------------------------------------------

public sealed class DepartmentDto
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? HeadOfDepartment { get; set; }
    public string? Summary { get; set; }
    public string? Overview { get; set; }
    public string? ImageUrl { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }

    /// <summary>Courses, subjects or streams offered, one per line in the editor.</summary>
    public IReadOnlyList<string> Programmes { get; set; } = [];

    public int DisplayOrder { get; set; }
    public bool IsPublished { get; set; } = true;
}

public sealed class SaveDepartmentDto
{
    public string? Key { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? HeadOfDepartment { get; set; }
    public string? Summary { get; set; }
    public string? Overview { get; set; }
    public string? ImageUrl { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }

    /// <summary>Newline-separated in the form; split into a list on save.</summary>
    public string? Programmes { get; set; }

    public int DisplayOrder { get; set; }
    public bool IsPublished { get; set; } = true;
}

// ---------------------------------------------------------------------------
// Faculty and staff
// ---------------------------------------------------------------------------

public enum FacultyCategory
{
    Leadership = 1,
    Teaching = 2,
    Administration = 3,
    Support = 4
}

public sealed class FacultyMemberDto
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Designation { get; set; }
    public string? Department { get; set; }
    public FacultyCategory Category { get; set; } = FacultyCategory.Teaching;
    public string? Qualification { get; set; }
    public int? ExperienceYears { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? PhotoUrl { get; set; }
    public string? Headline { get; set; }
    public string? Biography { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsPublished { get; set; } = true;

    /// <summary>
    /// Placeholder shown when no photo is set. Honorifics are skipped so "Dr. Anita Rao"
    /// reads as "AR" rather than "D".
    /// </summary>
    public string Initials
    {
        get
        {
            var honorifics = new[] { "dr", "mr", "mrs", "ms", "miss", "prof", "professor", "sri", "smt" };
            var words = FullName
                .Split([' ', '.', ','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(word => !honorifics.Contains(word.ToLowerInvariant()))
                .ToArray();

            if (words.Length == 0)
            {
                return FullName.Length > 0 ? FullName[..1].ToUpperInvariant() : "?";
            }

            return words.Length == 1
                ? words[0][..1].ToUpperInvariant()
                : $"{words[0][0]}{words[^1][0]}".ToUpperInvariant();
        }
    }
}

public sealed class SaveFacultyMemberDto
{
    public string? Key { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Designation { get; set; }
    public string? Department { get; set; }
    public FacultyCategory Category { get; set; } = FacultyCategory.Teaching;
    public string? Qualification { get; set; }
    public int? ExperienceYears { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? PhotoUrl { get; set; }
    public string? Headline { get; set; }
    public string? Biography { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsPublished { get; set; } = true;
}

// ---------------------------------------------------------------------------
// News, notices and circulars
// ---------------------------------------------------------------------------

public enum NewsCategory
{
    News = 1,
    Notice = 2,
    Circular = 3,
    Achievement = 4
}

public sealed class NewsArticleDto
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Headline { get; set; } = string.Empty;
    public NewsCategory Category { get; set; } = NewsCategory.News;
    public DateTime? PublishDate { get; set; }
    public string? Summary { get; set; }
    public string? Body { get; set; }
    public string? ImageUrl { get; set; }
    public string? AttachmentUrl { get; set; }
    public bool IsFeatured { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsPublished { get; set; } = true;
}

public sealed class SaveNewsArticleDto
{
    public string? Key { get; set; }
    public string Headline { get; set; } = string.Empty;
    public NewsCategory Category { get; set; } = NewsCategory.News;
    public DateTime? PublishDate { get; set; }
    public string? Summary { get; set; }
    public string? Body { get; set; }
    public string? ImageUrl { get; set; }
    public string? AttachmentUrl { get; set; }
    public bool IsFeatured { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsPublished { get; set; } = true;
}

// ---------------------------------------------------------------------------
// Events
// ---------------------------------------------------------------------------

public sealed class SchoolEventDto
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime? StartsOn { get; set; }
    public DateTime? EndsOn { get; set; }
    public string? Venue { get; set; }
    public string? Summary { get; set; }
    public string? Body { get; set; }
    public string? ImageUrl { get; set; }
    public string? RegistrationUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsPublished { get; set; } = true;

    public bool HasFinished(DateTime asOfUtc) => (EndsOn ?? StartsOn) is DateTime end && end < asOfUtc;
}

public sealed class SaveSchoolEventDto
{
    public string? Key { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime? StartsOn { get; set; }
    public DateTime? EndsOn { get; set; }
    public string? Venue { get; set; }
    public string? Summary { get; set; }
    public string? Body { get; set; }
    public string? ImageUrl { get; set; }
    public string? RegistrationUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsPublished { get; set; } = true;
}

// ---------------------------------------------------------------------------
// Site settings
// ---------------------------------------------------------------------------

public enum AdmissionStatus
{
    Closed = 0,
    Open = 1,
    OpeningSoon = 2
}

/// <summary>
/// Operational settings for one website. Visual identity (logo, colours, address, map)
/// belongs to Branding; this covers what the school changes through the year.
/// </summary>
public sealed class SiteSettingsDto
{
    public string? NoticeTicker { get; set; }
    public AdmissionStatus AdmissionStatus { get; set; } = AdmissionStatus.Closed;
    public string? AdmissionsEmail { get; set; }
    public string? AdmissionsPhone { get; set; }
    public string? BrochureUrl { get; set; }
    public string? ApplicationUrl { get; set; }
    public string? OfficeHours { get; set; }
    public string? WhatsAppNumber { get; set; }

    public string? Facebook { get; set; }
    public string? Instagram { get; set; }
    public string? YouTube { get; set; }
    public string? Twitter { get; set; }
    public string? LinkedIn { get; set; }

    public bool HasSocialLinks =>
        !string.IsNullOrWhiteSpace(Facebook) || !string.IsNullOrWhiteSpace(Instagram)
        || !string.IsNullOrWhiteSpace(YouTube) || !string.IsNullOrWhiteSpace(Twitter)
        || !string.IsNullOrWhiteSpace(LinkedIn);

    // ------------------------------------------------------------------ Appearance

    /// <summary>Scrolls the notice strip across the page instead of leaving it still.</summary>
    public bool NoticeTickerScrolls { get; set; }

    /// <summary>
    /// Seconds for the notice to travel across once. A long notice at the speed that suited a
    /// short one is unreadable, so the school sets it against its own text. Zero keeps the
    /// default pace.
    /// </summary>
    public int NoticeTickerSeconds { get; set; }

    /// <summary>
    /// Height of the header logo in pixels. Schools send crests of wildly different proportions
    /// and one fixed size flatters none of them, so the size is theirs to set. Zero keeps the
    /// design's own default.
    /// </summary>
    public int LogoHeight { get; set; }

    /// <summary>Whether sections fade in as they are scrolled to. On unless turned off.</summary>
    public bool ScrollAnimations { get; set; } = true;

    /// <summary>
    /// The notice strip's own colour. Schools want the admissions strip to shout in a colour
    /// that is not their crest's navy, and had no way to say so. Empty keeps the brand colour.
    /// </summary>
    public string? NoticeBarColor { get; set; }

    /// <summary>solid · outline · soft · gradient. Empty is the design's own button.</summary>
    public string? ButtonStyle { get; set; }

    /// <summary>rounded · pill · square. Empty is the design's own corner.</summary>
    public string? ButtonShape { get; set; }

    /// <summary>lift · fill · glow · slide. Empty is the design's own hover.</summary>
    public string? ButtonHover { get; set; }

    /// <summary>lift · zoom · glow · tilt. How cards and tiles answer the pointer.</summary>
    public string? CardHover { get; set; }

    // ------------------------------------------------------------------ Opening popup

    public bool PopupEnabled { get; set; }

    /// <summary>
    /// The posters, separated by a vertical bar. A school runs an admissions poster and a
    /// results poster at the same time and wants both seen, which one image could never do.
    /// </summary>
    public string? PopupImageUrl { get; set; }

    /// <summary>Seconds each poster is shown before the next. Zero leaves the first one up.</summary>
    public int PopupSlideSeconds { get; set; }

    /// <summary>
    /// Seconds before the popup closes itself. Zero waits for the visitor, which is right when
    /// there is a form to fill in and wrong when there is only a poster to glance at.
    /// </summary>
    public int PopupAutoCloseSeconds { get; set; }

    /// <summary>The posters as a list, in the order they were entered.</summary>
    public IReadOnlyList<string> PopupImages =>
        string.IsNullOrWhiteSpace(PopupImageUrl)
            ? []
            : PopupImageUrl.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    public string? PopupHeading { get; set; }

    /// <summary>Where the poster leads when clicked. Left empty, the poster is not a link.</summary>
    public string? PopupLinkUrl { get; set; }

    /// <summary>Adds an enquiry form beside the poster. Submissions reach the contact inbox.</summary>
    public bool PopupShowEnquiryForm { get; set; }

    public string? PopupFormHeading { get; set; }

    /// <summary>Shows the popup once per visit rather than on every page.</summary>
    public bool PopupOncePerVisit { get; set; } = true;

    /// <summary>Nothing to show is not the same as switched on, and an empty box helps nobody.</summary>
    public bool HasPopup => PopupEnabled && (PopupImages.Count > 0 || PopupShowEnquiryForm);
}
