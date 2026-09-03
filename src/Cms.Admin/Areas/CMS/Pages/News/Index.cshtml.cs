using Cms.Admin.Filters;
using Cms.Application.DTOs.SchoolContent;
using Cms.Application.Interfaces;
using Cms.Domain.Constants;
using Cms.Shared.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Cms.Admin.Areas.CMS.Pages.News;

public sealed class IndexModel : PageModel, IReloadablePage
{
    private readonly ISchoolContentService _service;

    public IndexModel(ISchoolContentService service) => _service = service;

    [BindProperty] public Guid? EditId { get; set; }
    [BindProperty] public SaveNewsArticleDto Input { get; set; } = new();

    public IReadOnlyList<NewsArticleDto> Articles { get; private set; } = [];
    public bool CanDelete => User.IsInRole(AppRoles.SuperAdmin) || User.IsInRole(AppRoles.TenantAdmin);

    [TempData] public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid? edit, CancellationToken cancellationToken)
    {
        await LoadAsync(cancellationToken);

        if (edit.HasValue)
        {
            NewsArticleDto article;
            try
            {
                article = await _service.GetNewsArticleAsync(edit.Value, cancellationToken);
            }
            catch (NotFoundException)
            {
                return NotFound();
            }

            EditId = article.Id;
            Input = new SaveNewsArticleDto
            {
                Key = article.Key,
                Headline = article.Headline,
                Category = article.Category,
                PublishDate = article.PublishDate,
                Summary = article.Summary,
                Body = article.Body,
                ImageUrl = article.ImageUrl,
                AttachmentUrl = article.AttachmentUrl,
                IsFeatured = article.IsFeatured,
                DisplayOrder = article.DisplayOrder,
                IsPublished = article.IsPublished
            };
        }
        else
        {
            Input.PublishDate = DateTime.UtcNow;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostSaveAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _service.SaveNewsArticleAsync(EditId, Input, cancellationToken);
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

        StatusMessage = EditId.HasValue ? "Update saved." : "Published.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        if (!CanDelete)
        {
            return Forbid();
        }

        await _service.DeleteNewsArticleAsync(id, cancellationToken);
        StatusMessage = "Removed.";
        return RedirectToPage();
    }


    /// <summary>
    /// Refetches the lists when a save or a removal is refused. Without it the page comes back
    /// with the error beside an empty table, which reads as though the refused action destroyed
    /// everything — the opposite of what happened.
    /// </summary>
    public Task ReloadAsync(CancellationToken cancellationToken) => LoadAsync(cancellationToken);
    private async Task LoadAsync(CancellationToken cancellationToken) =>
        Articles = await _service.GetNewsAsync(includeUnpublished: true, cancellationToken);
}
