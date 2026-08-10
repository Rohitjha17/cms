using Cms.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cms.Infrastructure.Persistence.Seed;

/// <summary>
/// Demo faculty, notices, events and site settings so the People, News, Events and Settings
/// screens — and the public pages they feed — have something real to show.
/// Idempotent: an entry is only created when its key is absent.
/// </summary>
public static class SchoolContentSeed
{
    public static async Task EnsureAsync(
        ApplicationDbContext db,
        Guid tenantId,
        Guid siteId,
        CancellationToken cancellationToken = default)
    {
        var existing = await db.ContentEntries.IgnoreQueryFilters()
            .Where(x => x.TenantId == tenantId && x.SiteId == siteId)
            .Select(x => x.ContentType + "::" + x.Key)
            .ToListAsync(cancellationToken);
        var present = existing.ToHashSet(StringComparer.OrdinalIgnoreCase);

        var now = DateTime.UtcNow;
        var seeds = new List<ContentEntry>
        {
            Entry(tenantId, siteId, "person", "anita-rao", "Dr. Anita Rao",
                summary: "Leading the school since 2016.",
                body: "<p>Dr. Rao has spent twenty-two years in school leadership and chairs the academic council.</p>",
                json: """{"designation":"Principal","department":"Administration","category":"Leadership","qualification":"Ph.D, M.Ed","experienceYears":22,"email":"principal@demo.local","phone":"+91 98765 43210"}""",
                order: 0),

            Entry(tenantId, siteId, "person", "vikram-mehta", "Vikram Mehta",
                summary: "Head of the science faculty.",
                body: "<p>Leads the physics department and the school's robotics programme.</p>",
                json: """{"designation":"Head of Science","department":"Science","category":"Teaching","qualification":"M.Sc, B.Ed","experienceYears":14,"email":"science@demo.local"}""",
                order: 1),

            Entry(tenantId, siteId, "person", "sunita-nair", "Sunita Nair",
                summary: "English literature and debate.",
                json: """{"designation":"Senior Teacher","department":"Languages","category":"Teaching","qualification":"M.A, B.Ed","experienceYears":9}""",
                order: 2),

            Entry(tenantId, siteId, "person", "rahul-desai", "Rahul Desai",
                summary: "Admissions and front office.",
                json: """{"designation":"Admissions Officer","department":"Administration","category":"Administration","email":"admissions@demo.local","phone":"+91 98765 43211"}""",
                order: 3),

            Entry(tenantId, siteId, "news", "admissions-open-2026", "Admissions open for 2026–27",
                summary: "Applications for classes I to IX are now open. Forms close on 28 February.",
                body: "<p>Application forms are available at the school office and online. Entrance interactions begin in March.</p>",
                json: """{"category":"Notice","isFeatured":true}""",
                publishDate: now.AddDays(-3), order: 0),

            Entry(tenantId, siteId, "news", "annual-results-2025", "Class XII results: 100% pass, 41 distinctions",
                summary: "Our senior cohort recorded the school's strongest board results to date.",
                body: "<p>Congratulations to the class of 2025 and to the teachers who guided them.</p>",
                json: """{"category":"Achievement","isFeatured":false}""",
                publishDate: now.AddDays(-21), order: 1),

            Entry(tenantId, siteId, "news", "winter-break-circular", "Winter break circular",
                summary: "The school closes from 24 December and reopens on 2 January.",
                json: """{"category":"Circular","isFeatured":false}""",
                publishDate: now.AddDays(-40), order: 2),

            Entry(tenantId, siteId, "event", "annual-day-2026", "Annual Day 2026",
                summary: "An evening of music, drama and dance from every grade.",
                body: "<p>Families are warmly invited. Seating opens at 17:00.</p>",
                json: $$"""{"endsOn":"{{now.AddDays(30).AddHours(3):O}}","venue":"School auditorium"}""",
                publishDate: now.AddDays(30), order: 0),

            Entry(tenantId, siteId, "event", "open-day-admissions", "Admissions open day",
                summary: "Tour the campus, meet the faculty and ask us anything.",
                json: $$"""{"endsOn":"{{now.AddDays(12).AddHours(4):O}}","venue":"Main campus, Gate 2"}""",
                publishDate: now.AddDays(12), order: 1),

            Entry(tenantId, siteId, "event", "sports-meet-2025", "Inter-house sports meet",
                summary: "Track and field finals across all four houses.",
                json: $$"""{"endsOn":"{{now.AddDays(-25).AddHours(6):O}}","venue":"School ground"}""",
                publishDate: now.AddDays(-25), order: 2),

            Entry(tenantId, siteId, "department", "science", "Science",
                summary: "Physics, Chemistry and Biology with fully equipped laboratories.",
                json: """{"headOfDepartment":"Vikram Mehta","email":"science@demo.local","programmes":["Physics","Chemistry","Biology","Computer Science"]}""",
                order: 0),

            Entry(tenantId, siteId, "department", "languages", "Languages",
                summary: "English, Hindi and Sanskrit, with a active debate and literary society.",
                json: """{"headOfDepartment":"Sunita Nair","programmes":["English","Hindi","Sanskrit"]}""",
                order: 1),

            Entry(tenantId, siteId, "department", "commerce", "Commerce",
                summary: "Accountancy, Business Studies and Economics for senior grades.",
                json: """{"headOfDepartment":"Rahul Desai","programmes":["Accountancy","Business Studies","Economics"]}""",
                order: 2),

            Entry(tenantId, siteId, "setting", "site", "Site settings",
                json: """
                {
                  "noticeTicker":"Admissions for 2026–27 are now open · Forms close 28 February",
                  "admissionStatus":"Open",
                  "admissionsEmail":"admissions@demo.local",
                  "admissionsPhone":"+91 98765 43211",
                  "officeHours":"Monday to Saturday, 9:00–15:00",
                  "whatsAppNumber":"+919876543211",
                  "facebook":"https://facebook.com/demoacademy",
                  "instagram":"https://instagram.com/demoacademy",
                  "youTube":"https://youtube.com/@demoacademy"
                }
                """)
        };

        var added = false;
        foreach (var seed in seeds.Where(x => !present.Contains($"{x.ContentType}::{x.Key}")))
        {
            db.ContentEntries.Add(seed);
            added = true;
        }

        if (added)
        {
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    private static ContentEntry Entry(
        Guid tenantId,
        Guid siteId,
        string type,
        string key,
        string title,
        string? summary = null,
        string? body = null,
        string? json = null,
        DateTime? publishDate = null,
        int order = 0) => new()
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            SiteId = siteId,
            ContentType = type,
            Key = key,
            Title = title,
            Summary = summary,
            Body = body,
            JsonData = json,
            PublishDate = publishDate,
            DisplayOrder = order,
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "seed"
        };
}
