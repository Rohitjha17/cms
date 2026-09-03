namespace Cms.Domain.Constants;

/// <summary>
/// Canonical homepage section keys. Avoid magic strings across the solution.
/// </summary>
public static class HomePageSectionKeys
{
    public const string Hero = "hero";
    public const string Welcome = "welcome";
    public const string About = "about";
    public const string Principal = "principal";
    public const string Chairman = "chairman";
    public const string Director = "director";
    public const string Manager = "manager";
    public const string Statistics = "statistics";
    public const string Courses = "courses";
    public const string Departments = "departments";
    public const string WhyChooseUs = "why_choose_us";
    public const string Announcements = "announcements";
    public const string LatestNews = "latest_news";
    public const string UpcomingEvents = "upcoming_events";
    public const string Gallery = "gallery";
    public const string Video = "video";
    public const string Testimonials = "testimonials";
    public const string Achievements = "achievements";
    public const string AdmissionCta = "admission_cta";
    public const string DownloadBrochure = "download_brochure";
    public const string Contact = "contact";
    public const string Partners = "partners";
    public const string FooterCta = "footer_cta";

    /// <summary>School hours, by term and by wing — the single most looked-up fact on a school site.</summary>
    public const string Timings = "timings";

    /// <summary>The crest read symbol by symbol, which is how a school explains what it stands for.</summary>
    public const string Crest = "crest";

    /// <summary>Former students by name, with what they went on to do.</summary>
    public const string Alumni = "alumni";

    /// <summary>The full staff list as a table: forty rows nobody wants as forty cards.</summary>
    public const string StaffList = "staff_list";

    /// <summary>Laboratories, library and grounds, each named and described.</summary>
    public const string Facilities = "facilities";

    /// <summary>The founder, and the school's own history as a dated sequence.</summary>
    public const string Founder = "founder";

    /// <summary>Circulars, timetables and forms as a table with format and size.</summary>
    public const string Downloads = "downloads";

    public static readonly IReadOnlyList<(string Key, string DisplayName, int Order)> All =
    [
        (Hero, "Hero Banner", 1),
        (Welcome, "Welcome Section", 2),
        (About, "About School", 3),
        (Principal, "Principal Message", 4),
        (Chairman, "Chairman Message", 5),
        (Director, "Director Message", 6),
        (Manager, "Manager Message", 7),
        (Statistics, "Statistics", 8),
        (Courses, "Courses", 9),
        (Departments, "Departments", 10),
        (WhyChooseUs, "Why Choose Us", 11),
        (Announcements, "Announcements", 12),
        (LatestNews, "Latest News", 13),
        (UpcomingEvents, "Upcoming Events", 14),
        (Gallery, "Gallery", 15),
        (Video, "Video Section", 16),
        (Testimonials, "Testimonials", 17),
        (Achievements, "Achievements", 18),
        (AdmissionCta, "Admission CTA", 19),
        (DownloadBrochure, "Download Brochure", 20),
        (Contact, "Contact Section", 21),
        (Partners, "Partners", 22),
        (FooterCta, "Footer CTA", 23),
        (Timings, "School Timings", 24),
        (Crest, "Crest & Motto", 25),
        (Alumni, "Notable Alumni", 26),
        (StaffList, "Staff List", 27),
        (Facilities, "Facilities", 28),
        (Founder, "Founder & History", 29),
        (Downloads, "Downloads", 30)
    ];

    public static bool IsKnown(string sectionKey) =>
        All.Any(x => string.Equals(x.Key, sectionKey, StringComparison.OrdinalIgnoreCase));

    public static string Normalize(string sectionKey) => sectionKey.Trim().ToLowerInvariant();
}
