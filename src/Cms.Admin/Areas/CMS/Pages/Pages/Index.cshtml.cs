using Cms.Application.DTOs.Content;
using Cms.Application.Interfaces;
using Cms.Domain.Constants;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Cms.Admin.Areas.CMS.Pages.Pages;

public sealed class IndexModel : PageModel
{
    private readonly ISiteContentService _service;
    private readonly IValidator<SavePageDto> _validator;

    public IndexModel(ISiteContentService service, IValidator<SavePageDto> validator)
    {
        _service = service;
        _validator = validator;
    }

    public IReadOnlyList<PageDto> Pages { get; private set; } = [];
    public bool CanDelete => User.IsInRole(AppRoles.SuperAdmin) || User.IsInRole(AppRoles.TenantAdmin);

    [BindProperty]
    public Guid? EditId { get; set; }

    [BindProperty]
    public SavePageDto Input { get; set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task OnGetAsync(Guid? edit, CancellationToken cancellationToken)
    {
        await LoadAsync(cancellationToken);
        if (edit.HasValue)
        {
            var page = await _service.GetPageAsync(edit.Value, cancellationToken);
            EditId = page.Id;
            Input = new SavePageDto
            {
                Title = page.Title, Slug = page.Slug, Excerpt = page.Excerpt,
                Content = page.Content, FeaturedImageUrl = page.FeaturedImageUrl,
                MetaTitle = page.MetaTitle, MetaDescription = page.MetaDescription,
                IsActive = page.IsActive
            };
        }
    }

    public async Task<IActionResult> OnPostSaveAsync(CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(Input, cancellationToken);
        if (!validation.IsValid)
        {
            foreach (var error in validation.Errors)
            {
                ModelState.AddModelError($"Input.{error.PropertyName}", error.ErrorMessage);
            }
            await LoadAsync(cancellationToken);
            return Page();
        }

        await _service.SavePageAsync(EditId, Input, cancellationToken);
        StatusMessage = EditId.HasValue ? "Page updated." : "Page created.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        if (!CanDelete) return Forbid();
        await _service.DeletePageAsync(id, cancellationToken);
        StatusMessage = "Page deleted.";
        return RedirectToPage();
    }

    private async Task LoadAsync(CancellationToken cancellationToken) =>
        Pages = await _service.GetPagesAsync(true, cancellationToken);
}
