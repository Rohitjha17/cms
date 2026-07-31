using Cms.Application.DTOs.Content;
using Cms.Application.Interfaces;
using Cms.Domain.Constants;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Cms.Admin.Areas.CMS.Pages.Content;

public sealed class IndexModel : PageModel
{
    private static readonly HashSet<string> AllowedTypes =
        new(["event", "news", "person", "department", "setting", "theme"], StringComparer.OrdinalIgnoreCase);
    private readonly ISiteContentService _service;
    private readonly IValidator<SaveContentEntryDto> _validator;

    public IndexModel(ISiteContentService service, IValidator<SaveContentEntryDto> validator)
    {
        _service = service;
        _validator = validator;
    }

    [BindProperty(SupportsGet = true)] public string Type { get; set; } = "news";
    [BindProperty] public Guid? EditId { get; set; }
    [BindProperty] public SaveContentEntryDto Input { get; set; } = new();
    public IReadOnlyList<ContentEntryDto> Entries { get; private set; } = [];
    public bool CanDelete => User.IsInRole(AppRoles.SuperAdmin) || User.IsInRole(AppRoles.TenantAdmin);
    public string TypeLabel => Type switch
    {
        "person" => "People",
        "setting" => "Site settings",
        _ => char.ToUpperInvariant(Type[0]) + Type[1..] + (Type.EndsWith('s') ? "" : "s")
    };
    [TempData] public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid? edit, CancellationToken cancellationToken)
    {
        if (!AllowedTypes.Contains(Type)) return NotFound();
        Type = Type.ToLowerInvariant();
        await LoadAsync(cancellationToken);
        if (edit.HasValue)
        {
            var entry = await _service.GetEntryAsync(edit.Value, cancellationToken);
            if (!string.Equals(entry.ContentType, Type, StringComparison.OrdinalIgnoreCase)) return NotFound();
            EditId = entry.Id;
            Input = new SaveContentEntryDto
            {
                ContentType = entry.ContentType, Key = entry.Key, Title = entry.Title,
                Summary = entry.Summary, Body = entry.Body, ImageUrl = entry.ImageUrl,
                JsonData = entry.JsonData, DisplayOrder = entry.DisplayOrder,
                IsActive = entry.IsActive, PublishDate = entry.PublishDate
            };
        }
        else
        {
            Input.ContentType = Type;
        }
        return Page();
    }

    public async Task<IActionResult> OnPostSaveAsync(CancellationToken cancellationToken)
    {
        Input.ContentType = Type.ToLowerInvariant();
        var validation = await _validator.ValidateAsync(Input, cancellationToken);
        if (!validation.IsValid)
        {
            foreach (var error in validation.Errors) ModelState.AddModelError(string.Empty, error.ErrorMessage);
            await LoadAsync(cancellationToken);
            return Page();
        }
        await _service.SaveEntryAsync(EditId, Input, cancellationToken);
        StatusMessage = $"{TypeLabel} content saved.";
        return RedirectToPage(new { type = Type });
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        if (!CanDelete) return Forbid();
        await _service.DeleteEntryAsync(id, cancellationToken);
        StatusMessage = "Content entry deleted.";
        return RedirectToPage(new { type = Type });
    }

    private async Task LoadAsync(CancellationToken cancellationToken) =>
        Entries = await _service.GetEntriesAsync(Type, true, cancellationToken);
}
