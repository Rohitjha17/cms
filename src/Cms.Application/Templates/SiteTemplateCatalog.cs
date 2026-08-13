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
            Key = "global-international-school",
            Name = "Global International School",
            Summary = "An IB and Cambridge-curriculum site with programme pathways, a global-outcomes band and alumni destinations.",
            BestFor = "International schools and IB/IGCSE curricula",
            WebsiteType = WebsiteType.School,
            HomeVariant = HomeVariant.Modern,
            PrimaryColor = "#0d5c63",
            SecondaryColor = "#f2a541",
            SampleTagline = "An international education, rooted in who your child is",
            Highlights =
            [
                "Programme pathways: Primary Years, Middle Years, Diploma",
                "University destinations and outcomes band",
                "Built for schools that admit mid-year and from abroad",
                "Reads well to parents relocating from another country"
            ],
            HeroImageUrl = "https://images.unsplash.com/photo-1577896851231-70ef18881754?auto=format&fit=crop&w=1800&q=80",
            HeroHeading = "Learners without borders",
            HeroDescription = "An inquiry-led international curriculum that travels with your family, wherever you go next.",
            Statistics = new StatisticsSample(1150, 96, 100, 18),
            WhyIntro = "Small classes, teachers from eleven countries, and a curriculum recognised by universities worldwide.",
            PageContent = new Dictionary<string, string>
            {
                ["about"] = """
<p>{name} is an international school teaching the Primary Years, Middle Years and Diploma programmes on one campus. Our families come from more than thirty nationalities, and many arrive mid-year — so we are built to welcome a child in October as readily as in April.</p>
<h2>How we teach</h2><p>Inquiry comes first. Children are taught to ask a good question, gather evidence, and defend a conclusion — in class, in the laboratory and on the stage. Subject knowledge is the floor, not the ceiling.</p>
<h2>Language</h2><p>English is the language of instruction. We additionally teach French, Spanish, Hindi and Mandarin, and run an English acquisition programme for students joining from another language.</p>
<h2>Accreditation</h2><p>Authorised for all three IB programmes and a registered Cambridge International centre. Our Diploma results have exceeded the world average in each of the last six sessions.</p>
""",
                ["admission"] = """
<p>{name} admits throughout the year wherever a place exists, because international families rarely move to the academic calendar.</p>
<h2>How to apply</h2><p>Submit the online enquiry, then send us two years of school reports and any external assessment results. Applications from abroad are handled entirely online, including the family interview.</p>
<h2>Assessment</h2><p>Applicants sit an age-appropriate assessment in English and mathematics — online if you are overseas. For the Diploma we also discuss subject choices and predicted grades.</p>
<h2>Relocating families</h2><p>Our admissions team can advise on visas, transport routes and housing near the campus, and pair your child with a student buddy for the first month.</p>
""",
                ["facilities"] = """
<h2>Learning</h2><p>Design-technology and robotics workshops, four science laboratories, a recording studio, a black-box theatre and a two-floor library with an independent-research commons for Diploma students.</p>
<h2>Sport</h2><p>A FIFA-standard football pitch, a twenty-five-metre pool, four tennis courts and an indoor sports hall. We compete in the international schools league across six sports.</p>
<h2>Wellbeing</h2><p>Two full-time counsellors, a school nurse, and a pastoral tutor for every year group. Every child is known by name by an adult responsible for them.</p>
<h2>Boarding</h2><p>A sixty-bed residence for Grades 9 to 12, with house parents, supervised study and weekend programmes.</p>
""",
                ["messages"] = """
<h2>From the Head of School</h2><p>An international school is not defined by its passports but by its habits of mind. We want students who can sit with a hard problem, listen to someone who disagrees, and change their mind when the evidence says so.</p>
<h2>From the Diploma Coordinator</h2><p>The Diploma is demanding, and it should be. Our job is to make it survivable and worth it — through subject choice that fits the student, and an extended essay they actually care about.</p>
""",
                ["committee"] = """
<p>{name} is governed by a board of trustees that meets each term. It is responsible for strategy, finance, safeguarding and the appointment of the Head of School.</p>
<h2>Composition</h2><p>The board comprises the Chair, four trustees appointed by the founding trust, two elected parent representatives, one alumni representative and the Head of School as an ex-officio member.</p>
<h2>Safeguarding</h2><p>A named safeguarding trustee reviews child-protection practice annually. Our safeguarding policy is published on this site and reviewed each year.</p>
""",
            },
            Faculty =
            [
                new ContentSample("Head of School", "Dr. Anneke Visser", "Leading the school since 2019.", "Leadership", "Ed.D, M.Ed"),
                new ContentSample("Diploma Coordinator", "Rahul Iyer", "IB Diploma and university guidance.", "Leadership", "M.Sc, PGCE"),
                new ContentSample("PYP Coordinator", "Sofia Marquez", "Early and primary years inquiry.", "Leadership", "M.Ed"),
                new ContentSample("Head of Sciences", "Dr. Chen Wei", "Physics, and the research programme.", "Teaching", "Ph.D (Physics)"),
                new ContentSample("Head of Languages", "Camille Roux", "French, Spanish and English acquisition.", "Teaching", "M.A, CELTA")
            ],
            Departments =
            [
                new DepartmentSample("Primary Years Programme", "Inquiry-led learning for ages 3 to 11, built around six transdisciplinary themes.",
                    ["Early Years", "Lower Primary", "Upper Primary", "Exhibition"]),
                new DepartmentSample("Middle Years Programme", "Ages 11 to 16, with a personal project in the final year.",
                    ["Sciences", "Individuals and Societies", "Design", "Arts"]),
                new DepartmentSample("Diploma Programme", "The two-year pre-university course, with the core taught across all subjects.",
                    ["Theory of Knowledge", "Extended Essay", "Creativity, Activity, Service"]),
                new DepartmentSample("University Guidance", "Applications to the UK, US, Canada, Europe, Australia and India.",
                    ["UCAS", "Common App", "Portfolio support"])
            ],
            News =
            [
                new NewsSample("Diploma results above the world average for a sixth year", "Achievement", "An average of 34 points, with four students at 42 or above.", true),
                new NewsSample("Mid-year admissions open for Grades 1 to 9", "Notice", "Places available for families relocating this term.", false),
                new NewsSample("Model United Nations team places second at the regional conference", "Achievement", "Delegates from eleven schools took part.", false)
            ],
            Events =
            [
                new EventSample("Virtual Open House for relocating families", 10, "Online", "A live tour and Q&A with the Head of School."),
                new EventSample("University Fair", 26, "Sports hall", "Admissions officers from forty universities."),
                new EventSample("International Day", -12, "Whole campus", "Food, dress and performance from every nationality on campus.")
            ]
        },

        new SiteTemplate
        {
            Key = "technology-institute",
            Name = "Institute of Technology",
            Summary = "An engineering and technology site led by placements, laboratories and industry partnerships.",
            BestFor = "Engineering colleges and technical institutes",
            WebsiteType = WebsiteType.College,
            HomeVariant = HomeVariant.Academic,
            PrimaryColor = "#1a2b6d",
            SecondaryColor = "#00a3a3",
            SampleTagline = "Engineers who can build the thing, not just describe it",
            Highlights =
            [
                "Placement record and recruiter list on the home page",
                "Branch-wise departments with laboratory detail",
                "Research, patents and industry-partnership sections",
                "Admissions page written around entrance-exam counselling"
            ],
            HeroImageUrl = "https://images.unsplash.com/photo-1581092160562-40aa08e78837?auto=format&fit=crop&w=1800&q=80",
            HeroHeading = "Build what comes next",
            HeroDescription = "Four-year engineering degrees taught in laboratories, not just lecture halls — with a placement cell that starts working in your second year.",
            Statistics = new StatisticsSample(3800, 240, 94, 26),
            WhyIntro = "Accredited programmes, laboratories that match industry, and a placement record we publish in full.",
            PageContent = new Dictionary<string, string>
            {
                ["about"] = """
<p>{name} has taught engineering for twenty-six years. We are approved by AICTE, affiliated to the state technical university, and our core branches hold NBA accreditation.</p>
<h2>How we teach</h2><p>Every core subject carries a laboratory component, and every student completes an eight-week industry internship and a two-semester capstone project. Theory that is never built is theory a student forgets.</p>
<h2>Industry</h2><p>We run joint laboratories with three industry partners, and practising engineers teach one elective per branch each semester. Our syllabus review board includes employers who hire our graduates.</p>
<h2>Research</h2><p>Funded research in power electronics, structural materials and applied machine learning, with undergraduates working alongside faculty from the third year.</p>
""",
                ["admission"] = """
<p>Admission to the B.Tech programmes at {name} is through the state engineering entrance examination and the centralised counselling process, with a management quota governed by the same academic minimum.</p>
<h2>Eligibility</h2><p>A pass in Class XII with Physics, Mathematics and one of Chemistry, Computer Science or Biotechnology, and a valid entrance rank.</p>
<h2>Counselling</h2><p>Choose {name} and your branch during centralised counselling. Our admissions desk runs a helpline through the counselling window — call before you lock your choices, not after.</p>
<h2>Lateral entry</h2><p>Diploma holders may enter the second year through the lateral-entry examination, subject to available seats.</p>
<h2>Fees and scholarships</h2><p>The fee structure is fixed by the state fee regulatory authority and published under Mandatory Disclosure. Merit scholarships cover full tuition for the top rank holders in each branch.</p>
""",
                ["facilities"] = """
<h2>Laboratories</h2><p>Thirty-two laboratories across the branches, including a high-voltage laboratory, a materials testing laboratory, a fabrication workshop with CNC and 3D printing, and a GPU cluster for machine-learning coursework.</p>
<h2>Library and digital resources</h2><p>Ninety thousand volumes, and institutional subscriptions to IEEE Xplore, ScienceDirect and ASME, accessible from the hostels.</p>
<h2>Hostels</h2><p>Separate residences for men and women, housing eighteen hundred students, with mess, gymnasium and twenty-four-hour internet.</p>
<h2>Incubation centre</h2><p>Seed grants, mentoring and workspace for student ventures. Nine companies have been incorporated out of the centre since 2019.</p>
""",
                ["messages"] = """
<h2>From the Principal</h2><p>An engineering degree should leave a graduate able to do something on the first day of the job. That is the standard we hold ourselves to, and it is why our laboratories matter more to us than our lecture halls.</p>
<h2>From the Training and Placement Officer</h2><p>We start in the second year: aptitude, communication, and the specific skills recruiters ask us for. Our placement record is published in full, including the students who chose higher study instead.</p>
""",
                ["committee"] = """
<p>{name} is governed by a Board of Governors constituted under AICTE norms, meeting twice a year.</p>
<h2>Composition</h2><p>The Board comprises the Chairman, nominees of the trust, a nominee of the affiliating university, a nominee of the state government, two senior faculty members, an industry representative and the Principal as Member Secretary.</p>
<h2>Statutory committees</h2><p>An Academic Council, a Grievance Redressal Committee, an Internal Complaints Committee and an Anti-Ragging Committee operate under the Board. Contact details for each are published on this site as required.</p>
""",
            },
            Faculty =
            [
                new ContentSample("Principal", "Dr. S. Venkataraman", "Power systems; heading the institute since 2016.", "Leadership", "Ph.D, M.Tech"),
                new ContentSample("Dean, Academics", "Dr. Priya Nair", "Curriculum, accreditation and examinations.", "Leadership", "Ph.D (Computer Science)"),
                new ContentSample("Training and Placement Officer", "Vikram Desai", "Recruiter relationships and placement training.", "Leadership", "MBA"),
                new ContentSample("Head, Computer Science", "Dr. Aisha Rahman", "Applied machine learning and systems.", "Teaching", "Ph.D, M.E"),
                new ContentSample("Head, Mechanical Engineering", "Dr. Joseph Mathew", "Materials and manufacturing.", "Teaching", "Ph.D, M.Tech")
            ],
            Departments =
            [
                new DepartmentSample("Computer Science and Engineering", "Systems, data and applied machine learning, with a GPU cluster for coursework.",
                    ["B.Tech CSE", "B.Tech AI and Data Science", "M.Tech CSE"]),
                new DepartmentSample("Electronics and Communication", "Embedded systems, VLSI and communication networks.",
                    ["B.Tech ECE", "M.Tech VLSI Design"]),
                new DepartmentSample("Mechanical Engineering", "Design, thermal sciences and manufacturing, with a CNC and fabrication workshop.",
                    ["B.Tech Mechanical", "M.Tech Manufacturing"]),
                new DepartmentSample("Civil Engineering", "Structures, geotechnics and transportation, with a materials testing laboratory.",
                    ["B.Tech Civil", "M.Tech Structural Engineering"]),
                new DepartmentSample("Electrical and Electronics", "Power systems, drives and renewable energy integration.",
                    ["B.Tech EEE", "M.Tech Power Electronics"])
            ],
            News =
            [
                new NewsSample("Placement season closes at 94% with 212 recruiters on campus", "Placement", "The highest offer this year was 32 lakh per annum; the median was 6.4 lakh.", true),
                new NewsSample("Counselling helpline open for the 2026 admission cycle", "Notice", "Call the admissions desk before locking your branch choices.", false),
                new NewsSample("Two patents granted to the power electronics group", "Research", "Both filed with undergraduate co-inventors.", false)
            ],
            Events =
            [
                new EventSample("Campus recruitment drive: core engineering", 18, "Placement centre", "Eleven manufacturing and infrastructure recruiters."),
                new EventSample("TechFest", 40, "Whole campus", "Robotics, coding and design competitions open to all colleges."),
                new EventSample("Industry syllabus review board", -8, "Board room", "Employers review the branch syllabi for the coming year.")
            ]
        },

        new SiteTemplate
        {
            Key = "early-years-academy",
            Name = "Early Years Academy",
            Summary = "A warm, photograph-led site for pre-schools and primary schools, built around reassurance and safety.",
            BestFor = "Pre-schools, kindergartens and primary schools",
            WebsiteType = WebsiteType.School,
            HomeVariant = HomeVariant.Campus,
            PrimaryColor = "#2f6b4f",
            SecondaryColor = "#f08a3c",
            SampleTagline = "Where the first years are the best years",
            Highlights =
            [
                "Large photography and a gentle, uncrowded layout",
                "Safety, staffing ratios and a typical day up front",
                "Written for parents choosing a first school",
                "Short pages a parent can read on a phone"
            ],
            HeroImageUrl = "https://images.unsplash.com/photo-1587654780291-39c9404d746b?auto=format&fit=crop&w=1800&q=80",
            HeroHeading = "A happy start lasts a lifetime",
            HeroDescription = "A small, safe pre-school where children are known by name, and parents are told how the day really went.",
            Statistics = new StatisticsSample(320, 34, 100, 12),
            WhyIntro = "Small groups, low ratios, and teachers who have been here long enough for the children to trust them.",
            PageContent = new Dictionary<string, string>
            {
                ["about"] = """
<p>{name} is a pre-school and primary school for children from eighteen months to eleven years. We keep the school deliberately small, so that every adult knows every child.</p>
<h2>How children learn here</h2><p>Through play, then through structure. In the early years the day is built around exploration, story, movement and rest. Formal literacy and numeracy arrive gradually, and never at the cost of a child's confidence.</p>
<h2>Our ratios</h2><p>One adult to six children in the toddler group, one to ten in kindergarten, and a maximum class size of twenty-two in primary. These are limits we hold to, not averages.</p>
<h2>Talking to parents</h2><p>You will hear from us more than you expect. A daily note in the early years, a photograph when something lovely happens, and an honest conversation the moment we are worried about something.</p>
""",
                ["admission"] = """
<p>{name} admits children from eighteen months. Places are limited by our ratios, so we open a waiting list for each age group in November.</p>
<h2>Visit first</h2><p>We do not take an application from a family who has not visited. Come during a working morning, watch a session, and see whether the children look happy — that is the only test that matters.</p>
<h2>What we need</h2><p>The completed form, the child's birth certificate, an immunisation record and two photographs. There is no assessment and no interview for the child.</p>
<h2>Settling in</h2><p>New children start with short days, extended over a fortnight at the child's pace. A parent is welcome to stay for the first few sessions.</p>
""",
                ["facilities"] = """
<h2>Classrooms</h2><p>Bright, low-shelved rooms with reading corners, water and sand play, and everything within a child's reach. Every room opens onto the garden.</p>
<h2>Outdoors</h2><p>A shaded play garden with soft-fall surfacing, a climbing frame, a sandpit and a vegetable patch each class plants and harvests.</p>
<h2>Safety</h2><p>Secure single-point entry with staffed reception, CCTV in every shared space, verified pick-up only, and staff trained in paediatric first aid. Our safeguarding policy is published and reviewed each year.</p>
<h2>Food and rest</h2><p>A freshly cooked vegetarian lunch and two snacks, planned by a nutritionist, with allergy protocols for every child. A quiet nap room for the youngest groups.</p>
""",
                ["messages"] = """
<h2>From the Head Teacher</h2><p>Small children do not need to be hurried. Given time, warmth and something interesting to do, they learn faster than any curriculum can push them. Our job is to protect that.</p>
<h2>To parents</h2><p>Leaving your child somewhere for the first time is hard. Ask us anything, visit whenever you like, and tell us what worries you — we would far rather hear it early.</p>
""",
                ["committee"] = """
<p>{name} is run by a management committee that meets each term, with a standing parent representative from each age group.</p>
<h2>Composition</h2><p>The Head Teacher, two trustees, an early-childhood education adviser, and three elected parent representatives.</p>
<h2>Safeguarding</h2><p>A named safeguarding lead is responsible for child protection, and all staff complete annual safeguarding training. Concerns may be raised with the lead or any committee member.</p>
""",
            },
            Faculty =
            [
                new ContentSample("Head Teacher", "Nandini Rao", "With the school since it opened.", "Leadership", "M.Ed (Early Childhood)"),
                new ContentSample("Early Years Lead", "Grace Fernandes", "Toddler and nursery groups.", "Teaching", "B.Ed, Montessori Diploma"),
                new ContentSample("Primary Coordinator", "Sneha Kulkarni", "Classes 1 to 5.", "Teaching", "M.A, B.Ed"),
                new ContentSample("Safeguarding Lead", "Deepa Menon", "Child protection and pastoral care.", "Leadership", "M.S.W")
            ],
            Departments =
            [
                new DepartmentSample("Toddler group", "Eighteen months to three years, at one adult to every six children.",
                    ["Play and exploration", "Language and song", "Motor skills"]),
                new DepartmentSample("Kindergarten", "Three to six years, where early literacy and numeracy begin through play.",
                    ["Early literacy", "Early numeracy", "Music and movement", "Art"]),
                new DepartmentSample("Primary", "Classes 1 to 5, in classes of no more than twenty-two.",
                    ["English", "Mathematics", "Environmental Studies", "Art and Craft"])
            ],
            News =
            [
                new NewsSample("Waiting list opens for the 2026 toddler group", "Notice", "Visit us before applying — places are limited by our ratios.", true),
                new NewsSample("Our vegetable patch fed the whole school lunch this week", "Story", "Class 3 planted it in June.", false),
                new NewsSample("Paediatric first aid refresher completed by all staff", "Notice", "Certification renewed for every member of staff.", false)
            ],
            Events =
            [
                new EventSample("Come and See morning", 8, "Main gate", "Watch a working session and meet the teachers."),
                new EventSample("Grandparents' Day", 22, "Play garden", "Songs, stories and tea."),
                new EventSample("Annual Sports Morning", -15, "Play garden", "Races, games and a lot of laughing.")
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
