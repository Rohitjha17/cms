using System.Text.Json;

namespace Cms.Web.Helpers;

public static class SectionJson
{
    public static JsonElement? Parse(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            return document.RootElement.Clone();
        }
        catch
        {
            return null;
        }
    }

    public static string? GetString(JsonElement? root, string property)
    {
        if (root is null || root.Value.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        if (!root.Value.TryGetProperty(property, out var value))
        {
            return null;
        }

        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString(),
            JsonValueKind.Number => value.ToString(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            _ => value.ToString()
        };
    }

    public static IEnumerable<JsonElement> GetArray(JsonElement? root, string property)
    {
        if (root is null || root.Value.ValueKind != JsonValueKind.Object)
        {
            return [];
        }

        if (!root.Value.TryGetProperty(property, out var value) || value.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return value.EnumerateArray();
    }
}
