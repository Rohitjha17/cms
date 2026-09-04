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
    /// How many copies of the notice are on the strip at once.
    ///
    /// A continuous strip has to tile, or the loop shows a gap and a jump at the seam — but a
    /// school that entered its notice once reasonably expects to see it once. One means one
    /// copy crossing the strip with a gap behind it; two or three keep the strip full. Zero
    /// fills the width, whatever that takes.
    /// </summary>
    public int NoticeTickerRepeat { get; set; }

    /// <summary>
    /// A short label at the head of the notice strip — "NOTICE", "LATEST". Schools mark the
    /// strip out this way so it reads as an announcement rather than as decoration.
    /// </summary>
    public string? NoticeLabel { get; set; }

    /// <summary>Puts the phone and email in the header itself, with icons and labels.</summary>
    public bool HeaderContact { get; set; }

    /// <summary>A single call to action in the header. Empty shows none.</summary>
    public string? HeaderCtaText { get; set; }

    public string? HeaderCtaLink { get; set; }

    /// <summary>
    /// The banner pictures are finished artwork — a school's admissions poster with its own
    /// headline, dates and button drawn into it. With this on, the hero shows each one whole
    /// and prints nothing over it: a heading laid across artwork that already has a heading is
    /// two headings on top of one another, and the scrim that makes ours legible dims theirs.
    /// </summary>
    public bool HeroPlainImages { get; set; }

    /// <summary>
    /// original · rounded · square. How the crest in the header is cut. Empty is "rounded",
    /// which is what every site had before this was a choice.
    /// </summary>
    public string? LogoShape { get; set; }

    /// <summary>
    /// small · medium · large · xlarge. The size of the heading at the top of an inner page.
    /// Empty is "large", which is what every page had before this was a choice.
    /// </summary>
    public string? PageTitleSize { get; set; }

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

    /// <summary>
    /// The buttons' own colour. Until now they took the brand colour, so a school that wanted a
    /// louder button had to repaint the whole site to get one. Empty keeps the brand colour.
    /// </summary>
    public string? ButtonColor { get; set; }

    /// <summary>solid · gradient · dark · outline. How the notice strip itself is painted.</summary>
    public string? NoticeBarStyle { get; set; }

    /// <summary>solid · outline · soft · gradient. Empty is the design's own button.</summary>
    public string? ButtonStyle { get; set; }

    /// <summary>rounded · pill · square. Empty is the design's own corner.</summary>
    public string? ButtonShape { get; set; }

    /// <summary>lift · fill · glow · slide. Empty is the design's own hover.</summary>
    public string? ButtonHover { get; set; }

    /// <summary>lift · zoom · glow · tilt. How cards and tiles answer the pointer.</summary>
    public string? CardHover { get; set; }

    /// <summary>none · zoom · lift · tint. How pictures answer the pointer.</summary>
    public string? ImageHover { get; set; }

    /// <summary>none · underline · color. How links in body copy answer the pointer.</summary>
    public string? LinkHover { get; set; }

    /// <summary>
    /// The colour a glow, a fill or a tint uses. Every hover took the accent, so a school could
    /// choose the effect but not what colour it happened in. Empty keeps the accent.
    /// </summary>
    public string? HoverColor { get; set; }

    // ------------------------------------------------------- Defaults for every section

    /// <summary>
    /// The entrance every section uses unless it chose its own. Setting twenty-three sections
    /// one at a time to get a consistent site is not a choice anyone would make.
    /// </summary>
    public string? SectionAnimation { get; set; }

    /// <summary>The backdrop every section uses unless it chose its own.</summary>
    public string? SectionPattern { get; set; }

    // ------------------------------------------------------------------- Hero slideshow

    /// <summary>
    /// Seconds between hero images. The hero's own setting wins where one was entered; this is
    /// for a school that would rather set it once for whichever template it is using.
    /// </summary>
    public int HeroSlideSeconds { get; set; }

    /// <summary>Arrows and dots on the hero slideshow. On unless turned off.</summary>
    public bool HeroShowControls { get; set; } = true;

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

    /// <summary>
    /// The posters as a list, in the order they were entered. One per line is what anyone
    /// pasting a handful of addresses will do, and the bar still works for anything already
    /// saved that way.
    /// </summary>
    public IReadOnlyList<string> PopupImages =>
        string.IsNullOrWhiteSpace(PopupImageUrl)
            ? []
            : PopupImageUrl.Split(
                ['|', '\n', '\r'],
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    public string? PopupHeading { get; set; }

    /// <summary>Where the poster leads when clicked. Left empty, the poster is not a link.</summary>
    public string? PopupLinkUrl { get; set; }

    /// <summary>Adds an enquiry form beside the poster. Submissions reach the contact inbox.</summary>
    public bool PopupShowEnquiryForm { get; set; }

    public string? PopupFormHeading { get; set; }

    /// <summary>
    /// The kinds of enquiry a school takes, one per line. A contact form that cannot tell an
    /// admissions question from a job application sends both to the same person, who then has
    /// to sort the inbox by reading it. Empty offers no choice, as before.
    /// </summary>
    public string? EnquiryTypes { get; set; }

    /// <summary>The enquiry types as a list, in the order the school entered them.</summary>
    public IReadOnlyList<string> EnquiryTypeList =>
        string.IsNullOrWhiteSpace(EnquiryTypes)
            ? []
            : EnquiryTypes.Split(
                ['|', '\n', '\r'],
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    /// <summary>
    /// Shows the popup once per visit rather than on every page. Off by default: a school runs
    /// a popup to be seen, and a visitor who reloads to look again should find it there.
    /// </summary>
    public bool PopupOncePerVisit { get; set; }

    /// <summary>Nothing to show is not the same as switched on, and an empty box helps nobody.</summary>
    public bool HasPopup => PopupEnabled && (PopupImages.Count > 0 || PopupShowEnquiryForm);
}
