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
            HeroImageUrl = "https://images.unsplash.com/photo-1580582932707-520aed937b7b?auto=format&fit=crop&w=1800&q=80",
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
            HeroImageUrl = "https://images.unsplash.com/photo-1509062522246-3755977927d7?auto=format&fit=crop&w=1800&q=80",
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
            HeroImageUrl = "https://images.unsplash.com/photo-1592280771190-3e2e4d571952?auto=format&fit=crop&w=1800&q=80",
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
            HeroImageUrl = "https://images.unsplash.com/photo-1541339907198-e08756dedf3f?auto=format&fit=crop&w=1800&q=80",
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
            HeroImageUrl = "https://images.unsplash.com/photo-1523050854058-8df90110c9f1?auto=format&fit=crop&w=1800&q=80",
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
        },

        new SiteTemplate
        {
            Key = "notice-board-school",
            Name = "Notice Board School",
            Summary = "Dense and practical: notices beside the welcome, a tight figures strip, and everything a parent needs within one screen.",
            BestFor = "Schools that publish circulars, timetables and downloads constantly",
            WebsiteType = WebsiteType.School,
            HomeVariant = HomeVariant.Bulletin,
            PrimaryColor = "#29166f",
            SecondaryColor = "#ff3115",
            SampleTagline = "Learn to lead",
            Highlights =
            [
                "Four claims in a strip above the welcome, where a parent reads first",
                "Notices as a dated list beside the welcome, not a grid of cards",
                "Condensed headings and a tighter page — more visible without scrolling",
                "Figures band in the school's own colour"
            ],
            HeroImageUrl = "https://images.unsplash.com/photo-1577896851231-70ef18881754?auto=format&fit=crop&w=1800&q=80",
            Settings = new Dictionary<string, object?>
            {
                ["noticeLabel"] = "NOTICE",
                ["noticeTickerScrolls"] = true,
                ["noticeTickerRepeat"] = 1,
                ["headerContact"] = true,
                ["headerCtaText"] = "Admission Enquiry",
                ["headerCtaLink"] = "/admission",
                ["topBarIcons"] = true,
                ["scrollAnimations"] = true
            },
            HeroHeading = "Where every student is an achiever",
            HeroDescription = "An all-round education — physical, social, emotional, intellectual and cultural — since 1984.",
            Statistics = new StatisticsSample(1450, 76, 340, 41),
            WhyIntro = "Four things we are asked about most, answered before you have to ask.",
            PageContent = new Dictionary<string, string>
            {
                ["about"] = """
<p>{name} works to a simple motto: learn to lead. We believe in the all-round development of a child — physical, social, emotional, intellectual and cultural — and our founding principle is to develop the individuality of each one.</p>
<h2>Our objectives</h2><p>To provide quality education that helps students become responsible citizens, through continual improvement and consistent implementation of a quality management system.</p>
<h2>Our team</h2><p>Well qualified and dedicated teachers who between them account for the school's academic record and its reputation for pastoral care.</p>
""",
                ["admission"] = """
<h2>Registration and admission</h2><p>Admission is open to all irrespective of caste, creed and community.</p>
<h2>Minimum age</h2><p>At the beginning of the session: 2+ for Play Group, 3+ for Nursery.</p>
<h2>How to apply</h2><p>Apply to the Principal on the form supplied with the prospectus. On receipt of the completed form the child's name is entered on the waiting list, and suitable candidates are called for an entrance test.</p>
<h2>Documents</h2><p>Birth certificate, transfer certificate from the previous school, report card of the last class completed, and two passport photographs.</p>
""",
                ["facilities"] = """
<h2>Laboratories</h2><p>Physics, chemistry, biology and computer laboratories, each supervised by a subject teacher.</p>
<h2>Library</h2><p>Reference and lending sections, with periodicals and a reading room.</p>
<h2>Sports</h2><p>Playing fields, indoor games and a covered assembly area used through the monsoon.</p>
<h2>Transport</h2><p>School buses covering the city on fixed routes, with an attendant on every bus.</p>
""",
                ["messages"] = """
<h2>Director's message</h2><p>I feel blessed to be part of the noble act of imparting knowledge. We provide the best possible environment to develop the power of observation, stimulate curiosity and build the capacity to think and learn.</p>
<h2>Principal's message</h2><p>This school has come to symbolise dedication to maintaining an excellent standard of education. The outcome has been consistent excellence, not only in academic results but in every field of extracurricular activity.</p>
""",
                ["committee"] = """
<p>{name} is managed by a committee constituted under the rules of the affiliating board.</p>
<h2>Members</h2><p>The Chairman, the Manager, the Principal, two parent representatives, two teacher representatives and a nominee of the board.</p>
<h2>Meetings</h2><p>The committee meets each quarter. Minutes are held at the school office and available for inspection on request.</p>
""",
            },
            Faculty =
            [
                new ContentSample("Director", "Mrs. Neelam Malhotra", "Oversees the school and its pre-primary wing.", "Leadership", "M.A"),
                new ContentSample("Principal", "Mrs. Ruchi Kohli", "Academics, staff and day-to-day running.", "Leadership", "M.Ed"),
                new ContentSample("Head, Primary", "Mr. Sanjeev Awasthi", "Classes I to V.", "Leadership", "B.Ed")
            ],
            Departments =
            [
                new DepartmentSample("Senior School", "Classes IX to XII across science, commerce and humanities.",
                    ["Science", "Commerce", "Humanities"]),
                new DepartmentSample("Primary School", "Classes I to V, with a class teacher for each section.",
                    ["Languages", "Mathematics", "Environmental Studies"]),
                new DepartmentSample("Pre-School", "Play Group to Upper Kindergarten.",
                    ["Play Group", "Nursery", "Kindergarten"])
            ],
            News =
            [
                new NewsSample("Examination timetables published", "Notice", "Timetables for the board examinations are on the Downloads page.", true),
                new NewsSample("Winter vacation for Classes I to V", "Circular", "School closed from 28 December to 14 January.", false),
                new NewsSample("Admission open for the coming session", "Notice", "Call the office or complete the enquiry form on this site.", false)
            ],
            Events =
            [
                new EventSample("Parent–Teacher Meeting", 6, "Classrooms", "Term reports and a conversation with each class teacher."),
                new EventSample("Annual Function", 26, "School hall", "Music, drama and prize distribution."),
                new EventSample("Inter-House Sports", -9, "Playing field", "Track and field across all four houses.")
            ]
        },

        new SiteTemplate
        {
            Key = "campus-prospectus",
            Name = "Campus Prospectus",
            Summary = "Spacious and editorial: one statement at a time, a principal's portrait at full size, and a wall of photographs.",
            BestFor = "Established institutions whose website is a prospectus, not a noticeboard",
            WebsiteType = WebsiteType.School,
            HomeVariant = HomeVariant.Atrium,
            PrimaryColor = "#0a438d",
            SecondaryColor = "#991f22",
            SampleTagline = "We learn to serve",
            Highlights =
            [
                "Large display type with room around it — few things per screen, each given weight",
                "Principal's portrait and quotation as a full section",
                "Numbered pillars, one argument per row, rather than a grid of tiles",
                "Edge-to-edge photograph wall"
            ],
            HeroImageUrl = "https://noida.cambridgeschool.edu.in/wp-content/uploads/sites/12/2025/07/ad-1920x1080-1.jpg",
            HeroImages =
            [
                "https://noida.cambridgeschool.edu.in/wp-content/uploads/sites/12/2025/07/ad-1920x1080-1.jpg",
                "https://noida.cambridgeschool.edu.in/wp-content/uploads/sites/12/2025/09/Neutral-Modern-Top-Dishes-Must-Try-When-Visiting-Certain-Country-Youtube-Thumbnail.jpg",
                "https://noida.cambridgeschool.edu.in/wp-content/uploads/sites/12/2023/06/1.jpg"
            ],
            HeroAutoplaySeconds = 6,
            Settings = new Dictionary<string, object?>
            {
                ["heroPlainImages"] = true,
                ["popupEnabled"] = true,
                // Two posters, shown whole. The reference site opens on exactly these.
                ["popupImageUrl"] = string.Join('|',
                    "https://noida.cambridgeschool.edu.in/wp-content/uploads/sites/12/2026/06/Nursery-Admission-Open-2027-2028.jpeg",
                    "https://noida.cambridgeschool.edu.in/wp-content/uploads/sites/12/2026/07/WhatsApp-Image-2026-07-02-at-10.40.22-AM.jpeg"),
                ["popupHeading"] = "Admission Open 2027-2028",
                ["popupSlideSeconds"] = 3,
                ["popupOncePerVisit"] = false,
                ["noticeTicker"] = string.Join('|',
                    "Admission Open 2027-2028 — apply online.",
                    "Syllabus and datesheets for 2026-27 are on the Downloads page."),
                ["noticeLabel"] = "LATEST",
                ["headerContact"] = true,
                ["noticeTickerScrolls"] = true,
                ["noticeTickerRepeat"] = 1,
                ["headerCtaText"] = "Admission Enquiry",
                ["headerCtaLink"] = "/admission",
                ["topBarIcons"] = true,
                ["scrollAnimations"] = true,
                ["topBarLinks"] = string.Join('\n',
                    "Alumni|/alumni",
                    "E-Newsletter|/newsletter",
                    "Parent Portal|/parent-portal",
                    "Mandatory Disclosure|/disclosure")
            },
            HomeSections =
            [
                new SectionSample("why_choose_us", "What Makes Our School Special", null, null,
                    @"{""intro"": ""What makes our school special."", ""items"": [{""title"": ""Activity-Based Learning"", ""description"": ""We encourage our students to experiment and go beyond the normal learning methodologies, and promote personal learning experience through practical activities and real-time problem-solving drills.""}, {""title"": ""Varying Teaching Modalities"", ""description"": ""We have practically shifted from blackboard to digital board, with the aim of staying up to date with ever-changing teaching styles to give our students the best of all.""}, {""title"": ""Academic Brilliance"", ""description"": ""We encourage our students to thrive towards academic excellence by offering seamless guidance and motivation. Their progress is tracked by weekly assessments and tests that also help us focus on individual shortcomings.""}, {""title"": ""All-Round Development"", ""description"": ""Development is not limited to academic progress; it involves physical and psychological growth. The school offers a wide horizon of opportunities to help children unfurl their true selves and develop to their full potential.""}]}"),
                new SectionSample("crest", "We Learn To Serve", null,
                    "https://noida.cambridgeschool.edu.in/wp-content/uploads/sites/12/2023/02/we-learn-icon.png",
                    @"{""intro"": ""Our motto — 'We learn to serve' — reflects concern about others, the resolve to listen and to help."", ""items"": [{""symbol"": ""The book"", ""meaning"": ""Signifies learning""}, {""symbol"": ""The star"", ""meaning"": ""Signifies the aspiration to excel""}, {""symbol"": ""The torch"", ""meaning"": ""Signifies the flame of courage and leadership""}, {""symbol"": ""The tree"", ""meaning"": ""Signifies rootedness in integrity and the development of empathy with the surroundings""}]}"),
                new SectionSample("principal", "Principal's Message", null,
                    "https://noida.cambridgeschool.edu.in/wp-content/uploads/sites/12/2026/06/SurabhiBhargav-1024x683.jpeg",
                    @"{""name"": ""Ms. Surabhi Bhargav"", ""designation"": ""Principal"", ""quote"": ""True education nurtures the whole person — mind, body and character — by embracing experiences that challenge and refine us."", ""message"": ""We believe that true learning stems from experience. Young minds grow and evolve not only through academic knowledge but through the experiences that shape their values and character. We are committed to providing a nurturing environment where every student is encouraged to explore, reflect, and make thoughtful choices that reflect their personal beliefs.""}"),
                new SectionSample("alumni", "Eminent Cambridgians", null, null,
                    @"{""intro"": ""Eminent Cambridgians."", ""items"": [{""name"": ""Salman Khan"", ""role"": ""Actor"", ""imageUrl"": ""https://noida.cambridgeschool.edu.in/wp-content/uploads/sites/12/2023/03/salman-khan.jpg""}, {""name"": ""Saurabh Sen Sharma"", ""role"": ""Alumnus"", ""imageUrl"": ""https://noida.cambridgeschool.edu.in/wp-content/uploads/sites/12/2023/03/saurabh-sen-verma.jpg""}, {""name"": ""Namrata Tomar"", ""role"": ""Alumna"", ""imageUrl"": ""https://noida.cambridgeschool.edu.in/wp-content/uploads/sites/12/2023/03/namrata.jpg""}]}"),
                new SectionSample("gallery", "Life @@ Cambridge", null, null,
                    @"{""items"": [{""imageUrl"": ""https://noida.cambridgeschool.edu.in/wp-content/uploads/sites/12/2023/06/32.jpg"", ""caption"": ""Life on campus""}, {""imageUrl"": ""https://noida.cambridgeschool.edu.in/wp-content/uploads/sites/12/2023/06/14.jpg"", ""caption"": ""Life on campus""}, {""imageUrl"": ""https://noida.cambridgeschool.edu.in/wp-content/uploads/sites/12/2023/06/8.jpg"", ""caption"": ""Life on campus""}, {""imageUrl"": ""https://noida.cambridgeschool.edu.in/wp-content/uploads/sites/12/2023/06/1.jpg"", ""caption"": ""Life on campus""}, {""imageUrl"": ""https://noida.cambridgeschool.edu.in/wp-content/uploads/sites/12/2023/06/86.jpg"", ""caption"": ""Life on campus""}, {""imageUrl"": ""https://noida.cambridgeschool.edu.in/wp-content/uploads/sites/12/2023/06/112.jpg"", ""caption"": ""Life on campus""}, {""imageUrl"": ""https://noida.cambridgeschool.edu.in/wp-content/uploads/sites/12/2023/06/110.jpg"", ""caption"": ""Life on campus""}, {""imageUrl"": ""https://noida.cambridgeschool.edu.in/wp-content/uploads/sites/12/2023/06/74.jpg"", ""caption"": ""Life on campus""}, {""imageUrl"": ""https://noida.cambridgeschool.edu.in/wp-content/uploads/sites/12/2023/06/88.jpg"", ""caption"": ""Life on campus""}, {""imageUrl"": ""https://noida.cambridgeschool.edu.in/wp-content/uploads/sites/12/2023/06/85.jpg"", ""caption"": ""Life on campus""}, {""imageUrl"": ""https://noida.cambridgeschool.edu.in/wp-content/uploads/sites/12/2023/06/22-2.jpg"", ""caption"": ""Life on campus""}, {""imageUrl"": ""https://noida.cambridgeschool.edu.in/wp-content/uploads/sites/12/2023/06/16.jpg"", ""caption"": ""Life on campus""}]}")
            ],
            HeroHeading = "We learn to serve",
            HeroDescription = "Established in 1981, and one of the oldest and most prestigious schools in the city — state-of-the-art infrastructure, experienced faculty, and teaching built around critical thinking and creativity.",
            Statistics = new StatisticsSample(2100, 140, 96, 45),
            WhyIntro = "What makes our school special.",
            PageContent = new Dictionary<string, string>
            {
                ["about"] = """
<p>{name} is among the oldest institutions in the city, and its reputation rests on more than age. The campus is equipped for the way children learn now, and the faculty is experienced enough to know when that should be resisted.</p>
<h2>Our motto</h2><p>'We learn to serve' reflects a concern for others, the resolve to listen, and to help.</p>
<h2>The crest</h2><p>The book signifies learning. The star, the aspiration to excel. The torch, the flame of courage and leadership. The tree, rootedness in integrity and empathy with one's surroundings.</p>
<h2>Founder and history</h2><p>Founded in 1931 in a small flat, moved twice as it grew, and by the nineteen forties already among the foremost schools in the region.</p>
""",
                ["admission"] = """
<p>Admission to {name} is a conversation, not a transaction. We want to understand the child, and we want families to understand us.</p>
<h2>Registration</h2><p>Register online during the admission window. Registration is not an application for a place; it opens the process.</p>
<h2>Interaction</h2><p>Children meet a teacher informally. Parents meet the Principal. Neither is an examination.</p>
<h2>Offer</h2><p>Offers are made in writing with a date by which the place must be confirmed.</p>
""",
                ["facilities"] = """
<h2>Chemistry laboratory</h2><p>Used extensively by the senior classes. Hands-on learning through trial and error makes the subject comprehensive and experiential.</p>
<h2>Physics laboratory</h2><p>Heat, motion, magnetism, electricity and buoyancy demonstrated in experiments that give the topic meaning.</p>
<h2>Biology laboratory and garden</h2><p>Flora and fauna in the laboratory, and composting, germination and the nitrogen cycle in the kitchen garden.</p>
<h2>Computer and multimedia laboratories</h2><p>Two equipped laboratories in the senior wing, with a separate suite for the primary school.</p>
<h2>Library</h2><p>A reading room, a reference collection and a lending section open through the school day.</p>
""",
                ["messages"] = """
<h2>Principal's message</h2><p>True education nurtures the whole person — mind, body and character — by embracing experiences that challenge and refine us. Young minds grow not only through academic knowledge but through the experiences that shape their values.</p>
<h2>Head Mistress's message</h2><p>The primary years are where a child decides whether school is a place they want to be. Everything else follows from that.</p>
""",
                ["committee"] = """
<p>{name} is administered by a society for the advancement of education, with a managing committee constituted under board rules.</p>
<h2>Managing committee</h2><p>The Chairman, the Secretary of the society, the Principal, parent and teacher representatives, and a board nominee.</p>
<h2>Other committees</h2><p>A committee for the prevention of sexual harassment, a student safety committee, and a grievance redressal committee. Contact details for each are held at the school office.</p>
""",
            },
            Faculty =
            [
                new ContentSample("Principal", "Ms. Surabhi Bhargav", "Leads the school and its academic direction.", "Leadership", "M.Sc., M.Ed."),
                new ContentSample("Vice Principal", "Ms. Sheetal Kapoor", "Senior school academics and staff development.", "Leadership", "M.A., B.Ed."),
                new ContentSample("Counsellor", "Ms. Sunetra Banerjee", "Pastoral care and the counsellor's corner.", "Student Support", "M.Phil Psychology")
            ],
            Departments =
            [
                new DepartmentSample("Curriculum and pedagogy", "How subjects are taught, and why.",
                    ["Reading", "Activity-based learning", "Assessment"]),
                new DepartmentSample("Beyond the classroom", "Sport, music, drama and service.",
                    ["Sports and games", "Performing arts", "Community service"]),
                new DepartmentSample("Innovation", "Spaces for making, testing and failing safely.",
                    ["Tinkering laboratory", "Robotics", "Design"])
            ],
            News =
            [
                new NewsSample("Class XII results announced", "Achievement", "Stream toppers and the school average published.", true),
                new NewsSample("Admissions open for Nursery and Prep", "Notice", "Registration is open for the coming session.", false),
                new NewsSample("Alumni meet", "Event", "Three decades of Cambridgians returned to campus.", false)
            ],
            Events =
            [
                new EventSample("Open Morning", 11, "Main campus", "Walk the campus and meet the faculty."),
                new EventSample("Founder's Day", 34, "Auditorium", "Marking the founding of the school in 1931."),
                new EventSample("Model United Nations", -6, "Conference hall", "Delegates from thirty schools.")
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

    /// <summary>
    /// The banner photograph the template ships with, so two schools created from different
    /// templates do not open on the same stock image. Replaced from the media library.
    /// </summary>
    public string? HeroImageUrl { get; init; }

    /// <summary>
    /// Pictures for the hero slideshow. With more than one the hero moves between them on its
    /// own and offers arrows; the school replaces them from the console like any other image.
    /// </summary>
    public IReadOnlyList<string> HeroImages { get; init; } = [];

    /// <summary>Seconds between slides. Zero leaves the hero still.</summary>
    public int HeroAutoplaySeconds { get; init; } = 6;

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

    /// <summary>
    /// Site settings the template starts a school on — the notice label, whether the header
    /// carries the phone and email, its one call to action, the portals above it.
    ///
    /// These are structure, not sample content: they are what makes one template's header read
    /// unlike another's, so they are written whether or not sample content was asked for. A
    /// school changes any of them in Site Settings afterwards.
    /// </summary>
    public IReadOnlyDictionary<string, object?> Settings { get; init; } =
        new Dictionary<string, object?>();

    /// <summary>
    /// Whole homepage sections the template fills in — the crest read symbol by symbol, the
    /// principal's portrait and words, the photographs, the alumni.
    ///
    /// Hero and statistics have their own fields because every template sets them. This is for
    /// the sections that make one school's homepage that school's: a template that ships colours
    /// and type but leaves every section empty produces a site nobody can be shown.
    /// </summary>
    public IReadOnlyList<SectionSample> HomeSections { get; init; } = [];

    /// <summary>Every template ships the full starter page set from the page gallery.</summary>
    public IReadOnlyList<string> PageTemplateKeys => PageTemplateKeys_;
    private static readonly string[] PageTemplateKeys_ = Domain.Constants.PageTemplateKeys.StarterPages;
}

/// <param name="Key">A key from <see cref="Domain.Constants.HomePageSectionKeys"/>.</param>
/// <param name="Json">The section's own payload, in the shape its renderer reads.</param>
public sealed record SectionSample(
    string Key, string? Title, string? SubTitle, string? ImageUrl, string Json);

public sealed record StatisticsSample(int Students, int Teachers, int Placements, int Years);

public sealed record ContentSample(
    string Designation, string FullName, string Headline, string Category, string Qualification);

public sealed record DepartmentSample(string Name, string Summary, IReadOnlyList<string> Programmes);

public sealed record NewsSample(string Headline, string Category, string Summary, bool IsFeatured);

/// <param name="DaysFromNow">Negative for a past event, positive for an upcoming one.</param>
public sealed record EventSample(string Title, int DaysFromNow, string Venue, string Summary);
