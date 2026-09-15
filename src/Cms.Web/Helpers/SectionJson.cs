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

    /// <summary>
    /// Whether a section's stored settings actually hold anything — a row in a list, or a field
    /// with something written in it. A section an editor opened and saved without filling in
    /// holds <c>{"items":[]}</c>, which is not an empty string and is not content either.
    ///
    /// The console decides "Needs content" the same way, so the two agree: a section the console
    /// calls empty is a section the website does not draw.
    /// </summary>
    public static bool HoldsSomething(JsonElement? root)
    {
        if (root is null)
        {
            return false;
        }

        return HasValue(root.Value);

        static bool HasValue(JsonElement element) => element.ValueKind switch
        {
            JsonValueKind.Object => element.EnumerateObject().Any(p => HasValue(p.Value)),
            JsonValueKind.Array => element.EnumerateArray().Any(HasValue),
            JsonValueKind.String => !string.IsNullOrWhiteSpace(element.GetString()),
            JsonValueKind.Null or JsonValueKind.Undefined or JsonValueKind.False => false,
            _ => true
        };
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

    /// <summary>One field of one item inside a section's list, or null when it is not there.</summary>
    public static string? Item(JsonElement item, string property) =>
        item.ValueKind == JsonValueKind.Object && item.TryGetProperty(property, out var value)
            ? value.ValueKind switch
            {
                JsonValueKind.String => value.GetString(),
                JsonValueKind.Number => value.ToString(),
                JsonValueKind.Null or JsonValueKind.Undefined => null,
                _ => value.ToString()
            }
            : null;

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
