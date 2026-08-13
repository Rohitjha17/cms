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
            PageContent = new Dictionary<string, string>
            {
                ["about"] = """
<p>{name} opened its doors in 1984 with three classrooms and ninety children. Four decades later we teach over two thousand students, and the principle has not changed: know every child, and expect the best of them.</p>
<h2>What we stand for</h2><p>Academic rigour matters, and so does the person a child becomes. Our house system, service programme and sport are not extras bolted on to the timetable — they are how character is built.</p>
<h2>Our campus</h2><p>Purpose-built laboratories, a library of over twelve thousand titles, an auditorium seating six hundred, and playing fields that have hosted inter-school athletics for thirty years.</p>
""",
                ["admission"] = """
<p>Admissions to {name} open in December for the following academic year. We admit at every grade subject to availability, with the largest intake at Nursery and Class VI.</p>
<h2>How to apply</h2><p>Collect a form from the school office or download it below. Submit it with the child's birth certificate, the previous school's report card and two photographs.</p>
<h2>What happens next</h2><p>Families are invited for an informal interaction with the Principal. This is a conversation, not a test — we want to understand the child and answer your questions. Offers are made within two weeks.</p>
<h2>Fees</h2><p>The current fee structure is published under Mandatory Disclosure. Sibling concessions and merit scholarships are available; please ask the office.</p>
""",
                ["facilities"] = """
<h2>Learning spaces</h2><p>Three science laboratories, two computer laboratories, a mathematics resource room and a language laboratory, all timetabled so every class uses them weekly.</p>
<h2>Library</h2><p>Over twelve thousand titles, a quiet reading room and a junior section with picture books and graded readers. Open through the school day and for an hour after.</p>
<h2>Sport</h2><p>A 200-metre track, cricket and football fields, four badminton courts and a covered basketball court. Coaching in athletics, cricket, basketball and table tennis.</p>
<h2>Safety and care</h2><p>A full-time nurse, CCTV across the campus, GPS-tracked buses on eleven routes, and a counsellor available to every student.</p>
""",
                ["messages"] = """
<h2>From the Principal</h2><p>A school is judged by the adults its children become. At {name} we care as much about honesty, effort and kindness as we do about marks — and in our experience the two rarely travel separately.</p>
<h2>From the Management</h2><p>This institution was founded by parents who wanted something better for their children. That is still who we answer to. Our doors are open, and your questions are welcome.</p>
""",
                ["committee"] = """
<p>{name} is governed by a managing committee constituted under CBSE affiliation bye-laws. It meets quarterly and is responsible for policy, finance and appointments.</p>
<h2>Constitution</h2><p>The committee comprises the Chairman, the Manager, the Principal as ex-officio member, two teacher representatives elected by the staff, two parent representatives, and nominees of the affiliating board.</p>
<h2>Meetings</h2><p>Minutes of the last four meetings are available for inspection at the school office during working hours.</p>
""",
            },
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
            PageContent = new Dictionary<string, string>
            {
                ["about"] = """
<p>{name} was built around a simple question: what should school look like for children who will work with tools that do not exist yet?</p>
<h2>How we teach</h2><p>Classes cap at twenty-four. Learning is inquiry-led — students investigate, build and defend their thinking rather than memorise it. Every teacher is a subject specialist and a mentor to a small tutor group.</p>
<h2>Beyond the timetable</h2><p>A makerspace open to every year group, a debating society that competes nationally, and a service programme that puts students in the city rather than in a hall listening to talks about it.</p>
""",
                ["admission"] = """
<p>{name} admits a single cohort each year across Grades 1 to 9. Places are limited by our class-size commitment, so early application is advisable.</p>
<h2>The process</h2><p>Submit the online enquiry form. We will invite you to a campus morning where you meet teachers and see lessons in progress. Children spend a taster session with their prospective year group.</p>
<h2>What we look for</h2><p>Curiosity and willingness, not prior coaching. There is no entrance examination for the primary years.</p>
<h2>Fees and support</h2><p>The fee schedule is available on request. A limited number of need-based bursaries are offered each year.</p>
""",
                ["facilities"] = """
<h2>Makerspace</h2><p>3D printers, laser cutter, electronics benches and hand tools, supervised and open to every year group during and after school.</p>
<h2>Studios</h2><p>Dedicated art, music and drama studios with recording and rehearsal space.</p>
<h2>Learning commons</h2><p>An open library and study area designed for collaboration rather than silence, with breakout rooms for group projects.</p>
<h2>Sport and wellbeing</h2><p>A multi-purpose indoor court, outdoor turf, and a wellbeing team including a full-time counsellor.</p>
""",
                ["messages"] = """
<h2>From the Head of School</h2><p>We are not preparing children for the examination alone. We are preparing them to think clearly, work with others and stay curious long after they leave us. That is a harder brief, and a better one.</p>
<h2>From the Academic Team</h2><p>Ask us about our curriculum and we will show you student work, not a brochure. It is the most honest answer we have.</p>
""",
                ["committee"] = """
<p>{name} is governed by a board of trustees which meets each term and is accountable for academic standards, safeguarding and finance.</p>
<h2>Membership</h2><p>The board comprises the Chair, three trustees drawn from education and industry, the Head of School, and an elected parent representative.</p>
<h2>Safeguarding</h2><p>A designated safeguarding lead reports directly to the board. Our policy is available on request.</p>
""",
            },
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
            PageContent = new Dictionary<string, string>
            {
                ["about"] = """
<p>{name} sits on sixty acres, and boarding is not an add-on here — it is the shape of the day. Prep, sport, music and meals happen together, and independence is learned by practice.</p>
<h2>House life</h2><p>Every student belongs to one of four houses, each with a resident housemaster and matron. Houses compete, eat and look after one another.</p>
<h2>The rhythm of a day</h2><p>Morning assembly, six teaching periods, games every afternoon, supervised prep in the evening, and lights out at a sensible hour.</p>
""",
                ["admission"] = """
<p>{name} admits boarders to Classes VI through XI. Day places are limited and offered locally.</p>
<h2>Visiting first</h2><p>We ask every family to visit before applying. Boarding suits many children and not all of them, and an afternoon on campus tells you more than any prospectus.</p>
<h2>Applying</h2><p>Complete the application form with the previous school's report and a medical history. Shortlisted candidates attend an assessment weekend, staying overnight in a house.</p>
<h2>Fees</h2><p>Boarding fees cover tuition, accommodation, all meals, laundry and routine medical care. The schedule is published under Mandatory Disclosure.</p>
""",
                ["facilities"] = """
<h2>Houses</h2><p>Four residential houses with dormitories for junior students and study-bedrooms for seniors, each with common rooms and a resident housemaster.</p>
<h2>Dining</h2><p>A central dining hall serving four meals a day, with vegetarian and special-diet provision as standard.</p>
<h2>Sport</h2><p>A full-size astro-turf, cricket ground, 400-metre track, swimming pool and indoor courts. Games are timetabled every afternoon.</p>
<h2>Health</h2><p>A campus infirmary with resident nursing staff and a visiting doctor, and a hospital tie-up eight kilometres away.</p>
""",
                ["messages"] = """
<h2>From the Headmaster</h2><p>Parents entrust us with their children for most of the year. We do not take that lightly. Our first duty is that every child is safe, known and happy; the academics follow from there.</p>
<h2>From the Dean of Residence</h2><p>Boarding teaches what timetables cannot: how to live well alongside others. It is the part of our work I am proudest of.</p>
""",
                ["committee"] = """
<p>{name} is administered by a board of governors meeting three times a year, with a standing committee for boarding welfare.</p>
<h2>Membership</h2><p>The board comprises the Chair, the Headmaster, the Dean of Residence, three governors, and an elected representative of parents of boarders.</p>
<h2>Boarding welfare</h2><p>The welfare committee includes the school doctor and counsellor and reviews pastoral matters each term.</p>
""",
            },
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
            PageContent = new Dictionary<string, string>
            {
                ["about"] = """
<p>{name} has offered undergraduate and postgraduate education for over three decades, across arts, science and commerce.</p>
<h2>Academics</h2><p>Programmes are taught by faculty who examine, publish and supervise research. Our departments maintain their own laboratories, seminar programmes and student societies.</p>
<h2>Placements</h2><p>A dedicated placement cell runs from the first year: aptitude training, mock interviews and an annual drive that brought over six hundred offers last season.</p>
""",
                ["admission"] = """
<p>Admission to {name} is by merit, calculated on qualifying examination marks and, where applicable, the entrance test prescribed by the university.</p>
<h2>Eligibility</h2><p>Undergraduate applicants require a pass in Class XII from a recognised board with the subject combination specified for each programme. Postgraduate applicants require a relevant bachelor's degree.</p>
<h2>How to apply</h2><p>Apply online during the admission window. Upload the marksheet, transfer certificate, category certificate where claimed, and a recent photograph.</p>
<h2>Merit lists and reservation</h2><p>Merit lists are published on this website. Reservation is applied as mandated by the state and the affiliating university.</p>
""",
                ["facilities"] = """
<h2>Laboratories</h2><p>Departmental laboratories for physics, chemistry, botany, zoology and computer science, equipped to university specification.</p>
<h2>Library</h2><p>Over forty thousand volumes, subscriptions to national and international journals, and a digital section with access to online databases.</p>
<h2>Placement cell</h2><p>Interview rooms, a seminar hall for pre-placement talks, and a training calendar running through the year.</p>
<h2>Campus</h2><p>Canteen, common rooms, a gymnasium and sports grounds, with hostel accommodation for outstation students.</p>
""",
                ["messages"] = """
<h2>From the Principal</h2><p>A degree is a beginning, not a destination. Our task is to send graduates out able to think, to write clearly and to keep learning without being taught.</p>
<h2>From the Dean of Academics</h2><p>We hold to the syllabus and go beyond it. Students who ask for more will always find a member of faculty willing to give it.</p>
""",
                ["committee"] = """
<p>{name} is governed by a managing committee constituted under the statutes of the affiliating university.</p>
<h2>Membership</h2><p>The committee comprises the Chairman, the Secretary, the Principal, two senior faculty members, a university nominee and a representative of the non-teaching staff.</p>
<h2>Statutory committees</h2><p>Separate committees exist for internal quality assurance, grievance redressal, anti-ragging and prevention of sexual harassment, as required by regulation.</p>
""",
            },
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
            PageContent = new Dictionary<string, string>
            {
                ["about"] = """
<p>{name} was founded in 1949 as a trust, and has been run as one ever since. No shareholders, no dividends — surpluses return to the institution.</p>
<h2>Our purpose</h2><p>To offer an education grounded in scholarship, character and service, accessible to families of ordinary means as well as to those of comfortable ones.</p>
<h2>Continuity</h2><p>Three generations of some families have passed through these gates. That continuity is not sentiment; it is the clearest evidence we have that the work is sound.</p>
""",
                ["admission"] = """
<p>Admission to {name} is by application to the Registrar, considered by the admissions committee.</p>
<h2>Applying</h2><p>Forms are issued from the trust office each January. Submit with the previous school record, a birth certificate and the names of two referees.</p>
<h2>Interview</h2><p>Shortlisted families meet the Principal and a member of the governing council. The conversation covers the child's interests and the family's expectations of us.</p>
<h2>Assistance</h2><p>The trust sets aside a portion of income each year for fee assistance. Applications are treated in confidence and considered on need alone.</p>
""",
                ["facilities"] = """
<h2>The main building</h2><p>The original 1949 hall, restored in 2018, seats four hundred and remains in daily use for assembly and examinations.</p>
<h2>Library and archive</h2><p>A reference library and an institutional archive holding records, photographs and correspondence from the founding years.</p>
<h2>Laboratories and studios</h2><p>Science laboratories, an art studio and a music room, refurbished within the last five years.</p>
<h2>Grounds</h2><p>Playing fields, a covered assembly area and gardens maintained by the trust.</p>
""",
                ["messages"] = """
<h2>From the Chairman</h2><p>A trust holds property for others. What we hold is not land or buildings but the confidence of the families who send us their children, and it is renewed or lost every year.</p>
<h2>From the Principal</h2><p>We change slowly and deliberately. What we teach has moved with the times; why we teach it has not.</p>
""",
                ["committee"] = """
<p>{name} is governed by a board of trustees constituted under its founding deed, meeting quarterly.</p>
<h2>The governing council</h2><p>Comprises the Chairman, the Honorary Secretary, the Treasurer, four trustees, the Principal as ex-officio member and a nominee of the founding family.</p>
<h2>Accountability</h2><p>Accounts are independently audited and the annual report is published. Both are available on request from the trust office.</p>
""",
            },
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

    /// <summary>
    /// Finished page copy, keyed by page-template key. <c>{name}</c> is replaced with the
    /// institution's own name at provisioning time so the site reads as theirs, not as a
    /// demo. Pages absent from here keep the gallery's default content.
    /// </summary>
    public IReadOnlyDictionary<string, string> PageContent { get; init; } =
        new Dictionary<string, string>();

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
