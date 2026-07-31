using Cms.Domain.Common;
using Cms.Domain.Enums;

namespace Cms.Domain.Entities;

public class MediaFile : BaseEntity, ITenantEntity, ISiteEntity
{
    public Guid TenantId { get; set; }
    public Guid SiteId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string Url { get; set; } = string.Empty;
    public string StorageKey { get; set; } = string.Empty;
    public MediaType MediaType { get; set; } = MediaType.Image;
    public string? Folder { get; set; }
    public bool IsActive { get; set; } = true;

    public Tenant Tenant { get; set; } = null!;
    public Site Site { get; set; } = null!;
}
