using Cms.Domain.Constants;
using Cms.Domain.Entities;
using Cms.Domain.Enums;
using Cms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Cms.Infrastructure.Persistence.Seed;

public static class PageTemplateSeed
{
    public static async Task EnsureAsync(ApplicationDbContext db, CancellationToken cancellationToken = default)
    {
        var definitions = BuildDefinitions();
        var existing = await db.PageTemplates.ToDictionaryAsync(x => x.TemplateKey, cancellationToken);

        foreach (var definition in definitions)
        {
            if (existing.TryGetValue(definition.TemplateKey, out var current))
            {
                if (current.UpdatedDate is null && current.CreatedBy == "seed")
                {
                    current.Name = definition.Name;
                    current.Description = definition.Description;
                    current.PageType = definition.PageType;
                    current.DefaultSlug = definition.DefaultSlug;
                    current.DefaultTitle = definition.DefaultTitle;
                    current.DefaultContent = definition.DefaultContent;
                    current.DefaultJsonData = definition.DefaultJsonData;
                    current.IsStarter = definition.IsStarter;
                    current.IsActive = true;
                    current.DisplayOrder = definition.DisplayOrder;
                }

                continue;
            }

            db.PageTemplates.Add(definition);
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static List<PageTemplate> BuildDefinitions() =>
    [
        Template(PageTemplateKeys.About, "About", PageType.About, "about", 10,
            "School/college overview, mission, vision and history.",
            "<p>Welcome to our institution. Edit this page to share your story, values and academic philosophy.</p>",
            """{"mission":"To nurture confident, compassionate learners.","vision":"A community of excellence and character.","history":"Founded with a commitment to quality education."}"""),
        Template(PageTemplateKeys.Admission, "Admission", PageType.Admission, "admission", 20,
            "Admission process, eligibility, documents and timelines.",
            "<p>Admissions are open. Update steps, eligibility and required documents for your institution.</p>",
            """{"eligibility":"Students seeking admission for the upcoming academic year.","processSteps":[{"title":"Enquiry","description":"Submit an enquiry or visit campus."},{"title":"Application","description":"Complete the admission form and attach documents."},{"title":"Interaction","description":"Attend counselling / interaction as scheduled."},{"title":"Confirmation","description":"Confirm seat and complete fee formalities."}],"documents":["Birth certificate","Previous marksheet","Transfer certificate","Passport photo"]}"""),
        Template(PageTemplateKeys.Facilities, "Facilities", PageType.Facilities, "facilities", 30,
            "Campus facilities with photos and descriptions.",
            "<p>Explore the facilities that support learning, sports and student life.</p>",
            """{"items":[{"title":"Smart Classrooms","description":"Technology-enabled learning spaces.","imageUrl":"https://images.unsplash.com/photo-1580582932707-520aed937b7b?auto=format&fit=crop&w=900&q=80"},{"title":"Science Laboratories","description":"Hands-on discovery and experimentation.","imageUrl":"https://images.unsplash.com/photo-1532094349884-543bc11b234d?auto=format&fit=crop&w=900&q=80"},{"title":"Library","description":"A quiet space for research and reading.","imageUrl":"https://images.unsplash.com/photo-1521587760476-6c12a4b040da?auto=format&fit=crop&w=900&q=80"},{"title":"Sports Complex","description":"Facilities for fitness and team sports.","imageUrl":"https://images.unsplash.com/photo-1461896836934-ffe607ba6851?auto=format&fit=crop&w=900&q=80"}]}"""),
        Template(PageTemplateKeys.Messages, "Messages", PageType.Messages, "messages", 40,
            "Principal, Manager and Director messages.",
            "<p>Leadership messages for parents and students.</p>",
            """{"messages":[{"role":"Principal","name":"Dr. A. Sharma","photoUrl":"","message":"Education is the foundation on which we build character, curiosity and courage."},{"role":"Manager","name":"R. Verma","photoUrl":"","message":"We are committed to a safe, inspiring campus experience for every learner."},{"role":"Director","name":"S. Iyer","photoUrl":"","message":"Our vision is excellence with values — preparing students for life."}]}"""),
        Template(PageTemplateKeys.Gallery, "Gallery", PageType.Gallery, "gallery", 50,
            "Photo and video gallery / media albums.",
            "<p>Moments from campus life, events and achievements.</p>",
            """{"items":[{"album":"Campus Life","type":"image","url":"https://images.unsplash.com/photo-1523050854058-8df90110c9f1?auto=format&fit=crop&w=900&q=80","caption":"Students on campus"},{"album":"Campus Life","type":"image","url":"https://images.unsplash.com/photo-1562774053-701939374585?auto=format&fit=crop&w=900&q=80","caption":"Main building"},{"album":"Events","type":"image","url":"https://images.unsplash.com/photo-1577896851231-70ef18881754?auto=format&fit=crop&w=900&q=80","caption":"Annual day"},{"album":"Events","type":"video","url":"https://www.youtube.com/embed/ScMzIvxBSi4","caption":"Campus tour"}]}"""),
        Template(PageTemplateKeys.Disclosure, "Mandatory Disclosure", PageType.Disclosure, "mandatory-disclosure", 60,
            "Mandatory disclosure documents with titles and PDF links.",
            "<p>Official documents published for transparency and compliance.</p>",
            """{"documents":[{"title":"Affiliation Certificate","category":"Affiliation","fileUrl":""},{"title":"Fee Structure","category":"Fees","fileUrl":""},{"title":"Academic Calendar","category":"Academics","fileUrl":""}]}"""),
        Template(PageTemplateKeys.Committee, "Committee", PageType.Committee, "committee", 70,
            "Managing / academic committee members.",
            "<p>Meet the committee guiding our institution.</p>",
            """{"members":[{"name":"Member One","role":"Chairperson","photoUrl":""},{"name":"Member Two","role":"Secretary","photoUrl":""},{"name":"Member Three","role":"Academic Advisor","photoUrl":""}]}"""),
        Template(PageTemplateKeys.Contact, "Contact", PageType.Contact, "contact", 80,
            "Contact details, map embed and enquiry form.",
            "<p>We would love to hear from you. Reach out using the form or visit us on campus.</p>",
            """{"formEnabled":true,"intro":"Send us a message and our admissions team will respond shortly."}""")
    ];

    private static PageTemplate Template(
        string key,
        string name,
        PageType pageType,
        string slug,
        int order,
        string description,
        string content,
        string json) => new()
    {
        Id = Guid.NewGuid(),
        TemplateKey = key,
        Name = name,
        Description = description,
        PageType = pageType,
        DefaultSlug = slug,
        DefaultTitle = name,
        DefaultContent = content,
        DefaultJsonData = json,
        IsStarter = true,
        IsActive = true,
        DisplayOrder = order,
        CreatedDate = DateTime.UtcNow,
        CreatedBy = "seed"
    };
}
