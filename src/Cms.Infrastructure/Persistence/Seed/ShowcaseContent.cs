using System.Globalization;
using Cms.Domain.Entities;

namespace Cms.Infrastructure.Persistence.Seed;

/// <summary>
/// The People, News, Events, Departments and Settings records behind one showcase website.
/// These feed both the console screens and the public pages, so a demo with empty ones shows
/// a console with nothing in it.
/// </summary>
internal static class ShowcaseContent
{
    internal static List<ContentEntry> Build(Guid tenantId, ShowcaseSpec s)
    {
        var now = DateTime.UtcNow;
        var order = 0;
        var list = new List<ContentEntry>();

        void Add(string type, string key, string title, string? summary = null, string? body = null,
            string? json = null, string? image = null, DateTime? publish = null)
            => list.Add(new ContentEntry
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                SiteId = s.SiteId,
                ContentType = type,
                Key = key,
                Title = title,
                Summary = summary,
                Body = body,
                JsonData = json,
                ImageUrl = image,
                PublishDate = publish,
                DisplayOrder = order++,
                IsActive = true,
                CreatedDate = now,
                CreatedBy = "showcase"
            });

        // ------------------------------------------------------------------ People
        order = 0;
        Add("person", "principal", s.PrincipalName,
            "Leading the school since 2016.",
            "<p>Twenty-two years in school leadership, and chair of the academic council.</p>",
            """{"designation":"Principal","department":"Administration","category":"Leadership","qualification":"Ph.D, M.Ed","experienceYears":22,"email":"principal@demo.local","phone":"+91 98765 43210"}""",
            "https://images.unsplash.com/photo-1544717305-2782549b5136?auto=format&fit=crop&w=600&q=80");
        Add("person", "director", s.DirectorName, "Curriculum, staffing and standards.",
            "<p>Oversees the academic programme from pre-primary to Class XII.</p>",
            """{"designation":"Director, Academics","department":"Administration","category":"Leadership","qualification":"M.A, M.Phil","experienceYears":18,"email":"academics@demo.local"}""",
            "https://images.unsplash.com/photo-1573497019940-1c28c88b4f3e?auto=format&fit=crop&w=600&q=80");
        Add("person", "vikram-mehta", "Vikram Mehta", "Head of the science faculty.",
            "<p>Leads the physics department and the robotics programme.</p>",
            """{"designation":"Head of Science","department":"Science","category":"Teaching","qualification":"M.Sc, B.Ed","experienceYears":14,"email":"science@demo.local"}""",
            "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?auto=format&fit=crop&w=600&q=80");
        Add("person", "sunita-nair", "Sunita Nair", "English literature and debate.", null,
            """{"designation":"Head of English","department":"Languages","category":"Teaching","qualification":"M.A, B.Ed","experienceYears":9}""",
            "https://images.unsplash.com/photo-1494790108377-be9c29b29330?auto=format&fit=crop&w=600&q=80");
        Add("person", "priya-raghavan", "Priya Raghavan", "Mathematics, and the olympiad group.", null,
            """{"designation":"Head of Mathematics","department":"Mathematics","category":"Teaching","qualification":"M.Sc, B.Ed","experienceYears":12}""",
            "https://images.unsplash.com/photo-1559839734-2b71ea197ec2?auto=format&fit=crop&w=600&q=80");
        Add("person", "anu-george", "Anu George", "School counsellor.", null,
            """{"designation":"Counsellor","department":"Wellbeing","category":"Support","qualification":"M.A Psychology, Dip. Counselling","experienceYears":7,"email":"counsellor@demo.local"}""",
            "https://images.unsplash.com/photo-1580489944761-15a19d654956?auto=format&fit=crop&w=600&q=80");
        Add("person", "rahul-desai", "Rahul Desai", "Admissions and front office.", null,
            """{"designation":"Admissions Officer","department":"Administration","category":"Administration","email":"admissions@demo.local","phone":"+91 98765 43211"}""",
            "https://images.unsplash.com/photo-1519085360753-af0119f7cbe7?auto=format&fit=crop&w=600&q=80");

        // -------------------------------------------------------------------- News
        order = 0;
        Add("news", "admissions-open", "Admissions open for 2027–28",
            "Applications for Classes I to IX are now open. Forms close on 28 February.",
            "<p>Application forms are available at the school office and online. Assessments begin in March.</p>",
            """{"category":"Notice","isFeatured":true}""",
            s.Gallery[2], now.AddDays(-2));
        Add("news", "innovation-challenge", "First place at the National Innovation Challenge",
            "A team from Class XI won for a low-cost water testing kit.",
            "<p>Four students spent a year on a kit that tests drinking water for three common contaminants at under ₹200 a unit.</p>",
            """{"category":"Achievement","isFeatured":true}""",
            s.Gallery[1], now.AddDays(-4));
        Add("news", "library-opens", "A new chapter for the library",
            "Twelve thousand volumes, a reading room and a digital archive.",
            "<p>The new library opened this month after eighteen months of building.</p>",
            """{"category":"Achievement","isFeatured":false}""",
            s.Gallery[4], now.AddDays(-18));
        Add("news", "exam-timetable", "Half-yearly examination timetable published",
            "The timetable for Classes VI to XII is available under Downloads.", null,
            """{"category":"Circular","isFeatured":false}""",
            null, now.AddDays(-6));
        Add("news", "bus-route-14", "Revised bus route 14",
            "Route 14 now serves the new sector extension. Timings are unchanged.", null,
            """{"category":"Circular","isFeatured":false}""",
            null, now.AddDays(-15));
        Add("news", "board-results", "Class XII results: 100% pass, 41 distinctions",
            "Our senior cohort recorded the school's strongest board results to date.",
            "<p>Congratulations to the class and to the teachers who guided them.</p>",
            """{"category":"Achievement","isFeatured":false}""",
            s.Gallery[5], now.AddDays(-46));

        // ------------------------------------------------------------------ Events
        order = 0;
        Add("event", "open-day", "Admissions open day",
            "Tour the campus, meet the faculty and ask us anything.", null,
            $$"""{"endsOn":"{{now.AddDays(9).AddHours(4):O}}","venue":"Main campus, Gate 2"}""",
            s.Gallery[0], now.AddDays(9));
        Add("event", "athletics-final", "Inter-house athletics final",
            "Track and field finals across all four houses.", null,
            $$"""{"endsOn":"{{now.AddDays(21).AddHours(6):O}}","venue":"School ground"}""",
            s.Gallery[3], now.AddDays(21));
        Add("event", "annual-day", $"Annual Day {now.Year}",
            "An evening of music, drama and dance from every grade.",
            "<p>Families are warmly invited. Seating opens at 17:00.</p>",
            $$"""{"endsOn":"{{now.AddDays(38).AddHours(3):O}}","venue":"School auditorium"}""",
            s.Gallery[2], now.AddDays(38));
        Add("event", "founders-day", "Founder's Day",
            "Our annual whole-school celebration.", null,
            $$"""{"endsOn":"{{now.AddDays(64).AddHours(5):O}}","venue":"Quadrangle"}""",
            s.Gallery[1], now.AddDays(64));
        Add("event", "sports-meet-past", "Inter-house sports meet",
            "Held last month across all four houses.", null,
            $$"""{"endsOn":"{{now.AddDays(-25).AddHours(6):O}}","venue":"School ground"}""",
            null, now.AddDays(-25));

        // ------------------------------------------------------------- Departments
        order = 0;
        Add("department", "science", "Science",
            "Physics, Chemistry and Biology with fully equipped laboratories.", null,
            """{"headOfDepartment":"Vikram Mehta","email":"science@demo.local","programmes":["Physics","Chemistry","Biology","Computer Science"]}""");
        Add("department", "mathematics", "Mathematics",
            "Setted from Class VI, with an olympiad group and a support group of equal standing.", null,
            """{"headOfDepartment":"Priya Raghavan","email":"maths@demo.local","programmes":["Mathematics","Applied Mathematics","Statistics"]}""");
        Add("department", "languages", "Languages",
            "English, Hindi and Sanskrit, with an active debating and literary society.", null,
            """{"headOfDepartment":"Sunita Nair","programmes":["English","Hindi","Sanskrit","French"]}""");
        Add("department", "humanities", "Humanities",
            "History, Political Science, Psychology and Geography, with fieldwork every year.", null,
            """{"headOfDepartment":"Imtiaz Ali","programmes":["History","Political Science","Psychology","Geography"]}""");
        Add("department", "commerce", "Commerce",
            "Accountancy, Business Studies and Economics for senior grades.", null,
            """{"headOfDepartment":"Rahul Desai","programmes":["Accountancy","Business Studies","Economics"]}""");
        Add("department", "arts", "Performing & Visual Arts",
            "Music, theatre, dance and studio art as timetabled subjects.", null,
            """{"headOfDepartment":"Daniel Fernandes","programmes":["Music","Theatre","Dance","Studio Art"]}""");

        // ---------------------------------------------------------------- Settings
        Add("setting", "site", "Site settings", json: s.Settings);

        return list;
    }
}
