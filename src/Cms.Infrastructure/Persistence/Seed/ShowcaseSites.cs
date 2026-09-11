using Cms.Domain.Enums;

namespace Cms.Infrastructure.Persistence.Seed;

/// <summary>
/// The four demo websites, one per design.
///
/// The appearance settings are deliberately spread rather than repeated: every value of every
/// choice the console offers appears on at least one of the four, so the whole surface can be
/// looked at by opening four pages instead of by editing settings one at a time. The table in
/// the class comment below is the map of which site shows what.
/// </summary>
internal static class ShowcaseSites
{
    // logoShape     original  rounded  square   rounded
    // pageTitleSize large     medium   small    xlarge
    // pageTitleAlign center   left     left     center
    // noticeBarStyle solid    gradient dark     outline
    // buttonStyle   solid     soft     outline  gradient
    // buttonShape   square    pill     rounded  square
    // buttonHover   lift      fill     slide    glow
    // cardHover     lift      zoom     tilt     glow
    // imageHover    zoom      lift     tint     zoom
    // linkHover     underline color    underline color
    // pattern       dots      bubbles  grid     shimmer
    // animation     rise      zoom     none     slide-left
    // admissions    Open      OpeningSoon Open  Closed
    // popup         poster    off      poster+form off

    internal static readonly ShowcaseSpec[] All =
    [
        new ShowcaseSpec(
            SiteId: Guid.Parse("33333333-3333-3333-3333-333333333331"),
            Key: "prestige",
            Name: "St. Aloysius Senior Secondary School",
            ShortName: "St. Aloysius",
            Tagline: "Founded on service, sustained by excellence",
            Motto: "Lux et Veritas — Light and Truth",
            Variant: HomeVariant.Prestige,
            WebsiteType: WebsiteType.School,
            Primary: "#1b2a4a",
            Secondary: "#b08d3f",
            Crest: "https://images.unsplash.com/photo-1599687267812-35c05ff70ee9?auto=format&fit=crop&w=200&q=80",
            Banner: "https://images.unsplash.com/photo-1562774053-701939374585?auto=format&fit=crop&w=1800&q=80",
            HeroSlides:
            [
                "https://images.unsplash.com/photo-1562774053-701939374585?auto=format&fit=crop&w=1800&q=80",
                "https://images.unsplash.com/photo-1562774053-701939374585?auto=format&fit=crop&w=1800&q=80",
                "https://images.unsplash.com/photo-1607237138185-eedd9c632b0b?auto=format&fit=crop&w=1800&q=80"
            ],
            Gallery:
            [
                "https://images.unsplash.com/photo-1562774053-701939374585?auto=format&fit=crop&w=900&q=80",
                "https://images.unsplash.com/photo-1562774053-701939374585?auto=format&fit=crop&w=900&q=80",
                "https://images.unsplash.com/photo-1577896851231-70ef18881754?auto=format&fit=crop&w=900&q=80",
                "https://images.unsplash.com/photo-1580582932707-520aed937b7b?auto=format&fit=crop&w=900&q=80",
                "https://images.unsplash.com/photo-1521587760476-6c12a4b040da?auto=format&fit=crop&w=900&q=80",
                "https://images.unsplash.com/photo-1509062522246-3755977927d7?auto=format&fit=crop&w=900&q=80"
            ],
            Founded: "1938",
            FounderName: "Fr. Ignatius D'Souza",
            FounderLife: "1889 – 1964",
            PrincipalName: "Dr. Meera Krishnan",
            ChairmanName: "Justice (Retd.) A. R. Pillai",
            DirectorName: "Fr. Thomas Vaz",
            ManagerName: "Mrs. Leela Fernandes",
            City: "Bengaluru",
            Address: "12 Residency Road, Bengaluru 560025, Karnataka",
            Phone: "+91 80 2558 1938",
            Email: "office@staloysius.demo",
            Settings: """
            {
              "noticeTicker":"Admissions for 2027–28 open 1 November · Founder's Day 14 December · Office closed 25 December",
              "noticeLabel":"NOTICE",
              "noticeTickerScrolls":false,
              "noticeTickerSeconds":0,
              "noticeTickerRepeat":1,
              "noticeBarStyle":"solid",
              "noticeBarColor":"#1b2a4a",
              "admissionStatus":"Open",
              "admissionsEmail":"admissions@staloysius.demo",
              "admissionsPhone":"+91 80 2558 1940",
              "brochureUrl":"/documents/prospectus.pdf",
              "applicationUrl":"/admission",
              "officeHours":"Monday to Friday, 8:30–15:30 · Saturday, 8:30–12:30",
              "whatsAppNumber":"+918025581940",
              "facebook":"https://facebook.com/staloysius.demo",
              "instagram":"https://instagram.com/staloysius.demo",
              "youTube":"https://youtube.com/@staloysius.demo",
              "twitter":"https://twitter.com/staloysius",
              "linkedIn":"https://linkedin.com/school/staloysius",
              "headerContact":true,
              "headerCtaText":"Enquire",
              "headerCtaLink":"/contact",
              "heroPlainImages":false,
              "logoShape":"original",
              "pageTitleSize":"large",
              "pageTitleAlign":"center",
              "logoHeight":64,
              "scrollAnimations":true,
              "buttonColor":"#b08d3f",
              "buttonStyle":"solid",
              "buttonShape":"square",
              "buttonHover":"lift",
              "cardHover":"lift",
              "imageHover":"zoom",
              "linkHover":"underline",
              "hoverColor":"#b08d3f",
              "sectionAnimation":"rise",
              "sectionPattern":"dots",
              "heroSlideSeconds":6,
              "heroShowControls":true,
              "popupEnabled":true,
              "popupImageUrl":"https://images.unsplash.com/photo-1509062522246-3755977927d7?auto=format&fit=crop&w=1000&q=80",
              "popupSlideSeconds":0,
              "popupAutoCloseSeconds":12,
              "popupHeading":"Founder's Day 2026",
              "popupLinkUrl":"/events",
              "popupShowEnquiryForm":false,
              "popupOncePerVisit":true,
              "enquiryTypes":"Admissions\nFees and scholarships\nTransport\nAlumni\nCareers"
            }
            """),

        new ShowcaseSpec(
            SiteId: Guid.Parse("33333333-3333-3333-3333-333333333332"),
            Key: "campus",
            Name: "Greenfield Residential Campus",
            ShortName: "Greenfield",
            Tagline: "Room to grow, in every sense",
            Motto: "Learn. Belong. Lead.",
            Variant: HomeVariant.Campus,
            WebsiteType: WebsiteType.School,
            Primary: "#14432f",
            Secondary: "#d98e2b",
            Crest: "https://images.unsplash.com/photo-1592280771190-3e2e4d571952?auto=format&fit=crop&w=200&q=80",
            Banner: "https://images.unsplash.com/photo-1592280771190-3e2e4d571952?auto=format&fit=crop&w=1800&q=80",
            HeroSlides:
            [
                "https://images.unsplash.com/photo-1592280771190-3e2e4d571952?auto=format&fit=crop&w=1800&q=80",
                "https://images.unsplash.com/photo-1571260899304-425eee4c7efc?auto=format&fit=crop&w=1800&q=80",
                "https://images.unsplash.com/photo-1571260899304-425eee4c7efc?auto=format&fit=crop&w=1800&q=80",
                "https://images.unsplash.com/photo-1519452575417-564c1401ecc0?auto=format&fit=crop&w=1800&q=80"
            ],
            Gallery:
            [
                "https://images.unsplash.com/photo-1592280771190-3e2e4d571952?auto=format&fit=crop&w=900&q=80",
                "https://images.unsplash.com/photo-1571260899304-425eee4c7efc?auto=format&fit=crop&w=900&q=80",
                "https://images.unsplash.com/photo-1571260899304-425eee4c7efc?auto=format&fit=crop&w=900&q=80",
                "https://images.unsplash.com/photo-1519452575417-564c1401ecc0?auto=format&fit=crop&w=900&q=80",
                "https://images.unsplash.com/photo-1546410531-bb4caa6b424d?auto=format&fit=crop&w=900&q=80",
                "https://images.unsplash.com/photo-1503676260728-1c00da094a0b?auto=format&fit=crop&w=900&q=80"
            ],
            Founded: "1994",
            FounderName: "Col. Harbhajan Singh Gill",
            FounderLife: "1931 – 2011",
            PrincipalName: "Col. (Retd.) Vikram Singh",
            ChairmanName: "Mrs. Kiran Gill",
            DirectorName: "Dr. Sunita Menon",
            ManagerName: "Mr. Arjun Pillai",
            City: "Dehradun",
            Address: "Greenfield Estate, Rajpur Road, Dehradun 248009, Uttarakhand",
            Phone: "+91 135 273 1994",
            Email: "office@greenfield.demo",
            Settings: """
            {
              "noticeTicker":"Parents' Weekend 18–19 October · Boarding applications for Class VI close 31 January · Winter uniform from 1 November",
              "noticeLabel":"LATEST",
              "noticeTickerScrolls":true,
              "noticeTickerSeconds":28,
              "noticeTickerRepeat":2,
              "noticeBarStyle":"gradient",
              "noticeBarColor":"#14432f",
              "admissionStatus":"OpeningSoon",
              "admissionsEmail":"admissions@greenfield.demo",
              "admissionsPhone":"+91 135 273 1996",
              "brochureUrl":"/documents/greenfield-prospectus.pdf",
              "applicationUrl":"/admission",
              "officeHours":"Monday to Saturday, 9:00–16:00",
              "whatsAppNumber":"+911352731996",
              "facebook":"https://facebook.com/greenfield.demo",
              "instagram":"https://instagram.com/greenfield.demo",
              "youTube":"https://youtube.com/@greenfield.demo",
              "headerContact":true,
              "headerCtaText":"Book a visit",
              "headerCtaLink":"/contact",
              "heroPlainImages":false,
              "logoShape":"rounded",
              "pageTitleSize":"medium",
              "pageTitleAlign":"left",
              "logoHeight":56,
              "scrollAnimations":true,
              "buttonColor":"#d98e2b",
              "buttonStyle":"soft",
              "buttonShape":"pill",
              "buttonHover":"fill",
              "cardHover":"zoom",
              "imageHover":"lift",
              "linkHover":"color",
              "hoverColor":"#14432f",
              "sectionAnimation":"zoom",
              "sectionPattern":"bubbles",
              "heroSlideSeconds":5,
              "heroShowControls":true,
              "popupEnabled":false,
              "popupOncePerVisit":false,
              "enquiryTypes":"Boarding admission\nDay admission\nAssessment weekend\nTransport\nGeneral enquiry"
            }
            """),

        new ShowcaseSpec(
            SiteId: Guid.Parse("33333333-3333-3333-3333-333333333333"),
            Key: "bulletin",
            Name: "Delhi Public Academy",
            ShortName: "DPA",
            Tagline: "Every circular, every timetable, one place",
            Motto: "Vidya Dadati Vinayam — Knowledge gives humility",
            Variant: HomeVariant.Bulletin,
            WebsiteType: WebsiteType.School,
            Primary: "#7a1b2e",
            Secondary: "#12508c",
            Crest: "https://images.unsplash.com/photo-1610484826967-09c5720778c7?auto=format&fit=crop&w=200&q=80",
            Banner: "https://images.unsplash.com/photo-1580582932707-520aed937b7b?auto=format&fit=crop&w=1800&q=80",
            HeroSlides:
            [
                "https://images.unsplash.com/photo-1580582932707-520aed937b7b?auto=format&fit=crop&w=1800&q=80",
                "https://images.unsplash.com/photo-1509062522246-3755977927d7?auto=format&fit=crop&w=1800&q=80"
            ],
            Gallery:
            [
                "https://images.unsplash.com/photo-1580582932707-520aed937b7b?auto=format&fit=crop&w=900&q=80",
                "https://images.unsplash.com/photo-1509062522246-3755977927d7?auto=format&fit=crop&w=900&q=80",
                "https://images.unsplash.com/photo-1532094349884-543bc11b234d?auto=format&fit=crop&w=900&q=80",
                "https://images.unsplash.com/photo-1544717305-2782549b5136?auto=format&fit=crop&w=900&q=80",
                "https://images.unsplash.com/photo-1427504494785-3a9ca7044f45?auto=format&fit=crop&w=900&q=80",
                "https://images.unsplash.com/photo-1503676260728-1c00da094a0b?auto=format&fit=crop&w=900&q=80"
            ],
            Founded: "1976",
            FounderName: "Shri Ram Prakash Gupta",
            FounderLife: "1920 – 1998",
            PrincipalName: "Mrs. Anjali Bhatnagar",
            ChairmanName: "Shri Mohan Lal Gupta",
            DirectorName: "Dr. Rakesh Saxena",
            ManagerName: "Shri Deepak Chandra",
            City: "New Delhi",
            Address: "Sector 12, Dwarka, New Delhi 110078",
            Phone: "+91 11 2507 1976",
            Email: "office@dpacademy.demo",
            Settings: """
            {
              "noticeTicker":"Half-yearly examinations begin 8 December · Class X pre-board timetable published · Bus route 14 revised from Monday · Fee due date extended to 20 November",
              "noticeLabel":"CIRCULARS",
              "noticeTickerScrolls":true,
              "noticeTickerSeconds":36,
              "noticeTickerRepeat":0,
              "noticeBarStyle":"dark",
              "noticeBarColor":"#7a1b2e",
              "admissionStatus":"Open",
              "admissionsEmail":"admissions@dpacademy.demo",
              "admissionsPhone":"+91 11 2507 1978",
              "brochureUrl":"/documents/dpa-prospectus.pdf",
              "applicationUrl":"/admission",
              "officeHours":"Monday to Saturday, 8:00–14:00",
              "whatsAppNumber":"+911125071978",
              "facebook":"https://facebook.com/dpacademy.demo",
              "instagram":"https://instagram.com/dpacademy.demo",
              "twitter":"https://twitter.com/dpacademy",
              "headerContact":true,
              "headerCtaText":"Admission enquiry",
              "headerCtaLink":"/admission",
              "heroPlainImages":true,
              "logoShape":"square",
              "pageTitleSize":"small",
              "pageTitleAlign":"left",
              "logoHeight":48,
              "scrollAnimations":true,
              "buttonColor":"#12508c",
              "buttonStyle":"outline",
              "buttonShape":"rounded",
              "buttonHover":"slide",
              "cardHover":"tilt",
              "imageHover":"tint",
              "linkHover":"underline",
              "hoverColor":"#7a1b2e",
              "sectionAnimation":"none",
              "sectionPattern":"grid",
              "heroSlideSeconds":8,
              "heroShowControls":false,
              "popupEnabled":true,
              "popupImageUrl":"https://images.unsplash.com/photo-1427504494785-3a9ca7044f45?auto=format&fit=crop&w=1000&q=80\nhttps://images.unsplash.com/photo-1544717305-2782549b5136?auto=format&fit=crop&w=1000&q=80",
              "popupSlideSeconds":5,
              "popupAutoCloseSeconds":0,
              "popupHeading":"Admissions 2027–28",
              "popupLinkUrl":"/admission",
              "popupShowEnquiryForm":true,
              "popupFormHeading":"Request a callback",
              "popupOncePerVisit":false,
              "enquiryTypes":"Admission enquiry\nFee and payment\nTransport and bus route\nTransfer certificate\nComplaint or feedback"
            }
            """),

        new ShowcaseSpec(
            SiteId: Guid.Parse("33333333-3333-3333-3333-333333333334"),
            Key: "atrium",
            Name: "Riverstone International School",
            ShortName: "Riverstone",
            Tagline: "An education that reads like a prospectus",
            Motto: "Ad Astra — To the stars",
            Variant: HomeVariant.Atrium,
            WebsiteType: WebsiteType.Other,
            Primary: "#23262b",
            Secondary: "#a8762c",
            Crest: "https://images.unsplash.com/photo-1571260899304-425eee4c7efc?auto=format&fit=crop&w=200&q=80",
            Banner: "https://images.unsplash.com/photo-1562774053-701939374585?auto=format&fit=crop&w=1800&q=80",
            HeroSlides:
            [
                "https://images.unsplash.com/photo-1562774053-701939374585?auto=format&fit=crop&w=1800&q=80",
                "https://images.unsplash.com/photo-1541339907198-e08756dedf3f?auto=format&fit=crop&w=1800&q=80",
                "https://images.unsplash.com/photo-1498243691581-b145c3f54a5a?auto=format&fit=crop&w=1800&q=80"
            ],
            Gallery:
            [
                "https://images.unsplash.com/photo-1562774053-701939374585?auto=format&fit=crop&w=900&q=80",
                "https://images.unsplash.com/photo-1541339907198-e08756dedf3f?auto=format&fit=crop&w=900&q=80",
                "https://images.unsplash.com/photo-1498243691581-b145c3f54a5a?auto=format&fit=crop&w=900&q=80",
                "https://images.unsplash.com/photo-1523240795612-9a054b0db644?auto=format&fit=crop&w=900&q=80",
                "https://images.unsplash.com/photo-1517486808906-6ca8b3f04846?auto=format&fit=crop&w=900&q=80",
                "https://images.unsplash.com/photo-1519452575417-564c1401ecc0?auto=format&fit=crop&w=900&q=80"
            ],
            Founded: "2004",
            FounderName: "Dr. Nandita Roy",
            FounderLife: "b. 1952",
            PrincipalName: "Mr. Julian Mathews",
            ChairmanName: "Dr. Nandita Roy",
            DirectorName: "Ms. Farah Qureshi",
            ManagerName: "Mr. Sanjay Kulkarni",
            City: "Pune",
            Address: "Riverstone Campus, Baner Road, Pune 411045, Maharashtra",
            Phone: "+91 20 6720 2004",
            Email: "hello@riverstone.demo",
            Settings: """
            {
              "noticeTicker":"Prospectus 2027 now available · Admissions reopen 3 March",
              "noticeLabel":"NEWS",
              "noticeTickerScrolls":false,
              "noticeTickerSeconds":0,
              "noticeTickerRepeat":1,
              "noticeBarStyle":"outline",
              "noticeBarColor":"#a8762c",
              "admissionStatus":"Closed",
              "admissionsEmail":"admissions@riverstone.demo",
              "admissionsPhone":"+91 20 6720 2006",
              "brochureUrl":"/documents/riverstone-prospectus.pdf",
              "applicationUrl":"/admission",
              "officeHours":"Monday to Friday, 8:00–16:00",
              "whatsAppNumber":"+912067202006",
              "instagram":"https://instagram.com/riverstone.demo",
              "linkedIn":"https://linkedin.com/school/riverstone",
              "youTube":"https://youtube.com/@riverstone.demo",
              "headerContact":false,
              "headerCtaText":"Request the prospectus",
              "headerCtaLink":"/admission",
              "heroPlainImages":false,
              "logoShape":"rounded",
              "pageTitleSize":"xlarge",
              "pageTitleAlign":"center",
              "logoHeight":72,
              "scrollAnimations":true,
              "buttonColor":"#a8762c",
              "buttonStyle":"gradient",
              "buttonShape":"square",
              "buttonHover":"glow",
              "cardHover":"glow",
              "imageHover":"zoom",
              "linkHover":"color",
              "hoverColor":"#a8762c",
              "sectionAnimation":"slide-left",
              "sectionPattern":"shimmer",
              "heroSlideSeconds":7,
              "heroShowControls":true,
              "popupEnabled":false,
              "popupOncePerVisit":false,
              "enquiryTypes":"Admissions\nCurriculum and IB\nScholarships\nCareers\nMedia"
            }
            """)
    ];
}
