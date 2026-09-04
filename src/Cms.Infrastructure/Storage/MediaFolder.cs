using Cms.Application.Interfaces;

namespace Cms.Infrastructure.Storage;

/// <summary>
/// Where a school's uploads live, written so a person can read it.
///
/// The folders used to be the tenant's and the website's ids, so a bucket looked like
/// <c>11111111-1111-…/22222222-2222-…/media/</c> and nobody opening it could tell whose
/// content it was, or which of two schools they were about to delete.
///
/// They are named now: the tenant's code, which is unique across the whole platform, and then
/// the school's own name — <c>demo/cambridge-high-school/media/</c>. The website's key was
/// tried first and is guaranteed unique, but a key like "school" or "college" says nothing
/// about which school it is, which was the point of the exercise.
///
/// The cost of using the name is that two websites in one tenant named exactly the same would
/// share a folder. That is an administrator naming two schools identically, which already
/// makes them indistinguishable in the console's own list, and it mixes files rather than
/// losing them. Where a name is missing the key is used, and where both are missing the id —
/// a folder nobody can read still beats two schools sharing one by accident.
/// </summary>
public static class MediaFolder
{
    /// <summary>Longest a single segment may be, so a name cannot run away with the key.</summary>
    private const int MaxSegment = 60;

    public static string For(ITenantContext tenant, ISiteContext site, string folder)
    {
        var tenantSegment = Segment(tenant.TenantCode, tenant.TenantId);

        // The school's name first, because that is what somebody opening the bucket is looking
        // for; the key behind it, because a school may have no name set but always has a key.
        var siteSegment = Segment(site.SiteName, null) is { Length: > 0 } named && named != "unknown"
            ? named
            : Segment(site.SiteKey, site.SiteId);

        return $"{tenantSegment}/{siteSegment}/{Segment(folder, null)}";
    }

    /// <summary>
    /// Lower case, and only letters, digits and hyphens. Anything else — a space, an accent, a
    /// slash someone typed into a name — becomes a hyphen, so the name can never add a folder
    /// level of its own or produce a key the bucket will not take.
    /// </summary>
    public static string Segment(string? name, Guid? fallback)
    {
        var cleaned = new string((name ?? string.Empty)
            .Trim()
            .ToLowerInvariant()
            .Select(c => char.IsAsciiLetterOrDigit(c) ? c : '-')
            .ToArray())
            .Trim('-');

        while (cleaned.Contains("--", StringComparison.Ordinal))
        {
            cleaned = cleaned.Replace("--", "-", StringComparison.Ordinal);
        }

        if (cleaned.Length > MaxSegment)
        {
            cleaned = cleaned[..MaxSegment].Trim('-');
        }

        return cleaned.Length > 0
            ? cleaned
            : fallback?.ToString() ?? "unknown";
    }
}
