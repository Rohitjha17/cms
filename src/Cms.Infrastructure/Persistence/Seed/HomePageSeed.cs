using Cms.Domain.Constants;
using Cms.Domain.Entities;
using Cms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Cms.Infrastructure.Persistence.Seed;

public static class HomePageSeed
{
    public static async Task EnsureSectionsAsync(ApplicationDbContext db, Guid tenantId, Guid siteId, CancellationToken cancellationToken = default)
    {
        var existingSections = await db.HomePageSections
            .IgnoreQueryFilters()
            .Where(x => x.TenantId == tenantId && x.SiteId == siteId)
            .ToListAsync(cancellationToken);

        var existing = existingSections.Select(x => x.SectionKey).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var toAdd = new List<HomePageSection>();

        foreach (var (key, displayName, order) in HomePageSectionKeys.All)
        {
            if (existing.Contains(key))
            {
                continue;
            }

            toAdd.Add(new HomePageSection
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                SiteId = siteId,
                SectionKey = key,
                Title = displayName,
                DisplayOrder = order,
                IsActive = true,
                JsonData = DefaultJson(key),
                SubTitle = DefaultSubtitle(key),
                Description = DefaultDescription(key),
                ButtonText = DefaultButtonText(key),
                ButtonLink = DefaultButtonLink(key),
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "seed"
            });
        }

        // Enrich untouched demo seed rows without ever overwriting editor changes.
        foreach (var section in existingSections.Where(x =>
                     x.CreatedBy == "seed" && x.UpdatedDate is null))
        {
            section.IsActive = true;
            section.SubTitle ??= DefaultSubtitle(section.SectionKey);
            section.Description ??= DefaultDescription(section.SectionKey);
            section.ButtonText ??= DefaultButtonText(section.SectionKey);
            section.ButtonLink ??= DefaultButtonLink(section.SectionKey);
            section.JsonData ??= DefaultJson(section.SectionKey);
        }

        if (toAdd.Count > 0)
        {
            await db.HomePageSections.AddRangeAsync(toAdd, cancellationToken);
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static string? DefaultJson(string key) => key switch
    {
        HomePageSectionKeys.Hero => """{"heading":"Welcome to Demo Academy","primaryButton":"Apply Now","secondaryButton":"Contact Us","videoUrl":""}""",
        HomePageSectionKeys.Statistics => """{"students":1500,"teachers":80,"placements":500,"years":20}""",
        HomePageSectionKeys.Courses => """{"items":[{"title":"Science & Technology","description":"Inquiry-led learning for tomorrow's innovators.","url":"/departments"},{"title":"Business & Leadership","description":"Real-world skills with an entrepreneurial mindset.","url":"/departments"},{"title":"Arts & Humanities","description":"Creative expression grounded in critical thinking.","url":"/departments"}]}""",
        HomePageSectionKeys.Departments => """{"items":[{"title":"Sciences","description":"Discover, experiment and innovate."},{"title":"Humanities","description":"Understand people, culture and society."},{"title":"Commerce","description":"Build the skills to lead with purpose."}]}""",
        HomePageSectionKeys.WhyChooseUs => """{"intro":"A learning environment designed around every student's potential.","columns":3,"items":[{"title":"Future-ready learning","description":"A modern curriculum connected to the world beyond the classroom."},{"title":"Exceptional educators","description":"Mentors who know, challenge and champion every learner."},{"title":"A caring community","description":"A safe, inclusive culture where confidence can flourish."}]}""",
        HomePageSectionKeys.Announcements => """{"items":[{"title":"Admissions open for the 2026–27 academic year","date":"2026-08-01","url":"/admission","summary":"Applications are now invited across all grade levels."},{"title":"Scholarship assessment registrations","date":"2026-08-15","url":"/admission","summary":"Merit scholarship assessment registrations are now live."}]}""",
        HomePageSectionKeys.LatestNews => """{"items":[{"title":"Students shine at the National Innovation Challenge","date":"2026-07-18","url":"/news/innovation-challenge"},{"title":"A new chapter for our campus library","date":"2026-07-10","url":"/news/library"}]}""",
        HomePageSectionKeys.UpcomingEvents => """{"items":[{"title":"Open House 2026","date":"2026-08-22","url":"/events/open-house","location":"Main Campus"},{"title":"Founders Day Celebration","date":"2026-09-05","url":"/events/founders-day","location":"Central Auditorium"}]}""",
        HomePageSectionKeys.Gallery => """{"items":[{"title":"Learning beyond classrooms","imageUrl":"https://images.unsplash.com/photo-1523050854058-8df90110c9f1?auto=format&fit=crop&w=900&q=80","alt":"Students on campus"},{"title":"Spaces that inspire","imageUrl":"https://images.unsplash.com/photo-1562774053-701939374585?auto=format&fit=crop&w=900&q=80","alt":"University building"},{"title":"A vibrant community","imageUrl":"https://images.unsplash.com/photo-1577896851231-70ef18881754?auto=format&fit=crop&w=900&q=80","alt":"Teacher working with students"}]}""",
        HomePageSectionKeys.Video => """{"videoUrl":"","posterUrl":"https://images.unsplash.com/photo-1564981797816-1043664bf78d?auto=format&fit=crop&w=1400&q=80","caption":"Discover life at Demo Academy"}""",
        HomePageSectionKeys.Testimonials => """{"items":[{"name":"Aarav Sharma","role":"Class of 2025","quote":"The teachers helped me discover what I was capable of and gave me the confidence to aim higher."},{"name":"Priya Mehta","role":"Parent","quote":"This is a community that sees every child, celebrates their strengths and supports their growth."}]}""",
        HomePageSectionKeys.Achievements => """{"items":[{"title":"National School of Excellence","year":"2026","description":"Recognized for academic innovation and student outcomes."},{"title":"Inter-school Sports Champions","year":"2026","description":"Overall championship across athletics and team sports."}]}""",
        HomePageSectionKeys.AdmissionCta => """{"heading":"Your journey starts here","supportingText":"Applications are open for the 2026–27 academic year.","deadline":"2026-12-15"}""",
        HomePageSectionKeys.DownloadBrochure => """{"documentUrl":"/documents/prospectus.pdf","fileLabel":"2026–27 Prospectus","fileSize":"PDF · 4.2 MB"}""",
        HomePageSectionKeys.Partners => """{"items":[{"name":"Cambridge Learning","logoUrl":"","url":"#"},{"name":"Global Schools Network","logoUrl":"","url":"#"},{"name":"Future Skills Alliance","logoUrl":"","url":"#"}]}""",
        HomePageSectionKeys.Contact => """{"email":"hello@demoacademy.edu","phone":"+91 98765 43210","address":"Knowledge Park, New Delhi, India","mapEmbedUrl":""}""",
        HomePageSectionKeys.FooterCta => """{"heading":"Ready to discover your potential?","supportingText":"Visit our campus and meet the people who make Demo Academy exceptional.","secondaryButton":"Download prospectus"}""",
        HomePageSectionKeys.Principal => """{"personName":"Dr. Ananya Rao","designation":"Principal","quote":"Education should help every learner find their voice, purpose and courage."}""",
        HomePageSectionKeys.Chairman => """{"personName":"Rajiv Mehra","designation":"Chairman","quote":"We prepare young people not only for examinations, but for a life of contribution."}""",
        HomePageSectionKeys.Director => """{"personName":"","designation":"Director","quote":""}""",
        HomePageSectionKeys.Manager => """{"personName":"","designation":"Manager","quote":""}""",
        _ => """{}"""
    };

    private static string? DefaultSubtitle(string key) => key switch
    {
        HomePageSectionKeys.Hero => "A place where curiosity becomes confidence and every learner is known.",
        HomePageSectionKeys.Welcome => "Welcome to a community built around possibility.",
        HomePageSectionKeys.About => "Rooted in values. Ready for the future.",
        HomePageSectionKeys.Principal => "A message from our Principal",
        HomePageSectionKeys.Chairman => "A message from our Chairman",
        HomePageSectionKeys.Director => "A message from our Director",
        HomePageSectionKeys.Manager => "A message from our Manager",
        HomePageSectionKeys.Courses => "Pathways designed for ambition, curiosity and impact.",
        HomePageSectionKeys.Departments => "Deep expertise. Connected learning.",
        HomePageSectionKeys.WhyChooseUs => "An education that goes beyond achievement.",
        HomePageSectionKeys.Announcements => "Important updates from across our community.",
        HomePageSectionKeys.LatestNews => "Stories of learning, leadership and life on campus.",
        HomePageSectionKeys.UpcomingEvents => "Join us at our next campus experience.",
        HomePageSectionKeys.Gallery => "A glimpse of learning and life at Demo Academy.",
        HomePageSectionKeys.Video => "See what makes our community special.",
        HomePageSectionKeys.Testimonials => "Voices from our students and families.",
        HomePageSectionKeys.Achievements => "Celebrating effort, excellence and impact.",
        HomePageSectionKeys.AdmissionCta => "Applications for 2026–27 are now open.",
        HomePageSectionKeys.Contact => "We would love to hear from you.",
        HomePageSectionKeys.Partners => "Connected to a world of opportunity.",
        _ => null
    };

    private static string? DefaultDescription(string key) => key switch
    {
        HomePageSectionKeys.Welcome => "<p>At Demo Academy, learning is personal, purposeful and connected to the world. We create the conditions for every student to think deeply, act with empathy and grow with confidence.</p>",
        HomePageSectionKeys.About => "<p>For more than two decades, our institution has combined enduring values with a progressive approach to education. Our students learn to ask better questions, take meaningful action and contribute with character.</p>",
        HomePageSectionKeys.Principal => "<p>Our promise is simple: every learner will be challenged, supported and inspired. Together, we build the knowledge, confidence and humanity young people need to thrive.</p>",
        HomePageSectionKeys.Chairman => "<p>Education has the power to transform both individual lives and entire communities. We remain committed to creating opportunities that help every student become a thoughtful, capable citizen.</p>",
        HomePageSectionKeys.AdmissionCta => "<p>Take the first step toward an education shaped around your child’s potential.</p>",
        HomePageSectionKeys.Contact => "<p>Talk to our admissions team, plan a campus visit or ask us anything about life at Demo Academy.</p>",
        HomePageSectionKeys.FooterCta => "<p>Meet our educators, explore our spaces and imagine what your journey could become.</p>",
        _ => null
    };

    private static string? DefaultButtonText(string key) => key switch
    {
        HomePageSectionKeys.Hero => "Explore admissions",
        HomePageSectionKeys.About => "Discover our story",
        HomePageSectionKeys.Courses => "Explore all courses",
        HomePageSectionKeys.Gallery => "View gallery",
        HomePageSectionKeys.AdmissionCta => "Start your application",
        HomePageSectionKeys.DownloadBrochure => "Download prospectus",
        HomePageSectionKeys.Contact => "Contact admissions",
        HomePageSectionKeys.FooterCta => "Book a campus visit",
        _ => null
    };

    private static string? DefaultButtonLink(string key) => key switch
    {
        HomePageSectionKeys.Hero or HomePageSectionKeys.AdmissionCta => "/admission",
        HomePageSectionKeys.About => "/about",
        HomePageSectionKeys.Courses => "/departments",
        HomePageSectionKeys.Gallery => "/gallery",
        HomePageSectionKeys.DownloadBrochure => "/admission",
        HomePageSectionKeys.Contact => "/contact",
        HomePageSectionKeys.FooterCta => "/contact",
        _ => null
    };
}
