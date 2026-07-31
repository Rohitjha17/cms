namespace Cms.Application.DTOs.HomePage;

public class ReorderHomePageSectionsDto
{
    public List<ReorderItemDto> Items { get; set; } = [];
}

public class ReorderItemDto
{
    public string SectionKey { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}
