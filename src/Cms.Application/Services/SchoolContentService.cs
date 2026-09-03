using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Cms.Application.DTOs.Content;
using Cms.Application.DTOs.SchoolContent;
using Cms.Application.Interfaces;
using Cms.Shared.Exceptions;
using FluentValidation;

namespace Cms.Application.Services;

/// <summary>
/// Presents faculty, news, events and site settings as real models.
///
/// Storage goes through <see cref="ISiteContentService"/> so tenant scoping, HTML
/// sanitisation, key uniqueness and activity logging keep working exactly as they do for
/// every other content type; this class only owns the mapping between a typed model and the
/// generic entry it is stored in.
/// </summary>
public sealed class SchoolContentService : ISchoolContentService
{
    private readonly ISiteContentService _content;
    private readonly IValidator<SaveFacultyMemberDto> _facultyValidator;
    private readonly IValidator<SaveNewsArticleDto> _newsValidator;
    private readonly IValidator<SaveSchoolEventDto> _eventValidator;
    private readonly IValidator<SaveDepartmentDto> _departmentValidator;
    private readonly IValidator<SiteSettingsDto> _settingsValidator;

    public SchoolContentService(
        ISiteContentService content,
        IValidator<SaveFacultyMemberDto> facultyValidator,
        IValidator<SaveNewsArticleDto> newsValidator,
        IValidator<SaveSchoolEventDto> eventValidator,
        IValidator<SaveDepartmentDto> departmentValidator,
        IValidator<SiteSettingsDto> settingsValidator)
    {
        _content = content;
        _facultyValidator = facultyValidator;
        _newsValidator = newsValidator;
        _eventValidator = eventValidator;
        _departmentValidator = departmentValidator;
        _settingsValidator = settingsValidator;
    }

    // -----------------------------------------------------------------------
    // Faculty and staff
    // -----------------------------------------------------------------------

    public async Task<IReadOnlyList<FacultyMemberDto>> GetFacultyAsync(
        bool includeUnpublished, CancellationToken cancellationToken)
    {
        var entries = await _content.GetEntriesAsync(
            SchoolContentTypes.Faculty, includeUnpublished, cancellationToken);
        return entries.Select(ToFaculty)
            .OrderBy(x => x.Category)
            .ThenBy(x => x.DisplayOrder)
            .ThenBy(x => x.FullName, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    public async Task<FacultyMemberDto> GetFacultyMemberAsync(Guid id, CancellationToken cancellationToken) =>
        ToFaculty(await RequireEntryAsync(id, SchoolContentTypes.Faculty, "Staff member", cancellationToken));

    public async Task<FacultyMemberDto> SaveFacultyMemberAsync(
        Guid? id, SaveFacultyMemberDto dto, CancellationToken cancellationToken)
    {
        dto.FullName = dto.FullName.Trim();
        await _facultyValidator.ValidateAndThrowAsync(dto, cancellationToken);

        var json = new JsonObject
        {
            ["designation"] = Clean(dto.Designation),
            ["department"] = Clean(dto.Department),
            ["category"] = dto.Category.ToString(),
            ["qualification"] = Clean(dto.Qualification),
            ["experienceYears"] = dto.ExperienceYears,
            ["email"] = Clean(dto.Email),
            ["phone"] = Clean(dto.Phone)
        };

        var saved = await _content.SaveEntryAsync(id, new SaveContentEntryDto
        {
            ContentType = SchoolContentTypes.Faculty,
            Key = await ResolveKeyAsync(id, dto.Key, dto.FullName, SchoolContentTypes.Faculty, cancellationToken),
            Title = dto.FullName,
            Summary = Clean(dto.Headline),
            Body = dto.Biography,
            ImageUrl = Clean(dto.PhotoUrl),
            JsonData = json.ToJsonString(),
            DisplayOrder = dto.DisplayOrder,
            IsActive = dto.IsPublished
        }, cancellationToken);

        return ToFaculty(saved);
    }

    public Task DeleteFacultyMemberAsync(Guid id, CancellationToken cancellationToken) =>
        DeleteAsync(id, SchoolContentTypes.Faculty, "Staff member", cancellationToken);

    // -----------------------------------------------------------------------
    // News, notices and circulars
    // -----------------------------------------------------------------------

    public async Task<IReadOnlyList<NewsArticleDto>> GetNewsAsync(
        bool includeUnpublished, CancellationToken cancellationToken)
    {
        var entries = await _content.GetEntriesAsync(
            SchoolContentTypes.News, includeUnpublished, cancellationToken);
        return entries.Select(ToNews)
            .OrderByDescending(x => x.IsFeatured)
            .ThenByDescending(x => x.PublishDate ?? DateTime.MinValue)
            .ThenBy(x => x.DisplayOrder)
            .ToList();
    }

    public async Task<NewsArticleDto> GetNewsArticleAsync(Guid id, CancellationToken cancellationToken) =>
        ToNews(await RequireEntryAsync(id, SchoolContentTypes.News, "Article", cancellationToken));

    public async Task<NewsArticleDto?> GetNewsArticleByKeyAsync(string key, CancellationToken cancellationToken)
    {
        try
        {
            var entry = await _content.GetEntryByKeyAsync(
                SchoolContentTypes.News, key, includeInactive: false, cancellationToken);
            return ToNews(entry);
        }
        catch (NotFoundException)
        {
            return null;
        }
    }

    public async Task<NewsArticleDto> SaveNewsArticleAsync(
        Guid? id, SaveNewsArticleDto dto, CancellationToken cancellationToken)
    {
        dto.Headline = dto.Headline.Trim();
        await _newsValidator.ValidateAndThrowAsync(dto, cancellationToken);

        var json = new JsonObject
        {
            ["category"] = dto.Category.ToString(),
            ["attachmentUrl"] = Clean(dto.AttachmentUrl),
            ["isFeatured"] = dto.IsFeatured
        };

        var saved = await _content.SaveEntryAsync(id, new SaveContentEntryDto
        {
            ContentType = SchoolContentTypes.News,
            Key = await ResolveKeyAsync(id, dto.Key, dto.Headline, SchoolContentTypes.News, cancellationToken),
            Title = dto.Headline,
            Summary = Clean(dto.Summary),
            Body = dto.Body,
            ImageUrl = Clean(dto.ImageUrl),
            JsonData = json.ToJsonString(),
            DisplayOrder = dto.DisplayOrder,
            IsActive = dto.IsPublished,
            PublishDate = dto.PublishDate ?? DateTime.UtcNow
        }, cancellationToken);

        return ToNews(saved);
    }

    public Task DeleteNewsArticleAsync(Guid id, CancellationToken cancellationToken) =>
        DeleteAsync(id, SchoolContentTypes.News, "Article", cancellationToken);

    // -----------------------------------------------------------------------
    // Events
    // -----------------------------------------------------------------------

    public async Task<IReadOnlyList<SchoolEventDto>> GetEventsAsync(
        bool includeUnpublished, CancellationToken cancellationToken)
    {
        var entries = await _content.GetEntriesAsync(
            SchoolContentTypes.Event, includeUnpublished, cancellationToken);
        return entries.Select(ToEvent)
            .OrderBy(x => x.StartsOn ?? DateTime.MaxValue)
            .ThenBy(x => x.DisplayOrder)
            .ToList();
    }

    public async Task<SchoolEventDto> GetEventAsync(Guid id, CancellationToken cancellationToken) =>
        ToEvent(await RequireEntryAsync(id, SchoolContentTypes.Event, "Event", cancellationToken));

    public async Task<SchoolEventDto?> GetEventByKeyAsync(string key, CancellationToken cancellationToken)
    {
        try
        {
            var entry = await _content.GetEntryByKeyAsync(
                SchoolContentTypes.Event, key, includeInactive: false, cancellationToken);
            return ToEvent(entry);
        }
        catch (NotFoundException)
        {
            return null;
        }
    }

    public async Task<SchoolEventDto> SaveEventAsync(
        Guid? id, SaveSchoolEventDto dto, CancellationToken cancellationToken)
    {
        dto.Title = dto.Title.Trim();
        await _eventValidator.ValidateAndThrowAsync(dto, cancellationToken);

        var json = new JsonObject
        {
            ["endsOn"] = dto.EndsOn?.ToString("O"),
            ["venue"] = Clean(dto.Venue),
            ["registrationUrl"] = Clean(dto.RegistrationUrl)
        };

        var saved = await _content.SaveEntryAsync(id, new SaveContentEntryDto
        {
            ContentType = SchoolContentTypes.Event,
            Key = await ResolveKeyAsync(id, dto.Key, dto.Title, SchoolContentTypes.Event, cancellationToken),
            Title = dto.Title,
            Summary = Clean(dto.Summary),
            Body = dto.Body,
            ImageUrl = Clean(dto.ImageUrl),
            JsonData = json.ToJsonString(),
            DisplayOrder = dto.DisplayOrder,
            IsActive = dto.IsPublished,
            PublishDate = dto.StartsOn
        }, cancellationToken);

        return ToEvent(saved);
    }

    public Task DeleteEventAsync(Guid id, CancellationToken cancellationToken) =>
        DeleteAsync(id, SchoolContentTypes.Event, "Event", cancellationToken);

    // -----------------------------------------------------------------------
    // Departments
    // -----------------------------------------------------------------------

    public async Task<IReadOnlyList<DepartmentDto>> GetDepartmentsAsync(
        bool includeUnpublished, CancellationToken cancellationToken)
    {
        var entries = await _content.GetEntriesAsync(
            SchoolContentTypes.Department, includeUnpublished, cancellationToken);
        return entries.Select(ToDepartment)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    public async Task<DepartmentDto> GetDepartmentAsync(Guid id, CancellationToken cancellationToken) =>
        ToDepartment(await RequireEntryAsync(id, SchoolContentTypes.Department, "Department", cancellationToken));

    public async Task<DepartmentDto> SaveDepartmentAsync(
        Guid? id, SaveDepartmentDto dto, CancellationToken cancellationToken)
    {
        dto.Name = dto.Name.Trim();
        await _departmentValidator.ValidateAndThrowAsync(dto, cancellationToken);

        var programmes = SplitLines(dto.Programmes);
        var json = new JsonObject
        {
            ["headOfDepartment"] = Clean(dto.HeadOfDepartment),
            ["email"] = Clean(dto.Email),
            ["phone"] = Clean(dto.Phone),
            ["programmes"] = new JsonArray(programmes.Select(x => (JsonNode)JsonValue.Create(x)!).ToArray())
        };

        var saved = await _content.SaveEntryAsync(id, new SaveContentEntryDto
        {
            ContentType = SchoolContentTypes.Department,
            Key = await ResolveKeyAsync(id, dto.Key, dto.Name, SchoolContentTypes.Department, cancellationToken),
            Title = dto.Name,
            Summary = Clean(dto.Summary),
            Body = dto.Overview,
            ImageUrl = Clean(dto.ImageUrl),
            JsonData = json.ToJsonString(),
            DisplayOrder = dto.DisplayOrder,
            IsActive = dto.IsPublished
        }, cancellationToken);

        return ToDepartment(saved);
    }

    public Task DeleteDepartmentAsync(Guid id, CancellationToken cancellationToken) =>
        DeleteAsync(id, SchoolContentTypes.Department, "Department", cancellationToken);

    // -----------------------------------------------------------------------
    // Site settings
    // -----------------------------------------------------------------------

    public async Task<SiteSettingsDto> GetSettingsAsync(CancellationToken cancellationToken)
    {
        var entry = await FindSettingsEntryAsync(cancellationToken);
        if (entry is null)
        {
            return new SiteSettingsDto();
        }

        var json = ParseJson(entry.JsonData);
        return new SiteSettingsDto
        {
            NoticeTicker = String(json, "noticeTicker"),
            AdmissionStatus = Enum.TryParse<AdmissionStatus>(String(json, "admissionStatus"), true, out var status)
                ? status
                : AdmissionStatus.Closed,
            AdmissionsEmail = String(json, "admissionsEmail"),
            AdmissionsPhone = String(json, "admissionsPhone"),
            BrochureUrl = String(json, "brochureUrl"),
            ApplicationUrl = String(json, "applicationUrl"),
            OfficeHours = String(json, "officeHours"),
            WhatsAppNumber = String(json, "whatsAppNumber"),
            Facebook = String(json, "facebook"),
            Instagram = String(json, "instagram"),
            YouTube = String(json, "youTube"),
            Twitter = String(json, "twitter"),
            LinkedIn = String(json, "linkedIn"),

            NoticeTickerScrolls = Boolean(json, "noticeTickerScrolls"),
            NoticeTickerSeconds = Number(json, "noticeTickerSeconds") ?? 0,
            LogoHeight = Number(json, "logoHeight") ?? 0,
            ScrollAnimations = Boolean(json, "scrollAnimations", true),
            NoticeBarColor = String(json, "noticeBarColor"),
            ButtonStyle = String(json, "buttonStyle"),
            ButtonShape = String(json, "buttonShape"),
            ButtonHover = String(json, "buttonHover"),
            CardHover = String(json, "cardHover"),

            PopupEnabled = Boolean(json, "popupEnabled"),
            PopupImageUrl = String(json, "popupImageUrl"),
            PopupSlideSeconds = Number(json, "popupSlideSeconds") ?? 0,
            PopupAutoCloseSeconds = Number(json, "popupAutoCloseSeconds") ?? 0,
            PopupHeading = String(json, "popupHeading"),
            PopupLinkUrl = String(json, "popupLinkUrl"),
            PopupShowEnquiryForm = Boolean(json, "popupShowEnquiryForm"),
            PopupFormHeading = String(json, "popupFormHeading"),
            PopupOncePerVisit = Boolean(json, "popupOncePerVisit", true)
        };
    }

    public async Task<SiteSettingsDto> SaveSettingsAsync(SiteSettingsDto dto, CancellationToken cancellationToken)
    {
        await _settingsValidator.ValidateAndThrowAsync(dto, cancellationToken);

        var json = new JsonObject
        {
            ["noticeTicker"] = Clean(dto.NoticeTicker),
            ["admissionStatus"] = dto.AdmissionStatus.ToString(),
            ["admissionsEmail"] = Clean(dto.AdmissionsEmail),
            ["admissionsPhone"] = Clean(dto.AdmissionsPhone),
            ["brochureUrl"] = Clean(dto.BrochureUrl),
            ["applicationUrl"] = Clean(dto.ApplicationUrl),
            ["officeHours"] = Clean(dto.OfficeHours),
            ["whatsAppNumber"] = Clean(dto.WhatsAppNumber),
            ["facebook"] = Clean(dto.Facebook),
            ["instagram"] = Clean(dto.Instagram),
            ["youTube"] = Clean(dto.YouTube),
            ["twitter"] = Clean(dto.Twitter),
            ["linkedIn"] = Clean(dto.LinkedIn),

            ["noticeTickerScrolls"] = dto.NoticeTickerScrolls,
            ["noticeTickerSeconds"] = dto.NoticeTickerSeconds,
            ["logoHeight"] = dto.LogoHeight,
            ["scrollAnimations"] = dto.ScrollAnimations,
            ["noticeBarColor"] = Clean(dto.NoticeBarColor),
            ["buttonStyle"] = Clean(dto.ButtonStyle),
            ["buttonShape"] = Clean(dto.ButtonShape),
            ["buttonHover"] = Clean(dto.ButtonHover),
            ["cardHover"] = Clean(dto.CardHover),

            ["popupEnabled"] = dto.PopupEnabled,
            ["popupImageUrl"] = Clean(dto.PopupImageUrl),
            ["popupSlideSeconds"] = dto.PopupSlideSeconds,
            ["popupAutoCloseSeconds"] = dto.PopupAutoCloseSeconds,
            ["popupHeading"] = Clean(dto.PopupHeading),
            ["popupLinkUrl"] = Clean(dto.PopupLinkUrl),
            ["popupShowEnquiryForm"] = dto.PopupShowEnquiryForm,
            ["popupFormHeading"] = Clean(dto.PopupFormHeading),
            ["popupOncePerVisit"] = dto.PopupOncePerVisit
        };

        var existing = await FindSettingsEntryAsync(cancellationToken);
        await _content.SaveEntryAsync(existing?.Id, new SaveContentEntryDto
        {
            ContentType = SchoolContentTypes.Setting,
            Key = SchoolContentTypes.SettingsKey,
            Title = "Site settings",
            JsonData = json.ToJsonString(),
            IsActive = true
        }, cancellationToken);

        return await GetSettingsAsync(cancellationToken);
    }

    private async Task<ContentEntryDto?> FindSettingsEntryAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await _content.GetEntryByKeyAsync(
                SchoolContentTypes.Setting, SchoolContentTypes.SettingsKey, true, cancellationToken);
        }
        catch (NotFoundException)
        {
            return null;
        }
    }

    // -----------------------------------------------------------------------
    // Mapping
    // -----------------------------------------------------------------------

    private static FacultyMemberDto ToFaculty(ContentEntryDto entry)
    {
        var json = ParseJson(entry.JsonData);
        return new FacultyMemberDto
        {
            Id = entry.Id,
            Key = entry.Key,
            FullName = entry.Title,
            Designation = String(json, "designation"),
            Department = String(json, "department"),
            Category = Enum.TryParse<FacultyCategory>(String(json, "category"), true, out var category)
                ? category
                : FacultyCategory.Teaching,
            Qualification = String(json, "qualification"),
            ExperienceYears = Number(json, "experienceYears"),
            Email = String(json, "email"),
            Phone = String(json, "phone"),
            PhotoUrl = entry.ImageUrl,
            Headline = entry.Summary,
            Biography = entry.Body,
            DisplayOrder = entry.DisplayOrder,
            IsPublished = entry.IsActive
        };
    }

    private static NewsArticleDto ToNews(ContentEntryDto entry)
    {
        var json = ParseJson(entry.JsonData);
        return new NewsArticleDto
        {
            Id = entry.Id,
            Key = entry.Key,
            Headline = entry.Title,
            Category = Enum.TryParse<NewsCategory>(String(json, "category"), true, out var category)
                ? category
                : NewsCategory.News,
            PublishDate = entry.PublishDate,
            Summary = entry.Summary,
            Body = entry.Body,
            ImageUrl = entry.ImageUrl,
            AttachmentUrl = String(json, "attachmentUrl"),
            IsFeatured = Boolean(json, "isFeatured"),
            DisplayOrder = entry.DisplayOrder,
            IsPublished = entry.IsActive
        };
    }

    private static DepartmentDto ToDepartment(ContentEntryDto entry)
    {
        var json = ParseJson(entry.JsonData);
        return new DepartmentDto
        {
            Id = entry.Id,
            Key = entry.Key,
            Name = entry.Title,
            HeadOfDepartment = String(json, "headOfDepartment"),
            Summary = entry.Summary,
            Overview = entry.Body,
            ImageUrl = entry.ImageUrl,
            Email = String(json, "email"),
            Phone = String(json, "phone"),
            Programmes = StringArray(json, "programmes"),
            DisplayOrder = entry.DisplayOrder,
            IsPublished = entry.IsActive
        };
    }

    private static SchoolEventDto ToEvent(ContentEntryDto entry)
    {
        var json = ParseJson(entry.JsonData);
        return new SchoolEventDto
        {
            Id = entry.Id,
            Key = entry.Key,
            Title = entry.Title,
            StartsOn = entry.PublishDate,
            EndsOn = DateTimeValue(json, "endsOn"),
            Venue = String(json, "venue"),
            Summary = entry.Summary,
            Body = entry.Body,
            ImageUrl = entry.ImageUrl,
            RegistrationUrl = String(json, "registrationUrl"),
            DisplayOrder = entry.DisplayOrder,
            IsPublished = entry.IsActive
        };
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private async Task<ContentEntryDto> RequireEntryAsync(
        Guid id, string expectedType, string label, CancellationToken cancellationToken)
    {
        var entry = await _content.GetEntryAsync(id, cancellationToken);
        if (!string.Equals(entry.ContentType, expectedType, StringComparison.OrdinalIgnoreCase))
        {
            throw new NotFoundException($"{label} was not found.");
        }

        return entry;
    }

    private async Task DeleteAsync(
        Guid id, string expectedType, string label, CancellationToken cancellationToken)
    {
        await RequireEntryAsync(id, expectedType, label, cancellationToken);
        await _content.DeleteEntryAsync(id, cancellationToken);
    }

    /// <summary>
    /// Keys are the public URL segment. An author never has to invent one: it is derived from
    /// the title and made unique, and an existing entry keeps the key it was published under.
    /// </summary>
    private async Task<string> ResolveKeyAsync(
        Guid? id, string? requestedKey, string title, string type, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(requestedKey))
        {
            return Slug(requestedKey);
        }

        if (id.HasValue)
        {
            var existing = await _content.GetEntryAsync(id.Value, cancellationToken);
            if (!string.IsNullOrWhiteSpace(existing.Key))
            {
                return existing.Key;
            }
        }

        var baseKey = Slug(title);
        if (baseKey.Length == 0)
        {
            baseKey = type;
        }

        var taken = (await _content.GetEntriesAsync(type, true, cancellationToken))
            .Where(x => x.Id != id)
            .Select(x => x.Key)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (!taken.Contains(baseKey))
        {
            return baseKey;
        }

        for (var suffix = 2; suffix < 1000; suffix++)
        {
            var candidate = $"{baseKey}-{suffix}";
            if (!taken.Contains(candidate))
            {
                return candidate;
            }
        }

        return $"{baseKey}-{Guid.NewGuid():N}"[..64];
    }

    private static string Slug(string value)
    {
        var builder = new StringBuilder(value.Length);
        var lastWasSeparator = false;
        foreach (var character in value.Trim().ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(character) && character < 128)
            {
                builder.Append(character);
                lastWasSeparator = false;
            }
            else if (!lastWasSeparator && builder.Length > 0)
            {
                builder.Append('-');
                lastWasSeparator = true;
            }
        }

        return builder.ToString().Trim('-');
    }

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static JsonElement? ParseJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            return document.RootElement.ValueKind == JsonValueKind.Object
                ? document.RootElement.Clone()
                : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string? String(JsonElement? root, string property) =>
        root is { } element && element.TryGetProperty(property, out var value)
            && value.ValueKind == JsonValueKind.String
            && !string.IsNullOrWhiteSpace(value.GetString())
                ? value.GetString()
                : null;

    private static int? Number(JsonElement? root, string property) =>
        root is { } element && element.TryGetProperty(property, out var value)
            && value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number)
                ? number
                : null;

    private static bool Boolean(JsonElement? root, string property) =>
        root is { } element && element.TryGetProperty(property, out var value)
            && value.ValueKind == JsonValueKind.True;

    /// <summary>
    /// A flag added after sites already existed has no entry in their settings, and reading that
    /// silence as "off" would switch off behaviour those sites already have. The caller says what
    /// absence means.
    /// </summary>
    private static bool Boolean(JsonElement? root, string property, bool whenAbsent) =>
        root is { } element && element.TryGetProperty(property, out var value)
            && value.ValueKind is JsonValueKind.True or JsonValueKind.False
                ? value.ValueKind == JsonValueKind.True
                : whenAbsent;

    private static IReadOnlyList<string> StringArray(JsonElement? root, string property)
    {
        if (root is not { } element
            || !element.TryGetProperty(property, out var value)
            || value.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return value.EnumerateArray()
            .Where(x => x.ValueKind == JsonValueKind.String)
            .Select(x => x.GetString()!)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();
    }

    private static IReadOnlyList<string> SplitLines(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? []
            : value.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

    private static DateTime? DateTimeValue(JsonElement? root, string property) =>
        DateTime.TryParse(String(root, property), out var parsed) ? parsed : null;
}
