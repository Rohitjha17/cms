namespace Cms.Domain.Constants;

public static class PageTemplateKeys
{
    public const string HomeClassic = "home-classic";
    public const string HomeModern = "home-modern";
    public const string HomeCampus = "home-campus";
    public const string HomeAcademic = "home-academic";
    public const string HomePrestige = "home-prestige";
    public const string About = "about";
    public const string Admission = "admission";
    public const string Facilities = "facilities";
    public const string Messages = "messages";
    public const string Gallery = "gallery";
    public const string Disclosure = "disclosure";
    public const string Committee = "committee";
    public const string Contact = "contact";

    public static readonly string[] StarterPages =
    [
        About,
        Admission,
        Facilities,
        Messages,
        Gallery,
        Disclosure,
        Committee,
        Contact
    ];
}
