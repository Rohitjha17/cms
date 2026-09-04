using Cms.Application.Interfaces;

namespace Cms.Infrastructure.Storage;

/// <summary>
/// Where a school's uploads live, written so a person can read it.
///
/// The folders used to be the tenant's and the website's ids, so a bucket looked like
/// <c>11111111-1111-…/22222222-2222-…/media/</c> and nobody opening it could tell whose
/// content it was, or which of two schools they were about to delete. The names are used
/// instead: the tenant's code, which is unique across the platform, and the website's key,
/// which is unique within that tenant — so <c>demo/school/media/</c> is both readable and
/// still cannot collide with another school's.
///
/// A name that is missing or unusable falls back to the id, because a folder nobody can read
/// is better than two schools sharing one.
/// </summary>
public static class MediaFolder
{
    /// <summary>Longest a single segment may be, so a name cannot run away with the key.</summary>
    private const int MaxSegment = 60;

    public static string For(ITenantContext tenant, ISiteContext site, string folder)
    {
        var tenantSegment = Segment(tenant.TenantCode, tenant.TenantId);
        var siteSegment = Segment(site.SiteKey, site.SiteId);

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
