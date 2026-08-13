using Cms.Domain.Constants;
using Cms.Domain.Enums;

namespace Cms.Application.Templates;

/// <summary>
/// Complete, ready-to-show website templates.
///
/// Page templates cover one page; these cover a whole institution — home design, palette,
/// hero copy, statistics, the page set, and sample staff, notices, events and departments.
/// Provisioning from one produces a website that already looks finished, so it can be shown
/// to a school as "this could be yours" and edited from there.
///
/// Defined in code rather than the database so a template is versioned with the release and
/// cannot be half-deleted by an operator. Every value is sample copy meant to be replaced.
/// </summary>
public static class SiteTemplateCatalog
{
    public static IReadOnlyList<SiteTemplate> All { get; } =
    [
        new SiteTemplate
        {
            Key = "heritage-day-school",
            Name = "Heritage Day School",
            Summary = "A traditional CBSE day school with a photo-led hero, results band and gallery.",
            BestFor = "Established K–12 schools with a long history",
            WebsiteType = WebsiteType.School,
            HomeVariant = HomeVariant.Classic,
            PrimaryColor = "#0f2d5c",
            SecondaryColor = "#c9a227",
            SampleTagline = "Nurturing character and curiosity since 1984",
            Highlights =
            [
                "Photo hero with admissions call to action",
                "Statistics band: students, teachers, results, years",
                "Leadership messages and campus gallery",
                "Mandatory disclosure table ready for CBSE documents"
            ],
            HeroHeading = "An education that lasts a lifetime",
            HeroDescription = "Four decades of academic excellence, values and community in the heart of the city.",
            Statistics = new StatisticsSample(2400, 130, 100, 41),
            WhyIntro = "Families choose us for consistency: the same standards, care and outcomes, year after year.",
            Faculty =
            [
                new ContentSample("Principal", "Dr. Meera Krishnan", "Leading the school since 2011.", "Leadership", "Ph.D (Education), M.A"),
                new ContentSample("Vice Principal", "Anil Bhatt", "Academics and examinations.", "Leadership", "M.Sc, B.Ed"),
                new ContentSample("Head of Science", "Ritu Sharma", "Physics and the robotics programme.", "Teaching", "M.Sc, B.Ed"),
                new ContentSample("Head of Languages", "Farhan Qureshi", "English literature and debate.", "Teaching", "M.A, B.Ed")
            ],
            Departments =
            [
                new DepartmentSample("Science", "Physics, Chemistry and Biology with three dedicated laboratories.",
                    ["Physics", "Chemistry", "Biology", "Computer Science"]),
                new DepartmentSample("Commerce", "Accountancy, Business Studies and Economics for senior grades.",
                    ["Accountancy", "Business Studies", "Economics"]),
                new DepartmentSample("Humanities", "History, Geography and Political Science.",
                    ["History", "Geography", "Political Science", "Psychology"])
            ],
            News =
            [
                new NewsSample("Admissions open for 2026–27", "Notice", "Applications invited for Nursery to Class IX. Forms close 28 February.", true),
                new NewsSample("Class XII results: 100% pass, 41 distinctions", "Achievement", "Our strongest board results to date.", false),
                new NewsSample("Revised school timings from July", "Circular", "School will begin at 7:45 a.m. from the new session.", false)
            ],
            Events =
            [
                new EventSample("Admissions Open House", 14, "Main campus, Gate 2", "Tour the campus and meet the faculty."),
                new EventSample("Annual Day", 45, "School auditorium", "An evening of music, drama and dance."),
                new EventSample("Inter-house Sports Meet", -20, "School ground", "Track and field finals across all four houses.")
            ]
        },

        new SiteTemplate
        {
            Key = "metro-modern-school",
            Name = "Metro Modern School",
            Summary = "A contemporary split-hero layout for urban co-ed schools with strong photography.",
            BestFor = "New-age city schools and international curricula",
            WebsiteType = WebsiteType.School,
            HomeVariant = HomeVariant.Modern,
            PrimaryColor = "#123b63",
            SecondaryColor = "#e0673f",
            SampleTagline = "Learning designed for the world they will inherit",
            Highlights =
            [
                "Split hero: headline beside a full-bleed campus image",
                "Programme cards for streams and electives",
                "Latest news and testimonials on the home page",
                "Clean layout that suits heavy photography"
            ],
            HeroHeading = "Where curiosity becomes capability",
            HeroDescription = "An inquiry-led curriculum, small classes and mentors who know every learner by name.",
            Statistics = new StatisticsSample(1100, 96, 100, 12),
            WhyIntro = "A modern curriculum, taught in small groups, connected to the world beyond the classroom.",
            Faculty =
            [
                new ContentSample("Head of School", "Kavita Rao", "Curriculum design and pastoral care.", "Leadership", "M.Ed"),
                new ContentSample("IB Coordinator", "Daniel Fernandes", "Diploma programme and university guidance.", "Leadership", "M.A"),
                new ContentSample("Design & Technology", "Neha Kulkarni", "Makerspace and robotics.", "Teaching", "B.Tech, B.Ed")
            ],
            Departments =
            [
                new DepartmentSample("STEM", "Integrated science, mathematics and design technology.",
                    ["Mathematics", "Physics", "Design Technology", "Computer Science"]),
                new DepartmentSample("Global Perspectives", "Languages, humanities and world studies.",
                    ["English", "French", "Global Politics", "Economics"]),
                new DepartmentSample("Creative Arts", "Visual art, music, theatre and film.",
                    ["Visual Art", "Music", "Theatre"])
            ],
            News =
            [
                new NewsSample("Applications open for the 2026 cohort", "Notice", "Limited seats across Grades 1 to 9.", true),
                new NewsSample("Our students at the National Innovation Challenge", "Achievement", "Two teams reached the national final.", false)
            ],
            Events =
            [
                new EventSample("Campus Tour Morning", 9, "Reception", "A guided walk-through for prospective families."),
                new EventSample("Student Showcase", 33, "Atrium", "Exhibitions from every year group.")
            ]
        },

        new SiteTemplate
        {
            Key = "residential-campus",
            Name = "Residential Campus",
            Summary = "A tall hero with large facility panels, built for boarding schools with grounds to show.",
            BestFor = "Boarding and residential schools",
            WebsiteType = WebsiteType.School,
            HomeVariant = HomeVariant.Campus,
            PrimaryColor = "#14432f",
            SecondaryColor = "#c9a227",
            SampleTagline = "A campus where learning never stops at the classroom door",
            Highlights =
            [
                "Full-height hero for landscape photography",
                "Large facility panels: houses, sports, dining, infirmary",
                "Gallery-forward layout",
                "Facilities page prepared for boarding details"
            ],
            HeroHeading = "Room to grow, in every sense",
            HeroDescription = "Sixty acres of classrooms, fields, studios and residences — a community that lives and learns together.",
            Statistics = new StatisticsSample(820, 74, 100, 38),
            WhyIntro = "Boarding life builds independence, friendship and routine that day schooling cannot replicate.",
            Faculty =
            [
                new ContentSample("Headmaster", "Col. Vikram Singh (Retd.)", "Leading the school since 2015.", "Leadership", "M.A"),
                new ContentSample("Dean of Residence", "Sunita Menon", "Houses, pastoral care and wellbeing.", "Leadership", "M.A, Dip. Counselling"),
                new ContentSample("Director of Sport", "Arjun Pillai", "Athletics, swimming and team games.", "Teaching", "M.P.Ed")
            ],
            Departments =
            [
                new DepartmentSample("Sciences", "Laboratories open beyond school hours for boarders.",
                    ["Physics", "Chemistry", "Biology"]),
                new DepartmentSample("Sport & Wellbeing", "Coaching across athletics, swimming and team games.",
                    ["Athletics", "Swimming", "Cricket", "Basketball"]),
                new DepartmentSample("Performing Arts", "Music, dance and theatre studios in the arts block.",
                    ["Music", "Dance", "Theatre"])
            ],
            News =
            [
                new NewsSample("Boarding admissions for 2026–27", "Notice", "Applications open for Classes VI to XI.", true),
                new NewsSample("New astro-turf opens", "Achievement", "A full-size hockey and football surface.", false)
            ],
            Events =
            [
                new EventSample("Parents' Weekend", 21, "Main campus", "Two days with the houses and staff."),
                new EventSample("Founder's Day", 60, "Quadrangle", "Our annual whole-school celebration.")
            ]
        },

        new SiteTemplate
        {
            Key = "degree-college",
            Name = "Degree College",
            Summary = "Results-led hero with statistics and department columns, for higher education.",
            BestFor = "Degree colleges and senior secondary institutions",
            WebsiteType = WebsiteType.College,
            HomeVariant = HomeVariant.Academic,
            PrimaryColor = "#12263f",
            SecondaryColor = "#8b6b2e",
            SampleTagline = "Where ambition meets opportunity",
            Highlights =
            [
                "Statistics band leading with placements and outcomes",
                "Department columns for streams and faculties",
                "Mandatory disclosure prepared for UGC/AICTE documents",
                "Admission page structured around eligibility and dates"
            ],
            HeroHeading = "Graduate ready for what comes next",
            HeroDescription = "Undergraduate and postgraduate programmes across arts, science and commerce, with placement support throughout.",
            Statistics = new StatisticsSample(4200, 210, 640, 32),
            WhyIntro = "Teaching, research and placement support that stays with students well past graduation.",
            Faculty =
            [
                new ContentSample("Principal", "Prof. S. Raghavan", "Leading the college since 2018.", "Leadership", "Ph.D"),
                new ContentSample("Dean of Academics", "Dr. Leela Nair", "Curriculum, examinations and accreditation.", "Leadership", "Ph.D"),
                new ContentSample("Head, Placements", "Rohit Malhotra", "Industry partnerships and recruitment.", "Administration", "MBA")
            ],
            Departments =
            [
                new DepartmentSample("Commerce & Management", "B.Com, BBA and M.Com with placement support.",
                    ["B.Com", "B.Com (Honours)", "BBA", "M.Com"]),
                new DepartmentSample("Science", "Undergraduate and postgraduate science programmes.",
                    ["B.Sc Physics", "B.Sc Chemistry", "B.Sc Computer Science", "M.Sc Physics"]),
                new DepartmentSample("Arts & Humanities", "Languages, economics and social sciences.",
                    ["B.A English", "B.A Economics", "B.A Political Science"])
            ],
            News =
            [
                new NewsSample("Admissions 2026: merit list published", "Notice", "First merit list for all undergraduate programmes.", true),
                new NewsSample("Campus placements cross 640 offers", "Achievement", "Highest placement season on record.", false),
                new NewsSample("Examination schedule, even semester", "Circular", "Datesheet for all departments.", false)
            ],
            Events =
            [
                new EventSample("Admission Counselling", 7, "Administrative block", "Walk-in counselling for all programmes."),
                new EventSample("Annual Placement Drive", 28, "Seminar hall", "Recruiters across technology, finance and analytics.")
            ]
        },

        new SiteTemplate
        {
            Key = "prestige-institution",
            Name = "Prestige Institution",
            Summary = "A centred, framed hero with a leadership quote — understated and formal.",
            BestFor = "Heritage institutions and trusts",
            WebsiteType = WebsiteType.Other,
            HomeVariant = HomeVariant.Prestige,
            PrimaryColor = "#1b2a4a",
            SecondaryColor = "#b08d3f",
            SampleTagline = "Founded on service, sustained by excellence",
            Highlights =
            [
                "Centred framed hero over a single strong image",
                "Leadership quote as the opening statement",
                "Committee page prepared for trustees and governance",
                "Restrained palette suited to formal institutions"
            ],
            HeroHeading = "A tradition of purposeful education",
            HeroDescription = "Serving generations of families with an education grounded in character, scholarship and service.",
            Statistics = new StatisticsSample(1600, 118, 100, 76),
            WhyIntro = "Seventy-six years of continuity: the same values, taught with every generation's tools.",
            Faculty =
            [
                new ContentSample("Chairman", "Justice (Retd.) P. N. Bhagat", "Chair of the governing trust.", "Leadership", "LL.M"),
                new ContentSample("Principal", "Dr. Ayesha Siddiqui", "Academic leadership and staff development.", "Leadership", "Ph.D"),
                new ContentSample("Registrar", "Mahesh Iyer", "Admissions, records and compliance.", "Administration", "M.Com")
            ],
            Departments =
            [
                new DepartmentSample("Academics", "Curriculum, assessment and scholarship.",
                    ["Sciences", "Humanities", "Commerce"]),
                new DepartmentSample("Governance", "Trust administration and statutory compliance.",
                    ["Trust Office", "Compliance"])
            ],
            News =
            [
                new NewsSample("Platinum jubilee celebrations announced", "Notice", "A year of events marking seventy-five years.", true),
                new NewsSample("Annual report published", "Circular", "The trust's report for the year is available to download.", false)
            ],
            Events =
            [
                new EventSample("Founders' Commemoration", 30, "Main hall", "Our annual remembrance and prize giving."),
                new EventSample("Governing Council Meeting", 12, "Trust office", "Quarterly meeting of the governing council.")
            ]
        }
    ];

    public static SiteTemplate? Find(string key) =>
        All.FirstOrDefault(x => string.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase));
}

public sealed class SiteTemplate
{
    public required string Key { get; init; }
    public required string Name { get; init; }
    public required string Summary { get; init; }
    public required string BestFor { get; init; }
    public required WebsiteType WebsiteType { get; init; }
    public required HomeVariant HomeVariant { get; init; }
    public required string PrimaryColor { get; init; }
    public required string SecondaryColor { get; init; }
    public required string SampleTagline { get; init; }
    public required IReadOnlyList<string> Highlights { get; init; }

    public required string HeroHeading { get; init; }
    public required string HeroDescription { get; init; }
    public required StatisticsSample Statistics { get; init; }
    public required string WhyIntro { get; init; }

    public required IReadOnlyList<ContentSample> Faculty { get; init; }
    public required IReadOnlyList<DepartmentSample> Departments { get; init; }
    public required IReadOnlyList<NewsSample> News { get; init; }
    public required IReadOnlyList<EventSample> Events { get; init; }

    /// <summary>Every template ships the full starter page set from the page gallery.</summary>
    public IReadOnlyList<string> PageTemplateKeys => PageTemplateKeys_;
    private static readonly string[] PageTemplateKeys_ = Domain.Constants.PageTemplateKeys.StarterPages;
}

public sealed record StatisticsSample(int Students, int Teachers, int Placements, int Years);

public sealed record ContentSample(
    string Designation, string FullName, string Headline, string Category, string Qualification);

public sealed record DepartmentSample(string Name, string Summary, IReadOnlyList<string> Programmes);

public sealed record NewsSample(string Headline, string Category, string Summary, bool IsFeatured);

/// <param name="DaysFromNow">Negative for a past event, positive for an upcoming one.</param>
public sealed record EventSample(string Title, int DaysFromNow, string Venue, string Summary);
