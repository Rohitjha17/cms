using System.Text.Json;
using System.Text.Json.Nodes;
using Cms.Shared.Helpers;

namespace Cms.Application.Validators;

public static class HomePageSectionConfigValidator
{
    private const int MaxJsonLength = 64 * 1024;
    private static readonly HashSet<string> ReservedFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "sectionKey", "title", "subtitle", "description", "buttonText", "buttonLink",
        "imageUrl", "backgroundImageUrl", "displayOrder", "isActive"
    };

    private static readonly HashSet<string> NonNegativeNumberFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "students", "teachers", "placements", "years", "columns"
    };

    /// <summary>
    /// Moves any reserved key out of the configuration and into the field that owns it, returning
    /// the configuration without it.
    ///
    /// These keys duplicate a field the editor already shows, so the section had two values for
    /// one thing and no way to tell which the website used. Rejecting them was worse than
    /// pointless: the seeded and template-provisioned sections contained them from the moment
    /// they were created, so the hero section could never be saved at all — the editor answered
    /// every attempt with "$.description is reserved".
    /// </summary>
    /// <param name="adopted">Values taken out of the configuration, keyed by reserved name.</param>
    public static string? StripReservedFields(string? json, out IReadOnlyDictionary<string, string> adopted)
    {
        var taken = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        adopted = taken;

        if (string.IsNullOrWhiteSpace(json) || json.Length > MaxJsonLength)
        {
            return json;
        }

        JsonNode? node;
        try
        {
            node = JsonNode.Parse(json);
        }
        catch (JsonException)
        {
            return json; // Invalid JSON is reported by Validate; nothing to strip here.
        }

        if (node is not JsonObject root)
        {
            return json;
        }

        foreach (var name in root.Select(pair => pair.Key).Where(ReservedFields.Contains).ToList())
        {
            if (root[name] is JsonValue value && value.TryGetValue<string>(out var text)
                && !string.IsNullOrWhiteSpace(text))
            {
                taken[name] = text;
            }

            root.Remove(name);
        }

        return taken.Count == 0 && root.Count == RootCount(json) ? json : root.ToJsonString();
    }

    private static int RootCount(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            return document.RootElement.ValueKind == JsonValueKind.Object
                ? document.RootElement.EnumerateObject().Count()
                : -1;
        }
        catch (JsonException)
        {
            return -1;
        }
    }

    public static IReadOnlyList<string> Validate(string sectionKey, string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        var errors = new List<string>();
        if (json.Length > MaxJsonLength)
        {
            return [$"Configuration for '{sectionKey}' cannot exceed 64 KB."];
        }

        try
        {
            using var document = JsonDocument.Parse(json, new JsonDocumentOptions
            {
                MaxDepth = 16,
                CommentHandling = JsonCommentHandling.Disallow,
                AllowTrailingCommas = false
            });

            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return [$"Configuration for '{sectionKey}' must be a JSON object."];
            }

            ValidateObject(document.RootElement, "$", errors);
        }
        catch (JsonException)
        {
            errors.Add($"Configuration for '{sectionKey}' contains invalid JSON.");
        }

        return errors;
    }

    private static void ValidateObject(JsonElement element, string path, ICollection<string> errors)
    {
        foreach (var property in element.EnumerateObject())
        {
            var propertyPath = $"{path}.{property.Name}";
            if (path == "$" && ReservedFields.Contains(property.Name))
            {
                errors.Add($"{propertyPath} is reserved; use the section's standard field instead.");
            }

            if (property.Name.EndsWith("Url", StringComparison.OrdinalIgnoreCase)
                && property.Value.ValueKind == JsonValueKind.String
                && !UrlHelper.IsValidUrl(property.Value.GetString()))
            {
                errors.Add($"{propertyPath} must be a valid absolute or site-relative URL.");
            }

            if (NonNegativeNumberFields.Contains(property.Name)
                && property.Value.ValueKind == JsonValueKind.Number
                && property.Value.TryGetDecimal(out var number)
                && number < 0)
            {
                errors.Add($"{propertyPath} cannot be negative.");
            }

            if (property.NameEquals("items") && property.Value.ValueKind != JsonValueKind.Array)
            {
                errors.Add($"{propertyPath} must be an array.");
            }

            if (property.Value.ValueKind == JsonValueKind.Object)
            {
                ValidateObject(property.Value, propertyPath, errors);
            }
            else if (property.Value.ValueKind == JsonValueKind.Array)
            {
                if (property.Value.GetArrayLength() > 100)
                {
                    errors.Add($"{propertyPath} cannot contain more than 100 items.");
                    continue;
                }

                var index = 0;
                foreach (var item in property.Value.EnumerateArray())
                {
                    if (item.ValueKind == JsonValueKind.Object)
                    {
                        ValidateObject(item, $"{propertyPath}[{index}]", errors);
                    }
                    else if (property.NameEquals("items"))
                    {
                        errors.Add($"{propertyPath}[{index}] must be an object.");
                    }
                    index++;
                }
            }
        }
    }
}
