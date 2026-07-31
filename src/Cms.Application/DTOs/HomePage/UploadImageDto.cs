using Microsoft.AspNetCore.Http;

namespace Cms.Application.DTOs.HomePage;

public sealed class UploadImageDto
{
    public IFormFile File { get; set; } = null!;
    public string Folder { get; set; } = "homepage";
}
