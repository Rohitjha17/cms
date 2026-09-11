using System.Globalization;
using Cms.Domain.Constants;
using Cms.Domain.Entities;
using Cms.Domain.Enums;

namespace Cms.Infrastructure.Persistence.Seed;

/// <summary>
/// The pages of one showcase website: all eight starter types filled in, plus a ninth built
/// from the school's own HTML so that switch is on something too.
/// </summary>
internal static class ShowcasePages
{
    internal static List<Page> Build(Guid tenantId, ShowcaseSpec s)
    {
        var now = DateTime.UtcNow;
        var order = 0;
        var pages = new List<Page>();

        void Add(PageType type, string templateKey, string title, string slug, string excerpt,
            string content, string json, string? image = null, bool customHtml = false)
            => pages.Add(new Page
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                SiteId = s.SiteId,
                PageType = type,
                TemplateKey = templateKey,
                Title = title,
                Slug = slug,
                Excerpt = excerpt,
                Content = content,
                JsonData = json,
                UseCustomHtml = customHtml,
                FeaturedImageUrl = image,
                MetaTitle = $"{title} · {s.ShortName}",
                MetaDescription = excerpt,
                ShowInMenu = true,
                MenuOrder = ++order,
                IsActive = true,
                CreatedDate = now,
                CreatedBy = "showcase"
            });

        Add(PageType.About, PageTemplateKeys.About, "About us", "about",
            $"{s.ShortName} since {s.Founded} — who we are and what we are for.",
            $"<p>{s.ShortName} was founded in {s.Founded} by {s.FounderName}. We teach children from pre-primary to Class XII on an eleven-acre campus in {s.City}.</p><h2>What we are for</h2><p>To send out young people who are hard to fool, easy to work with, and unwilling to leave a job half done.</p><h2>How we teach</h2><p>Specialist teaching from Class VI, classes capped at 28, and a form tutor who stays with a group for three years. Games every afternoon for every child.</p><h2>Inspection and affiliation</h2><p>Affiliated to the Central Board of Secondary Education. Our most recent inspection report is published in full under Mandatory Disclosure.</p>",
            $$"""{"mission":"To form character as carefully as we teach a syllabus.","vision":"{{s.Motto}}","history":"Founded in {{s.Founded}} by {{s.FounderName}} with four classrooms and thirty-one children."}""",
            s.Gallery[1]);

        Add(PageType.Admission, PageTemplateKeys.Admission, "Admission", "admission",
            "How to apply, what we ask for, and when.",
            "<p>Applications for 2027–28 are invited for Classes I to IX. We admit on assessment and interaction, not on a first-come basis.</p><h2>Fees</h2><p>The full fee schedule is published under Mandatory Disclosure. There are no compulsory charges outside it.</p>",
            """
            {"eligibility":"Children completing five years by 31 March are eligible for Class I. Lateral entry is offered to Classes II to IX where places exist.",
             "processSteps":[
               {"title":"Enquiry","description":"Submit the enquiry form or telephone the admissions office."},
               {"title":"Campus visit","description":"Visit on a working day. We do not admit a family that has not seen the school."},
               {"title":"Application","description":"Complete the form and attach the documents listed below."},
               {"title":"Assessment","description":"An age-appropriate assessment and a conversation with the child and parents."},
               {"title":"Offer","description":"Offers are made within ten working days of assessment."},
               {"title":"Confirmation","description":"Confirm the place and complete the fee formalities within fourteen days."}],
             "documents":["Birth certificate (attested)","Report card from the previous school","Transfer certificate","Aadhaar card of the child","Four passport photographs","Proof of residence"]}
            """,
            s.Gallery[2]);

        Add(PageType.Facilities, PageTemplateKeys.Facilities, "Facilities", "facilities",
            "Every space on campus, and what it is used for.",
            "<p>Eleven acres, purpose-built. Nothing here is shared with another institution.</p>",
            """
            {"items":[
              {"title":"Smart classrooms","description":"Forty-two classrooms with a projector and a document camera in each.","imageUrl":"https://images.unsplash.com/photo-1580582932707-520aed937b7b?auto=format&fit=crop&w=900&q=80"},
              {"title":"Science laboratories","description":"Separate Physics, Chemistry and Biology laboratories, open until 17:00.","imageUrl":"https://images.unsplash.com/photo-1532094349884-543bc11b234d?auto=format&fit=crop&w=900&q=80"},
              {"title":"Library","description":"Twelve thousand volumes, a reading room and a digital archive.","imageUrl":"https://images.unsplash.com/photo-1521587760476-6c12a4b040da?auto=format&fit=crop&w=900&q=80"},
              {"title":"Sports complex","description":"A 400m track, cricket ground, astro-turf and an indoor hall.","imageUrl":"https://images.unsplash.com/photo-1571260899304-425eee4c7efc?auto=format&fit=crop&w=900&q=80"},
              {"title":"Auditorium","description":"Six hundred seats, with a sprung stage and a lighting rig.","imageUrl":"https://images.unsplash.com/photo-1507676184212-d03ab07a01bf?auto=format&fit=crop&w=900&q=80"},
              {"title":"Infirmary","description":"A full-time nurse, two beds and a visiting doctor three days a week.","imageUrl":"https://images.unsplash.com/photo-1519494026892-80bbd2d6fd0d?auto=format&fit=crop&w=900&q=80"},
              {"title":"Dining hall","description":"A freshly cooked vegetarian meal daily, with special-diet provision.","imageUrl":"https://images.unsplash.com/photo-1567521464027-f127ff144326?auto=format&fit=crop&w=900&q=80"},
              {"title":"Transport","description":"Fourteen routes with GPS tracking and an attendant on every bus.","imageUrl":"https://images.unsplash.com/photo-1557223562-6c77ef16210f?auto=format&fit=crop&w=900&q=80"}]}
            """,
            s.Gallery[3]);

        Add(PageType.Messages, PageTemplateKeys.Messages, "Messages", "messages",
            "From the Principal, the Chairman, the Director and the Manager.",
            "<p>Four people are answerable for this school. Here is each of them, in their own words.</p>",
            $$"""
            {"messages":[
              {"role":"Principal","name":"{{s.PrincipalName}}","photoUrl":"https://images.unsplash.com/photo-1544717305-2782549b5136?auto=format&fit=crop&w=600&q=80","message":"Our first duty is that every child is safe, known and happy. The results follow from that, and never the other way round."},
              {"role":"Chairman","name":"{{s.ChairmanName}}","photoUrl":"https://images.unsplash.com/photo-1560250097-0b93528c311a?auto=format&fit=crop&w=600&q=80","message":"A school is a promise made to a family, renewed every morning. The board's work is to see that it is kept."},
              {"role":"Director","name":"{{s.DirectorName}}","photoUrl":"https://images.unsplash.com/photo-1573497019940-1c28c88b4f3e?auto=format&fit=crop&w=600&q=80","message":"Depth before pace. A child who understands will always catch up; a child who has only kept up will not."},
              {"role":"Manager","name":"{{s.ManagerName}}","photoUrl":"https://images.unsplash.com/photo-1519085360753-af0119f7cbe7?auto=format&fit=crop&w=600&q=80","message":"Transport, meals and the office are mine. If any of them is not working for your family, write to me directly."}]}
            """,
            s.Gallery[4]);

        Add(PageType.Gallery, PageTemplateKeys.Gallery, "Gallery", "gallery",
            "Photographs and film from around the school.",
            "<p>Moments from campus life, events and achievements.</p>",
            $$"""
            {"items":[
              {"album":"Campus life","type":"image","url":"{{s.Gallery[0]}}","caption":"Students on campus"},
              {"album":"Campus life","type":"image","url":"{{s.Gallery[1]}}","caption":"The main building"},
              {"album":"Campus life","type":"image","url":"{{s.Gallery[4]}}","caption":"The library"},
              {"album":"Events","type":"image","url":"{{s.Gallery[2]}}","caption":"Annual Day"},
              {"album":"Events","type":"image","url":"{{s.Gallery[5]}}","caption":"Assembly"},
              {"album":"Sport","type":"image","url":"{{s.Gallery[3]}}","caption":"Inter-house athletics"},
              {"album":"Film","type":"video","url":"https://www.youtube.com/embed/ScMzIvxBSi4","caption":"Campus tour"},
              {"album":"Film","type":"video","url":"https://www.youtube.com/embed/aqz-KE-bpKQ","caption":"Annual Day highlights"}]}
            """,
            s.Gallery[0]);

        Add(PageType.Disclosure, PageTemplateKeys.Disclosure, "Mandatory Disclosure", "mandatory-disclosure",
            "Documents published for transparency and compliance.",
            "<p>Published in full, as required by the board and as a matter of course.</p>",
            $$"""
            {"facts":[
              {"label":"Affiliation number","value":"27{{s.Founded}}04"},
              {"label":"School code","value":"{{s.Key.ToUpperInvariant()}}-{{s.Founded}}"},
              {"label":"Board","value":"Central Board of Secondary Education"},
              {"label":"Year of establishment","value":"{{s.Founded}}"}],
             "documents":[
              {"title":"Affiliation certificate","category":"Affiliation","fileUrl":"/documents/affiliation.pdf"},
              {"title":"Trust registration certificate","category":"Affiliation","fileUrl":"/documents/trust.pdf"},
              {"title":"No-objection certificate","category":"Affiliation","fileUrl":"/documents/noc.pdf"},
              {"title":"Building safety certificate","category":"Safety","fileUrl":"/documents/building-safety.pdf"},
              {"title":"Fire safety certificate","category":"Safety","fileUrl":"/documents/fire-safety.pdf"},
              {"title":"Water and sanitation certificate","category":"Safety","fileUrl":"/documents/water.pdf"},
              {"title":"Fee structure 2027–28","category":"Fees","fileUrl":"/documents/fee-structure.pdf"},
              {"title":"Academic calendar","category":"Academics","fileUrl":"/documents/academic-calendar.pdf"},
              {"title":"Annual report","category":"Academics","fileUrl":"/documents/annual-report.pdf"},
              {"title":"Managing committee list","category":"Governance","fileUrl":"/documents/committee.pdf"}]}
            """);

        Add(PageType.Committee, PageTemplateKeys.Committee, "Committee", "committee",
            "The managing committee and the academic council.",
            "<p>The committee meets three times a year. Minutes are available to parents on request.</p>",
            $$"""
            {"members":[
              {"name":"{{s.ChairmanName}}","role":"Chairperson","photoUrl":"https://images.unsplash.com/photo-1560250097-0b93528c311a?auto=format&fit=crop&w=400&q=80"},
              {"name":"{{s.PrincipalName}}","role":"Principal and Member Secretary","photoUrl":"https://images.unsplash.com/photo-1544717305-2782549b5136?auto=format&fit=crop&w=400&q=80"},
              {"name":"{{s.DirectorName}}","role":"Academic Advisor","photoUrl":"https://images.unsplash.com/photo-1573497019940-1c28c88b4f3e?auto=format&fit=crop&w=400&q=80"},
              {"name":"{{s.ManagerName}}","role":"Manager","photoUrl":"https://images.unsplash.com/photo-1519085360753-af0119f7cbe7?auto=format&fit=crop&w=400&q=80"},
              {"name":"Mrs. Geeta Raman","role":"Parent Representative","photoUrl":"https://images.unsplash.com/photo-1494790108377-be9c29b29330?auto=format&fit=crop&w=400&q=80"},
              {"name":"Mr. Suresh Pillai","role":"Teacher Representative","photoUrl":"https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?auto=format&fit=crop&w=400&q=80"},
              {"name":"Dr. Farida Khan","role":"Board Nominee","photoUrl":"https://images.unsplash.com/photo-1559839734-2b71ea197ec2?auto=format&fit=crop&w=400&q=80"}]}
            """);

        Add(PageType.Contact, PageTemplateKeys.Contact, "Contact", "contact",
            "Where we are, when we are open, and how to reach us.",
            $"<p>The office is open through the working day. We answer messages within one working day and publish our record.</p><p><strong>{s.Address}</strong></p>",
            $$"""{"formEnabled":true,"intro":"Send us a message and the admissions office will respond within one working day.","mapEmbedUrl":"https://maps.google.com/maps?q={{Uri.EscapeDataString(s.City)}}&t=&z=13&ie=UTF8&iwloc=&output=embed"}""");

        // The ninth page is the school's own HTML, so the "build this page yourself" switch is
        // on something a tester can actually look at rather than only reading about.
        Add(PageType.Custom, null!, "Fee structure", "fee-structure",
            "The full fee schedule for 2027–28, as published.",
            $$"""
            <section style="max-width:60rem;margin:0 auto">
              <h1 style="font-family:Georgia,serif">Fee structure 2027–28</h1>
              <p style="color:#555">Every charge the school makes is in this table. There are no compulsory costs outside it.</p>
              <table style="width:100%;border-collapse:collapse;margin-top:1.5rem">
                <thead>
                  <tr style="background:{{s.Primary}};color:#fff;text-align:left">
                    <th style="padding:.7rem .8rem">Class</th>
                    <th style="padding:.7rem .8rem">Tuition (annual)</th>
                    <th style="padding:.7rem .8rem">Admission (one-time)</th>
                    <th style="padding:.7rem .8rem">Transport (optional)</th>
                  </tr>
                </thead>
                <tbody>
                  <tr><td style="padding:.6rem .8rem;border-bottom:1px solid #ddd">Pre-primary</td><td style="padding:.6rem .8rem;border-bottom:1px solid #ddd">₹ 68,000</td><td style="padding:.6rem .8rem;border-bottom:1px solid #ddd">₹ 25,000</td><td style="padding:.6rem .8rem;border-bottom:1px solid #ddd">₹ 18,000</td></tr>
                  <tr><td style="padding:.6rem .8rem;border-bottom:1px solid #ddd">I – V</td><td style="padding:.6rem .8rem;border-bottom:1px solid #ddd">₹ 82,000</td><td style="padding:.6rem .8rem;border-bottom:1px solid #ddd">₹ 25,000</td><td style="padding:.6rem .8rem;border-bottom:1px solid #ddd">₹ 20,000</td></tr>
                  <tr><td style="padding:.6rem .8rem;border-bottom:1px solid #ddd">VI – VIII</td><td style="padding:.6rem .8rem;border-bottom:1px solid #ddd">₹ 94,000</td><td style="padding:.6rem .8rem;border-bottom:1px solid #ddd">₹ 25,000</td><td style="padding:.6rem .8rem;border-bottom:1px solid #ddd">₹ 20,000</td></tr>
                  <tr><td style="padding:.6rem .8rem;border-bottom:1px solid #ddd">IX – X</td><td style="padding:.6rem .8rem;border-bottom:1px solid #ddd">₹ 1,06,000</td><td style="padding:.6rem .8rem;border-bottom:1px solid #ddd">₹ 25,000</td><td style="padding:.6rem .8rem;border-bottom:1px solid #ddd">₹ 22,000</td></tr>
                  <tr><td style="padding:.6rem .8rem;border-bottom:1px solid #ddd">XI – XII</td><td style="padding:.6rem .8rem;border-bottom:1px solid #ddd">₹ 1,18,000</td><td style="padding:.6rem .8rem;border-bottom:1px solid #ddd">₹ 25,000</td><td style="padding:.6rem .8rem;border-bottom:1px solid #ddd">₹ 22,000</td></tr>
                </tbody>
              </table>
              <p style="margin-top:1.25rem;font-size:.92rem;color:#666">Fees are payable in three instalments. Scholarships on merit and on means are assessed in March; see <a href="admission" style="color:{{s.Primary}}">Admission</a>.</p>
            </section>
            """,
            "{}",
            customHtml: true);

        return pages;
    }
}
