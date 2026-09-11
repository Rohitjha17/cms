using Cms.Domain.Enums;

namespace Cms.Infrastructure.Persistence.Seed;

/// <summary>
/// One complete demo website: which design it wears, what it is called, and the handful of
/// values that make it look like its own school rather than a copy of the last one.
///
/// Four of these exist, one per design. They are for showing a school what the product does
/// and for testing it by hand, so between them they must exercise every section, every page
/// type and every appearance option the console offers — a setting nobody can see on any of
/// the four is a setting nobody will test.
/// </summary>
internal sealed record ShowcaseSpec(
    Guid SiteId,
    string Key,
    string Name,
    string ShortName,
    string Tagline,
    string Motto,
    HomeVariant Variant,
    WebsiteType WebsiteType,
    string Primary,
    string Secondary,
    string Crest,
    string Banner,
    string[] HeroSlides,
    string[] Gallery,
    string Founded,
    string FounderName,
    string FounderLife,
    string PrincipalName,
    string ChairmanName,
    string DirectorName,
    string ManagerName,
    string City,
    string Address,
    string Phone,
    string Email,
    string Settings);
