namespace Cms.Application.DTOs.HomePage;

public class UploadImageResultDto
{
    public Guid MediaId { get; set; }
    public string Url { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
}
