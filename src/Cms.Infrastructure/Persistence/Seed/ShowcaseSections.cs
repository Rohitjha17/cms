using System.Globalization;
using Cms.Domain.Constants;
using Cms.Domain.Entities;

namespace Cms.Infrastructure.Persistence.Seed;

/// <summary>
/// All thirty home page sections, filled, for one showcase website.
///
/// Every section the console can edit is here and switched on. A demo that leaves half of them
/// empty shows a school a shorter page than it would actually get, and leaves the other half
/// untested — which is the same reason the per-section options (a section's own entrance, its
/// own hover, its own backdrop) are set on a spread of sections rather than on none.
/// </summary>
internal static class ShowcaseSections
{
    internal static List<HomePageSection> Build(Guid tenantId, ShowcaseSpec s)
    {
        var now = DateTime.UtcNow;
        string D(int days) => now.AddDays(days).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var order = 0;
        var list = new List<HomePageSection>();

        void Add(
            string key,
            string title,
            string? subTitle,
            string? description,
            string? json,
            string? buttonText = null,
            string? buttonLink = null,
            string? imageUrl = null,
            string? backgroundImageUrl = null)
            => list.Add(new HomePageSection
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                SiteId = s.SiteId,
                SectionKey = key,
                Title = title,
                SubTitle = subTitle,
                Description = description,
                ButtonText = buttonText,
                ButtonLink = buttonLink,
                ImageUrl = imageUrl,
                BackgroundImageUrl = backgroundImageUrl,
                JsonData = json,
                DisplayOrder = ++order,
                IsActive = true,
                CreatedDate = now,
                CreatedBy = "showcase"
            });

        // The carousel reads an items array of {imageUrl, alt}, and its own pace from
        // autoplaySeconds — not a newline-separated list, which is what the settings screen
        // takes for the popup and is easy to assume applies here too.
        var slides = string.Join(",", s.HeroSlides.Select((url, i) =>
            $$"""{"imageUrl":"{{url}}","alt":"{{s.ShortName}} campus, picture {{i + 1}}"}"""));

        // ---------------------------------------------------------------- 1. Hero
        Add(HomePageSectionKeys.Hero,
            s.Name,
            s.Tagline,
            $"<p>{s.Tagline}.</p>",
            $$"""
            {"heading":"{{s.Tagline}}","description":"{{s.ShortName}} has taught the children of {{s.City}} since {{s.Founded}}.","primaryButton":"Apply now","secondaryButton":"Visit us","autoplaySeconds":"6","items":[{{slides}}]}
            """,
            buttonText: "Apply now",
            buttonLink: "/admission",
            imageUrl: s.Banner,
            backgroundImageUrl: s.Banner);

        // ---------------------------------------------------------------- 2. Welcome
        Add(HomePageSectionKeys.Welcome,
            $"Welcome to {s.ShortName}",
            "A community built around possibility",
            $"<p>For more than {DateTime.UtcNow.Year - int.Parse(s.Founded, CultureInfo.InvariantCulture)} years {s.ShortName} has taught children to think carefully, speak honestly and look after one another. We are large enough to offer everything a young person needs and small enough that every child is known by name.</p><p>Our doors are open to visiting families throughout the year. Come and see an ordinary day.</p>",
            """{"animation":"fade"}""",
            buttonText: "Read more",
            buttonLink: "/about",
            imageUrl: s.Gallery[0]);

        // ---------------------------------------------------------------- 3. About
        Add(HomePageSectionKeys.About,
            "About the school",
            s.Motto,
            $"<p>{s.ShortName} was founded in {s.Founded} by {s.FounderName}, on the belief that a school should form character as carefully as it teaches a syllabus. That belief has outlasted every change of curriculum since.</p><p>Today we teach {(s.Variant == Domain.Enums.HomeVariant.Campus ? "820" : "1,450")} students across the primary, middle and senior schools, with a staff of {(s.Variant == Domain.Enums.HomeVariant.Campus ? "74" : "96")}.</p>",
            """{"background":"diagonal","hover":"lift"}""",
            buttonText: "Our story",
            buttonLink: "/about");

        // ------------------------------------------------- 4–7. Leadership messages
        Add(HomePageSectionKeys.Principal,
            "Principal's message", "From the Principal",
            "<p>Our first duty is that every child is safe, known and happy. The academic results follow from that, and never the other way round. I invite you to visit us on a working day and judge the school by what you see in its corridors.</p>",
            $$"""{"personName":"{{s.PrincipalName}}","designation":"Principal","quote":"Education should help every learner find their voice, purpose and courage."}""",
            imageUrl: "https://images.unsplash.com/photo-1544717305-2782549b5136?auto=format&fit=crop&w=600&q=80");

        Add(HomePageSectionKeys.Chairman,
            "Chairman's message", "From the Chairman",
            "<p>A school is a promise made to a family, renewed every morning. The board's work is to see that the promise is kept — in the quality of the teaching, in the care of the buildings, and in the fees we ask.</p>",
            $$"""{"personName":"{{s.ChairmanName}}","designation":"Chairman","quote":"We prepare young people not only for examinations, but for a life of contribution."}""",
            imageUrl: "https://images.unsplash.com/photo-1560250097-0b93528c311a?auto=format&fit=crop&w=600&q=80");

        Add(HomePageSectionKeys.Director,
            "Director's message", "From the Director",
            "<p>Curriculum, staffing and standards are my responsibility. We teach a demanding syllabus without hurrying children through it, because understanding that is rushed is understanding that does not last.</p>",
            $$"""{"personName":"{{s.DirectorName}}","designation":"Director","quote":"Depth before pace. A child who understands will always catch up; a child who has only kept up will not."}""",
            imageUrl: "https://images.unsplash.com/photo-1573497019940-1c28c88b4f3e?auto=format&fit=crop&w=600&q=80");

        Add(HomePageSectionKeys.Manager,
            "Manager's message", "From the Manager",
            "<p>Transport, meals, maintenance and the office are mine to run. If any of them is not working for your family, write to me directly and I will answer within two working days.</p>",
            $$"""{"personName":"{{s.ManagerName}}","designation":"Manager","quote":"A school runs on a hundred small things going right before the first bell."}""",
            imageUrl: "https://images.unsplash.com/photo-1519085360753-af0119f7cbe7?auto=format&fit=crop&w=600&q=80");

        // ---------------------------------------------------------------- 8. Statistics
        Add(HomePageSectionKeys.Statistics,
            "The school in figures", "Last reviewed this term", null,
            s.Variant == Domain.Enums.HomeVariant.Campus
                ? """{"students":820,"teachers":74,"placements":100,"years":32}"""
                : """{"students":1450,"teachers":96,"placements":340,"years":88}""");

        // ---------------------------------------------------------------- 9. Courses
        Add(HomePageSectionKeys.Courses,
            "Streams and pathways", "Pathways designed for ambition, curiosity and impact", null,
            """
            {"hover":"zoom","items":[
              {"title":"Science & Technology","description":"Physics, Chemistry, Biology and Computer Science, with laboratories open beyond school hours.","url":"/departments"},
              {"title":"Commerce & Economics","description":"Accountancy, Business Studies and Economics, with a student-run enterprise each year.","url":"/departments"},
              {"title":"Humanities","description":"History, Political Science, Psychology and Literature.","url":"/departments"},
              {"title":"Performing & Visual Arts","description":"Music, theatre, dance and studio art as timetabled subjects, not clubs.","url":"/departments"}
            ]}
            """,
            buttonText: "All departments", buttonLink: "/departments");

        // ---------------------------------------------------------------- 10. Departments
        Add(HomePageSectionKeys.Departments,
            "Departments", "Deep expertise, connected learning", null,
            """
            {"items":[
              {"title":"Sciences","description":"Four laboratories, a dedicated robotics room and a weekly research seminar."},
              {"title":"Mathematics","description":"Setted from Class VI, with an olympiad group and a support group of equal standing."},
              {"title":"Languages","description":"English, Hindi and a third language from Class V. Debating and a literary magazine."},
              {"title":"Humanities","description":"Fieldwork every year: archives, courts, local government and heritage sites."},
              {"title":"Sport & Wellbeing","description":"Games every afternoon, a counsellor on staff and a full-time nurse."},
              {"title":"Performing Arts","description":"Two productions and three concerts a year, open to every year group."}
            ]}
            """);

        // ---------------------------------------------------------------- 11. Why choose us
        Add(HomePageSectionKeys.WhyChooseUs,
            "Why families choose us", "An education that goes beyond achievement", null,
            """
            {"intro":"A learning environment designed around every student's potential.","columns":3,"animation":"rise","items":[
              {"title":"Known by name","description":"Class sizes capped at 28, and a form tutor who stays with a group for three years."},
              {"title":"Taught by specialists","description":"Every subject from Class VI is taught by a specialist in it."},
              {"title":"Answered the same day","description":"The office answers a parent's message within one working day. We publish our record."},
              {"title":"Nothing hidden","description":"Fees, results and inspection reports are published in full under Mandatory Disclosure."},
              {"title":"A real timetable for sport","description":"Games are timetabled for every child every week, not offered to the few who are good at them."},
              {"title":"Ready for what follows","description":"University counselling from Class IX, and an alumni network that answers."}
            ]}
            """,
            buttonText: "Facilities", buttonLink: "/facilities");

        // ---------------------------------------------------------------- 12. Announcements
        Add(HomePageSectionKeys.Announcements,
            "Announcements", "Important updates from across our community", null,
            $$"""
            {"hover":"tilt","items":[
              {"title":"Admissions open for 2027–28","date":"{{D(-2)}}","url":"/admission","summary":"Applications are invited for Classes I to IX. Forms close on 28 February."},
              {"title":"Half-yearly examination timetable published","date":"{{D(-6)}}","url":"/downloads","summary":"The timetable for Classes VI to XII is available under Downloads."},
              {"title":"Scholarship assessment registration","date":"{{D(-11)}}","url":"/admission","summary":"Merit and means scholarship assessments will be held on the first Saturday of March."},
              {"title":"Revised bus route 14","date":"{{D(-15)}}","url":"/contact","summary":"Route 14 now serves the new sector extension. Timings are unchanged."}
            ]}
            """,
            buttonText: "All notices", buttonLink: "/news");

        // ---------------------------------------------------------------- 13. Latest news
        Add(HomePageSectionKeys.LatestNews,
            "News", "Stories of learning, leadership and life on campus", null,
            $$"""
            {"items":[
              {"title":"Students take first place at the National Innovation Challenge","date":"{{D(-4)}}","url":"/news","summary":"A team from Class XI won for a low-cost water testing kit."},
              {"title":"A new chapter for the library","date":"{{D(-18)}}","url":"/news","summary":"Twelve thousand volumes, a reading room and a digital archive opened this month."},
              {"title":"Class XII results: 100% pass, 41 distinctions","date":"{{D(-46)}}","url":"/news","summary":"Our strongest board results to date."}
            ]}
            """,
            buttonText: "All news", buttonLink: "/news");

        // ---------------------------------------------------------------- 14. Events
        Add(HomePageSectionKeys.UpcomingEvents,
            "What's on", "Join us at our next campus experience", null,
            $$"""
            {"items":[
              {"title":"Admissions open day","date":"{{D(9)}}","url":"/events","location":"Main campus, Gate 2"},
              {"title":"Inter-house athletics final","date":"{{D(21)}}","url":"/events","location":"School ground"},
              {"title":"Annual Day {{DateTime.UtcNow.Year}}","date":"{{D(38)}}","url":"/events","location":"School auditorium"},
              {"title":"Founder's Day","date":"{{D(64)}}","url":"/events","location":"Quadrangle"}
            ]}
            """,
            buttonText: "Full calendar", buttonLink: "/events");

        // ---------------------------------------------------------------- 15. Gallery
        Add(HomePageSectionKeys.Gallery,
            "Campus gallery", "A glimpse of learning and life", null,
            $$"""
            {"hover":"zoom","items":[
              {"title":"Learning beyond classrooms","imageUrl":"{{s.Gallery[0]}}","alt":"Students on campus"},
              {"title":"Spaces that inspire","imageUrl":"{{s.Gallery[1]}}","alt":"The main building"},
              {"title":"A vibrant community","imageUrl":"{{s.Gallery[2]}}","alt":"A teacher working with students"},
              {"title":"Room to play","imageUrl":"{{s.Gallery[3]}}","alt":"The playing fields"},
              {"title":"Quiet for reading","imageUrl":"{{s.Gallery[4]}}","alt":"The library"},
              {"title":"Together","imageUrl":"{{s.Gallery[5]}}","alt":"Assembly"}
            ]}
            """,
            buttonText: "Full gallery", buttonLink: "/gallery");

        // ---------------------------------------------------------------- 16. Video
        Add(HomePageSectionKeys.Video,
            "Film", "See what makes our community special", null,
            $$"""
            {"caption":"A day at {{s.ShortName}}","items":[
              {"title":"Campus tour","videoUrl":"https://www.youtube.com/embed/ScMzIvxBSi4","posterUrl":"{{s.Gallery[1]}}"},
              {"title":"Annual Day highlights","videoUrl":"https://www.youtube.com/embed/aqz-KE-bpKQ","posterUrl":"{{s.Gallery[2]}}"}
            ]}
            """);

        // ---------------------------------------------------------------- 17. Testimonials
        Add(HomePageSectionKeys.Testimonials,
            "Voices from our community", "Students, parents and alumni", null,
            """
            {"items":[
              {"name":"Aarav Sharma","role":"Class of 2025","quote":"The teachers helped me find what I was capable of and then asked for more of it."},
              {"name":"Priya Mehta","role":"Parent, Class VII","quote":"They see my daughter. That is the whole of it, and it is not common."},
              {"name":"Dr. Ritu Kapoor","role":"Alumna, 1998","quote":"I learned to argue properly here. Twenty-five years later it is still the most useful thing I was taught."},
              {"name":"Imran Sheikh","role":"Parent, Class II","quote":"The office answers. After two other schools, I had stopped expecting that."}
            ]}
            """);

        // ---------------------------------------------------------------- 18. Achievements
        Add(HomePageSectionKeys.Achievements,
            "Achievements", "Celebrating effort, excellence and impact", null,
            $$"""
            {"hover":"glow","items":[
              {"title":"National School of Excellence","year":"{{DateTime.UtcNow.Year}}","description":"Recognised for academic innovation and student outcomes."},
              {"title":"Inter-school Sports Champions","year":"{{DateTime.UtcNow.Year}}","description":"Overall championship across athletics and team sports."},
              {"title":"Green Campus Award","year":"{{DateTime.UtcNow.Year - 1}}","description":"For rainwater harvesting and a zero-waste kitchen."},
              {"title":"Best Science Exhibition","year":"{{DateTime.UtcNow.Year - 1}}","description":"State-level first place for the senior physics project."}
            ]}
            """);

        // ---------------------------------------------------------------- 19. Admission CTA
        Add(HomePageSectionKeys.AdmissionCta,
            "Admissions", "Your journey starts here",
            "<p>Applications for the 2027–28 academic year are invited from families across the city.</p>",
            $$"""{"heading":"Your journey starts here","supportingText":"Applications are open for the 2027–28 academic year.","deadline":"{{D(96)}}","background":"waves"}""",
            buttonText: "Start an application", buttonLink: "/admission");

        // ---------------------------------------------------------------- 20. Brochure
        Add(HomePageSectionKeys.DownloadBrochure,
            "Prospectus", "Everything about the school in one document", null,
            """{"documentUrl":"/documents/prospectus.pdf","fileLabel":"2027–28 Prospectus","fileSize":"PDF · 4.2 MB"}""",
            buttonText: "Download prospectus", buttonLink: "/documents/prospectus.pdf");

        // ---------------------------------------------------------------- 21. Contact
        Add(HomePageSectionKeys.Contact,
            "Contact us", "We would love to hear from you", null,
            $$"""
            {"email":"{{s.Email}}","phone":"{{s.Phone}}","address":"{{s.Address}}","mapEmbedUrl":"https://maps.google.com/maps?q={{Uri.EscapeDataString(s.City)}}&t=&z=13&ie=UTF8&iwloc=&output=embed"}
            """,
            buttonText: "Send a message", buttonLink: "/contact");

        // ---------------------------------------------------------------- 22. Partners
        Add(HomePageSectionKeys.Partners,
            "Affiliations and partners", "Connected to a world of opportunity", null,
            """
            {"items":[
              {"name":"Central Board of Secondary Education","logoUrl":"","url":"#"},
              {"name":"Cambridge Assessment International","logoUrl":"","url":"#"},
              {"name":"Duke of Edinburgh's Award","logoUrl":"","url":"#"},
              {"name":"Round Square","logoUrl":"","url":"#"}
            ]}
            """);

        // ---------------------------------------------------------------- 23. Footer CTA
        Add(HomePageSectionKeys.FooterCta,
            "Come and see us", "Ready to discover your potential?",
            "<p>Visit the campus on a working day and meet the people who make the school what it is.</p>",
            """{"heading":"Ready to discover your potential?","supportingText":"Visit our campus and meet the people who make the school exceptional.","secondaryButton":"Download prospectus","backgroundColor":"#f5f4f1"}""",
            buttonText: "Book a campus visit", buttonLink: "/contact");

        // ---------------------------------------------------------------- 24. Timings
        Add(HomePageSectionKeys.Timings,
            "School timings", "Summer and winter, by wing",
            "<p>Gates open fifteen minutes before the first bell. The office is open through the working day.</p>",
            """
            {"intro":"Summer timings apply from 1 April; winter timings from 1 November.","firstTerm":"Summer (Apr–Oct)","secondTerm":"Winter (Nov–Mar)","items":[
              {"wing":"Pre-primary","summer":"8:30 – 11:30","winter":"9:00 – 12:00"},
              {"wing":"Primary (I–V)","summer":"7:45 – 13:15","winter":"8:30 – 14:00"},
              {"wing":"Middle (VI–VIII)","summer":"7:30 – 13:45","winter":"8:15 – 14:30"},
              {"wing":"Senior (IX–XII)","summer":"7:30 – 14:30","winter":"8:15 – 15:15"},
              {"wing":"Office","summer":"8:00 – 15:00","winter":"8:30 – 15:30"}
            ]}
            """);

        // ---------------------------------------------------------------- 25. Crest
        Add(HomePageSectionKeys.Crest,
            "Our crest", s.Motto, null,
            """
            {"items":[
              {"symbol":"The open book","meaning":"Learning that is shared rather than hoarded."},
              {"symbol":"The lamp","meaning":"Knowledge carried into places that need it."},
              {"symbol":"The banyan","meaning":"Shelter, and roots that hold in a storm."},
              {"symbol":"The star","meaning":"An ambition that is never quite reached, and is the better for it."}
            ]}
            """,
            imageUrl: s.Crest);

        // ---------------------------------------------------------------- 26. Alumni
        Add(HomePageSectionKeys.Alumni,
            "Notable alumni", "Where our students went", null,
            """
            {"items":[
              {"name":"Dr. Ritu Kapoor","batch":"1998","role":"Cardiologist, AIIMS","imageUrl":"https://images.unsplash.com/photo-1559839734-2b71ea197ec2?auto=format&fit=crop&w=400&q=80"},
              {"name":"Arjun Nair","batch":"2004","role":"Test cricketer","imageUrl":"https://images.unsplash.com/photo-1500648767791-00dcc994a43e?auto=format&fit=crop&w=400&q=80"},
              {"name":"Sana Qureshi","batch":"2009","role":"Documentary film-maker","imageUrl":"https://images.unsplash.com/photo-1494790108377-be9c29b29330?auto=format&fit=crop&w=400&q=80"},
              {"name":"Karthik Iyer","batch":"2012","role":"Founder, Meridian Robotics","imageUrl":"https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?auto=format&fit=crop&w=400&q=80"}
            ]}
            """);

        // ---------------------------------------------------------------- 27. Staff list
        Add(HomePageSectionKeys.StaffList,
            "Our staff", "Every teacher, with qualifications", null,
            $$"""
            {"intro":"The full teaching staff, as filed with the board.","items":[
              {"name":"{{s.PrincipalName}}","designation":"Principal","qualification":"Ph.D, M.Ed"},
              {"name":"{{s.DirectorName}}","designation":"Director, Academics","qualification":"M.A, M.Phil"},
              {"name":"Vikram Mehta","designation":"Head of Science","qualification":"M.Sc, B.Ed"},
              {"name":"Sunita Nair","designation":"Head of English","qualification":"M.A, B.Ed"},
              {"name":"Priya Raghavan","designation":"Head of Mathematics","qualification":"M.Sc, B.Ed"},
              {"name":"Imtiaz Ali","designation":"Head of Social Science","qualification":"M.A, B.Ed"},
              {"name":"Rekha Shetty","designation":"Senior Teacher, Biology","qualification":"M.Sc, B.Ed"},
              {"name":"Daniel Fernandes","designation":"Director of Music","qualification":"M.Mus"},
              {"name":"Anu George","designation":"Counsellor","qualification":"M.A Psychology, Dip. Counselling"},
              {"name":"Rahul Desai","designation":"Admissions Officer","qualification":"MBA"}
            ]}
            """);

        // ---------------------------------------------------------------- 28. Facilities
        Add(HomePageSectionKeys.Facilities,
            "Facilities", "Named, and described", null,
            $$"""
            {"intro":"Every space on campus, and what it is actually used for.","items":[
              {"title":"Smart classrooms","description":"Forty-two classrooms, each with a projector and a document camera.","imageUrl":"https://images.unsplash.com/photo-1580582932707-520aed937b7b?auto=format&fit=crop&w=900&q=80"},
              {"title":"Science laboratories","description":"Separate Physics, Chemistry and Biology laboratories, open until 17:00.","imageUrl":"https://images.unsplash.com/photo-1532094349884-543bc11b234d?auto=format&fit=crop&w=900&q=80"},
              {"title":"Library","description":"Twelve thousand volumes, a reading room and a digital archive.","imageUrl":"https://images.unsplash.com/photo-1521587760476-6c12a4b040da?auto=format&fit=crop&w=900&q=80"},
              {"title":"Sports complex","description":"A 400m track, cricket ground, astro-turf and an indoor hall.","imageUrl":"https://images.unsplash.com/photo-1571260899304-425eee4c7efc?auto=format&fit=crop&w=900&q=80"},
              {"title":"Infirmary","description":"A full-time nurse, two beds and a visiting doctor three days a week.","imageUrl":"https://images.unsplash.com/photo-1519494026892-80bbd2d6fd0d?auto=format&fit=crop&w=900&q=80"},
              {"title":"Transport","description":"Fourteen routes with GPS tracking and an attendant on every bus.","imageUrl":"https://images.unsplash.com/photo-1557223562-6c77ef16210f?auto=format&fit=crop&w=900&q=80"}
            ]}
            """,
            buttonText: "Facilities page", buttonLink: "/facilities");

        // ---------------------------------------------------------------- 29. Founder
        Add(HomePageSectionKeys.Founder,
            "Founder and history", "How the school began",
            $"<p>{s.FounderName} opened {s.ShortName} in {s.Founded} with four classrooms and thirty-one children. The school has moved twice and grown forty-fold, and has never taken a fee it did not publish.</p>",
            $$"""
            {"name":"{{s.FounderName}}","lifespan":"{{s.FounderLife}}","items":[
              {"year":"{{s.Founded}}","title":"The school opens","description":"Four classrooms, thirty-one children and two teachers."},
              {"year":"1961","title":"The senior school","description":"Classes IX and X added; the first board cohort sits in 1963."},
              {"year":"1987","title":"The present campus","description":"Eleven acres, purpose-built, with the quadrangle at its centre."},
              {"year":"2006","title":"Science block","description":"Three laboratories and a lecture theatre."},
              {"year":"{{DateTime.UtcNow.Year - 1}}","title":"Library and archive","description":"Twelve thousand volumes and the school's own records, catalogued."}
            ]}
            """,
            imageUrl: "https://images.unsplash.com/photo-1544717305-2782549b5136?auto=format&fit=crop&w=600&q=80");

        // ---------------------------------------------------------------- 30. Downloads
        Add(HomePageSectionKeys.Downloads,
            "Downloads", "Circulars, timetables and forms",
            "<p>Everything a parent is asked for, in one place.</p>",
            """
            {"items":[
              {"title":"Admission form 2027–28","format":"PDF","size":"320 KB","fileUrl":"/documents/admission-form.pdf"},
              {"title":"Fee structure 2027–28","format":"PDF","size":"180 KB","fileUrl":"/documents/fee-structure.pdf"},
              {"title":"Academic calendar","format":"PDF","size":"240 KB","fileUrl":"/documents/academic-calendar.pdf"},
              {"title":"Half-yearly examination timetable","format":"PDF","size":"150 KB","fileUrl":"/documents/exam-timetable.pdf"},
              {"title":"Bus routes and stops","format":"XLSX","size":"64 KB","fileUrl":"/documents/bus-routes.xlsx"},
              {"title":"Transfer certificate request","format":"DOCX","size":"28 KB","fileUrl":"/documents/tc-request.docx"}
            ]}
            """,
            buttonText: "All documents", buttonLink: "/mandatory-disclosure");

        return list;
    }
}
