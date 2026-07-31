using Cms.Application.DTOs.HomePage;
using Cms.Domain.Entities;
using Cms.Shared.Helpers;

namespace Cms.Application.Mapping;

public static class HomePageMapper
{
    private static readonly HashSet<string> ReservedResponseFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "title", "subtitle", "description", "buttonText", "buttonLink",
        "imageUrl", "backgroundImageUrl", "displayOrder", "isActive", "sectionKey"
    };

    public static HomePageSectionDto ToDto(HomePageSection entity) => new()
    {
        Id = entity.Id,
        SectionKey = entity.SectionKey,
        Title = entity.Title,
        SubTitle = entity.SubTitle,
        Description = entity.Description,
        ButtonText = entity.ButtonText,
        ButtonLink = entity.ButtonLink,
        ImageUrl = entity.ImageUrl,
        BackgroundImageUrl = entity.BackgroundImageUrl,
        JsonData = entity.JsonData,
        Config = JsonHelper.DeserializeToObject(entity.JsonData),
        DisplayOrder = entity.DisplayOrder,
        IsActive = entity.IsActive,
        CreatedDate = entity.CreatedDate,
        UpdatedDate = entity.UpdatedDate
    };

    public static Dictionary<string, object?> ToFrontendSection(HomePageSection entity)
    {
        var payload = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
        {
            ["title"] = entity.Title,
            ["subtitle"] = entity.SubTitle,
            ["description"] = entity.Description,
            ["buttonText"] = entity.ButtonText,
            ["buttonLink"] = entity.ButtonLink,
            ["imageUrl"] = entity.ImageUrl,
            ["backgroundImageUrl"] = entity.BackgroundImageUrl,
            ["displayOrder"] = entity.DisplayOrder,
            ["isActive"] = entity.IsActive
        };

        var config = JsonHelper.DeserializeToObject(entity.JsonData);
        if (config is System.Text.Json.JsonElement { ValueKind: System.Text.Json.JsonValueKind.Object } element)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (!ReservedResponseFields.Contains(property.Name))
                {
                    payload[property.Name] = property.Value.Clone();
                }
            }
        }

        return payload;
    }
}
