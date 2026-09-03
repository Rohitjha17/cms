using Cms.Admin.Filters;
using Cms.Application.DTOs.SchoolContent;
using Cms.Application.Interfaces;
using Cms.Domain.Constants;
using Cms.Shared.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Cms.Admin.Areas.CMS.Pages.Departments;

public sealed class IndexModel : PageModel, IReloadablePage
{
    private readonly ISchoolContentService _service;

    public IndexModel(ISchoolContentService service) => _service = service;

    [BindProperty] public Guid? EditId { get; set; }
    [BindProperty] public SaveDepartmentDto Input { get; set; } = new();

    public IReadOnlyList<DepartmentDto> Departments { get; private set; } = [];
    public bool CanDelete => User.IsInRole(AppRoles.SuperAdmin) || User.IsInRole(AppRoles.TenantAdmin);

    [TempData] public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid? edit, CancellationToken cancellationToken)
    {
        await LoadAsync(cancellationToken);

        if (edit.HasValue)
        {
            DepartmentDto department;
            try
            {
                department = await _service.GetDepartmentAsync(edit.Value, cancellationToken);
            }
            catch (NotFoundException)
            {
                return NotFound();
            }

            EditId = department.Id;
            Input = new SaveDepartmentDto
            {
                Key = department.Key,
                Name = department.Name,
                HeadOfDepartment = department.HeadOfDepartment,
                Summary = department.Summary,
                Overview = department.Overview,
                ImageUrl = department.ImageUrl,
                Email = department.Email,
                Phone = department.Phone,
                Programmes = string.Join(Environment.NewLine, department.Programmes),
                DisplayOrder = department.DisplayOrder,
                IsPublished = department.IsPublished
            };
        }
        else
        {
            Input.DisplayOrder = Departments.Count;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostSaveAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _service.SaveDepartmentAsync(EditId, Input, cancellationToken);
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

        StatusMessage = EditId.HasValue ? "Department updated." : "Department added.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        if (!CanDelete)
        {
            return Forbid();
        }

        await _service.DeleteDepartmentAsync(id, cancellationToken);
        StatusMessage = "Department removed.";
        return RedirectToPage();
    }


    /// <summary>
    /// Refetches the lists when a save or a removal is refused. Without it the page comes back
    /// with the error beside an empty table, which reads as though the refused action destroyed
    /// everything — the opposite of what happened.
    /// </summary>
    public Task ReloadAsync(CancellationToken cancellationToken) => LoadAsync(cancellationToken);
    private async Task LoadAsync(CancellationToken cancellationToken) =>
        Departments = await _service.GetDepartmentsAsync(includeUnpublished: true, cancellationToken);
}
