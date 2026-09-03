using Cms.Admin.Filters;
using Cms.Application.DTOs.SchoolContent;
using Cms.Application.Interfaces;
using Cms.Domain.Constants;
using Cms.Shared.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Cms.Admin.Areas.CMS.Pages.Events;

public sealed class IndexModel : PageModel, IReloadablePage
{
    private readonly ISchoolContentService _service;

    public IndexModel(ISchoolContentService service) => _service = service;

    [BindProperty] public Guid? EditId { get; set; }
    [BindProperty] public SaveSchoolEventDto Input { get; set; } = new();

    public IReadOnlyList<SchoolEventDto> Events { get; private set; } = [];
    public bool CanDelete => User.IsInRole(AppRoles.SuperAdmin) || User.IsInRole(AppRoles.TenantAdmin);

    [TempData] public string? StatusMessage { get; set; }

    public IReadOnlyList<SchoolEventDto> Upcoming =>
        Events.Where(x => !x.HasFinished(DateTime.UtcNow)).ToList();

    public IReadOnlyList<SchoolEventDto> Past =>
        Events.Where(x => x.HasFinished(DateTime.UtcNow)).Reverse().ToList();

    public async Task<IActionResult> OnGetAsync(Guid? edit, CancellationToken cancellationToken)
    {
        await LoadAsync(cancellationToken);

        if (edit.HasValue)
        {
            SchoolEventDto item;
            try
            {
                item = await _service.GetEventAsync(edit.Value, cancellationToken);
            }
            catch (NotFoundException)
            {
                return NotFound();
            }

            EditId = item.Id;
            Input = new SaveSchoolEventDto
            {
                Key = item.Key,
                Title = item.Title,
                StartsOn = item.StartsOn,
                EndsOn = item.EndsOn,
                Venue = item.Venue,
                Summary = item.Summary,
                Body = item.Body,
                ImageUrl = item.ImageUrl,
                RegistrationUrl = item.RegistrationUrl,
                DisplayOrder = item.DisplayOrder,
                IsPublished = item.IsPublished
            };
        }
        else
        {
            Input.StartsOn = DateTime.UtcNow.Date.AddDays(1).AddHours(9);
        }

        return Page();
    }

    public async Task<IActionResult> OnPostSaveAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _service.SaveEventAsync(EditId, Input, cancellationToken);
        }
        catch (ValidationException exception)
        {
            foreach (var error in exception.Errors)
            {
                ModelState.AddModelError(string.Empty, error.ErrorMessage);
            }

            await LoadAsync(cancellationToken);
            return Page();
        }
        catch (ValidationAppException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            await LoadAsync(cancellationToken);
            return Page();
        }

        StatusMessage = EditId.HasValue ? "Event updated." : "Event added to the calendar.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        if (!CanDelete)
        {
            return Forbid();
        }

        await _service.DeleteEventAsync(id, cancellationToken);
        StatusMessage = "Event removed.";
        return RedirectToPage();
    }


    /// <summary>
    /// Refetches the lists when a save or a removal is refused. Without it the page comes back
    /// with the error beside an empty table, which reads as though the refused action destroyed
    /// everything — the opposite of what happened.
    /// </summary>
    public Task ReloadAsync(CancellationToken cancellationToken) => LoadAsync(cancellationToken);
    private async Task LoadAsync(CancellationToken cancellationToken) =>
        Events = await _service.GetEventsAsync(includeUnpublished: true, cancellationToken);
}
