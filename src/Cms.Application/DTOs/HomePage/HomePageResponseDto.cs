using System.Text.Json.Serialization;

namespace Cms.Application.DTOs.HomePage;

/// <summary>
/// Frontend-facing homepage payload keyed by section (hero, about, statistics, ...).
/// </summary>
public class HomePageResponseDto
{
    [JsonExtensionData]
    public Dictionary<string, object?> Sections { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
