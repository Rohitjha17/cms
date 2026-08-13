using Cms.Admin.Filters;
using Cms.Application.DTOs.Websites;
using Cms.Application.Interfaces;
using Cms.Domain.Constants;
using Cms.Domain.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Cms.Admin.Areas.CMS.Pages.PageGallery;

public sealed class IndexModel : PageModel, IReloadablePage
{
    private readonly IWebsiteService _service;
    private readonly IValidator<SavePageTemplateDto> _validator;
    private readonly IValidator<AssignTemplatesDto> _assignValidator;

    public IndexModel(
        IWebsiteService service,
        IValidator<SavePageTemplateDto> validator,
        IValidator<AssignTemplatesDto> assignValidator)
    {
        _service = service;
        _validator = validator;
        _assignValidator = assignValidator;
    }

    public IReadOnlyList<PageTemplateDto> Templates { get; private set; } = [];
    public bool CanManage => User.IsInRole(AppRoles.SuperAdmin) || User.IsInRole(AppRoles.TenantAdmin);

    [BindProperty] public Guid? EditId { get; set; }
    [BindProperty] public SavePageTemplateDto Input { get; set; } = new() { PageType = PageType.Custom, IsStarter = true, IsActive = true };
    [BindProperty] public List<string> AssignKeys { get; set; } = [];
    [TempData] public string? StatusMessage { get; set; }

    public async Task OnGetAsync(Guid? edit, CancellationToken cancellationToken)
    {
        await LoadAsync(cancellationToken);
        if (edit.HasValue)
        {
            var template = Templates.FirstOrDefault(x => x.Id == edit.Value);
            if (template is not null)
            {
                EditId = template.Id;
                Input = new SavePageTemplateDto
                {
                    TemplateKey = template.TemplateKey,
                    Name = template.Name,
                    Description = template.Description,
                    PageType = template.PageType,
                    DefaultSlug = template.DefaultSlug,
                    DefaultTitle = template.DefaultTitle,
                    DefaultContent = template.DefaultContent,
                    DefaultJsonData = template.DefaultJsonData,
                    IsStarter = template.IsStarter,
                    IsActive = template.IsActive,
                    DisplayOrder = template.DisplayOrder
                };
            }
        }
    }

    public async Task<IActionResult> OnPostSaveAsync(CancellationToken cancellationToken)
    {
        if (!CanManage) return Forbid();
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

        await _service.SavePageTemplateAsync(EditId, Input, cancellationToken);
        StatusMessage = EditId.HasValue ? "Page template updated." : "Page template added to the gallery.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAssignAsync(CancellationToken cancellationToken)
    {
        if (!CanManage) return Forbid();
        var dto = new AssignTemplatesDto { TemplateKeys = AssignKeys };
        var validation = await _assignValidator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
        {
            foreach (var error in validation.Errors)
            {
                ModelState.AddModelError(string.Empty, error.ErrorMessage);
            }
            await LoadAsync(cancellationToken);
            return Page();
        }

        var created = await _service.AssignTemplatesAsync(dto, cancellationToken);
        StatusMessage = created.Count == 0
            ? "Selected templates were already assigned to this website."
            : $"Assigned {created.Count} page(s) to the current website.";
        return RedirectToPage();
    }

    /// <summary>Refetches the lists when a failed save redisplays this page.</summary>
    public Task ReloadAsync(CancellationToken cancellationToken) => LoadAsync(cancellationToken);

    private async Task LoadAsync(CancellationToken cancellationToken) =>
        Templates = await _service.GetPageTemplatesAsync(cancellationToken);
}
