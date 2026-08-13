using Cms.Admin.Filters;
using Cms.Application.DTOs.Content;
using Cms.Application.Interfaces;
using Cms.Domain.Constants;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Cms.Admin.Areas.CMS.Pages.Navigation;

public sealed class IndexModel : PageModel, IReloadablePage
{
    private readonly ISiteContentService _service;
    private readonly IValidator<SaveMenuDto> _validator;

    public IndexModel(ISiteContentService service, IValidator<SaveMenuDto> validator)
    {
        _service = service;
        _validator = validator;
    }

    public IReadOnlyList<MenuDto> Menus { get; private set; } = [];
    public bool CanDelete => User.IsInRole(AppRoles.SuperAdmin) || User.IsInRole(AppRoles.TenantAdmin);

    [BindProperty] public Guid? EditId { get; set; }
    [BindProperty] public SaveMenuDto Input { get; set; } = new();
    [TempData] public string? StatusMessage { get; set; }

    public async Task OnGetAsync(Guid? edit, CancellationToken cancellationToken)
    {
        await LoadAsync(cancellationToken);
        if (edit.HasValue)
        {
            var menu = await _service.GetMenuAsync(edit.Value, cancellationToken);
            EditId = menu.Id;
            Input = new SaveMenuDto
            {
                Name = menu.Name, Location = menu.Location, IsActive = menu.IsActive,
                Items = menu.Items.Select(x => new MenuItemDto
                {
                    Id = x.Id, Label = x.Label, Url = x.Url, Target = x.Target,
                    DisplayOrder = x.DisplayOrder, IsActive = x.IsActive
                }).ToList()
            };
        }
    }

    public async Task<IActionResult> OnPostSaveAsync(CancellationToken cancellationToken)
    {
        Input.Items = Input.Items.Select((item, index) =>
        {
            item.DisplayOrder = index + 1;
            return item;
        }).ToList();
        var validation = await _validator.ValidateAsync(Input, cancellationToken);
        if (!validation.IsValid)
        {
            foreach (var error in validation.Errors)
            {
                ModelState.AddModelError(string.Empty, error.ErrorMessage);
            }
            await LoadAsync(cancellationToken);
            return Page();
        }

        await _service.SaveMenuAsync(EditId, Input, cancellationToken);
        StatusMessage = "Navigation saved.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        if (!CanDelete) return Forbid();
        await _service.DeleteMenuAsync(id, cancellationToken);
        StatusMessage = "Menu deleted.";
        return RedirectToPage();
    }

    /// <summary>Refetches the lists when a failed save redisplays this page.</summary>
    public Task ReloadAsync(CancellationToken cancellationToken) => LoadAsync(cancellationToken);

    private async Task LoadAsync(CancellationToken cancellationToken) =>
        Menus = await _service.GetMenusAsync(true, cancellationToken);
}
