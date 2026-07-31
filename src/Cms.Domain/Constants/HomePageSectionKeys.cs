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

    public static readonly IReadOnlyList<(string Key, string DisplayName, int Order)> All =
    [
        (Hero, "Hero Banner", 1),
        (Welcome, "Welcome Section", 2),
        (About, "About School", 3),
        (Principal, "Principal Message", 4),
        (Chairman, "Chairman Message", 5),
        (Statistics, "Statistics", 6),
        (Courses, "Courses", 7),
        (Departments, "Departments", 8),
        (WhyChooseUs, "Why Choose Us", 9),
        (Announcements, "Announcements", 10),
        (LatestNews, "Latest News", 11),
        (UpcomingEvents, "Upcoming Events", 12),
        (Gallery, "Gallery", 13),
        (Video, "Video Section", 14),
        (Testimonials, "Testimonials", 15),
        (Achievements, "Achievements", 16),
        (AdmissionCta, "Admission CTA", 17),
        (DownloadBrochure, "Download Brochure", 18),
        (Contact, "Contact Section", 19),
        (Partners, "Partners", 20),
        (FooterCta, "Footer CTA", 21)
    ];

    public static bool IsKnown(string sectionKey) =>
        All.Any(x => string.Equals(x.Key, sectionKey, StringComparison.OrdinalIgnoreCase));

    public static string Normalize(string sectionKey) => sectionKey.Trim().ToLowerInvariant();
}
