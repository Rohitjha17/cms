namespace Cms.Application.DTOs.HomePage;

public class CreateHomePageSectionDto
{
    public string SectionKey { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? SubTitle { get; set; }
    public string? Description { get; set; }
    public string? ButtonText { get; set; }
    public string? ButtonLink { get; set; }
    public string? ImageUrl { get; set; }
    public string? BackgroundImageUrl { get; set; }
    public string? JsonData { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
