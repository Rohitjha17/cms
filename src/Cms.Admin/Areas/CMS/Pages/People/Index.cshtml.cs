using Cms.Application.DTOs.SchoolContent;
using Cms.Application.Interfaces;
using Cms.Domain.Constants;
using Cms.Shared.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Cms.Admin.Areas.CMS.Pages.People;

public sealed class IndexModel : PageModel
{
    private readonly ISchoolContentService _service;

    public IndexModel(ISchoolContentService service) => _service = service;

    [BindProperty] public Guid? EditId { get; set; }
    [BindProperty] public SaveFacultyMemberDto Input { get; set; } = new();

    public IReadOnlyList<FacultyMemberDto> Members { get; private set; } = [];
    public bool CanDelete => User.IsInRole(AppRoles.SuperAdmin) || User.IsInRole(AppRoles.TenantAdmin);

    [TempData] public string? StatusMessage { get; set; }

    public IEnumerable<IGrouping<FacultyCategory, FacultyMemberDto>> Grouped =>
        Members.GroupBy(x => x.Category).OrderBy(x => x.Key);

    public static string CategoryLabel(FacultyCategory category) => category switch
    {
        FacultyCategory.Leadership => "Leadership",
        FacultyCategory.Teaching => "Teaching staff",
        FacultyCategory.Administration => "Administration",
        _ => "Support staff"
    };

    public async Task<IActionResult> OnGetAsync(Guid? edit, CancellationToken cancellationToken)
    {
        await LoadAsync(cancellationToken);

        if (edit.HasValue)
        {
            FacultyMemberDto member;
            try
            {
                member = await _service.GetFacultyMemberAsync(edit.Value, cancellationToken);
            }
            catch (NotFoundException)
            {
                return NotFound();
            }

            EditId = member.Id;
            Input = new SaveFacultyMemberDto
            {
                Key = member.Key,
                FullName = member.FullName,
                Designation = member.Designation,
                Department = member.Department,
                Category = member.Category,
                Qualification = member.Qualification,
                ExperienceYears = member.ExperienceYears,
                Email = member.Email,
                Phone = member.Phone,
                PhotoUrl = member.PhotoUrl,
                Headline = member.Headline,
                Biography = member.Biography,
                DisplayOrder = member.DisplayOrder,
                IsPublished = member.IsPublished
            };
        }
        else
        {
            Input.DisplayOrder = Members.Count;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostSaveAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _service.SaveFacultyMemberAsync(EditId, Input, cancellationToken);
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

        StatusMessage = EditId.HasValue ? "Staff member updated." : "Staff member added.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        if (!CanDelete)
        {
            return Forbid();
        }

        await _service.DeleteFacultyMemberAsync(id, cancellationToken);
        StatusMessage = "Staff member removed.";
        return RedirectToPage();
    }

    private async Task LoadAsync(CancellationToken cancellationToken) =>
        Members = await _service.GetFacultyAsync(includeUnpublished: true, cancellationToken);
}
