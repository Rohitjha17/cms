namespace Cms.Application.DTOs.Media;

public sealed class MediaFileDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string Url { get; set; } = string.Empty;
    public string MediaType { get; set; } = string.Empty;
    public string? Folder { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
}

public sealed class UpdateMediaDto
{
    public string? OriginalFileName { get; set; }
    public string? Folder { get; set; }
    public bool? IsActive { get; set; }
}

public sealed class SetMediaStatusDto
{
    public bool IsActive { get; set; }
}
