using Cms.Application.DTOs.Media;
using Cms.Application.Interfaces;
using Cms.Domain.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Cms.Admin.Areas.CMS.Pages.Media;

public sealed class IndexModel : PageModel
{
    private readonly IMediaService _mediaService;

    public IndexModel(IMediaService mediaService)
    {
        _mediaService = mediaService;
    }

    public IReadOnlyList<MediaFileDto> Files { get; private set; } = [];
    public bool CanDelete => User.IsInRole(AppRoles.SuperAdmin) || User.IsInRole(AppRoles.TenantAdmin);

    [BindProperty]
    public IFormFile? Upload { get; set; }

    [BindProperty]
    public string UploadKind { get; set; } = "image";

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task OnGetAsync(CancellationToken cancellationToken) =>
        Files = await _mediaService.GetAllAsync(cancellationToken: cancellationToken);

    public async Task<IActionResult> OnGetListAsync(string? type, CancellationToken cancellationToken)
    {
        var files = await _mediaService.GetAllAsync(type, cancellationToken);
        return new JsonResult(files.Select(file => new
        {
            id = file.Id,
            fileName = file.OriginalFileName,
            url = file.Url,
            mediaType = file.MediaType
        }));
    }

    public async Task<IActionResult> OnPostUploadAsync(CancellationToken cancellationToken)
    {
        if (Upload is null)
        {
            ModelState.AddModelError(nameof(Upload), "Choose a file to upload.");
            Files = await _mediaService.GetAllAsync(cancellationToken: cancellationToken);
            return Page();
        }

        if (string.Equals(UploadKind, "document", StringComparison.OrdinalIgnoreCase))
        {
            await _mediaService.UploadDocumentAsync(Upload, "documents", cancellationToken);
        }
        else
        {
            await _mediaService.UploadImageAsync(Upload, "media", cancellationToken);
        }

        StatusMessage = "Media uploaded successfully.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid mediaId, CancellationToken cancellationToken)
    {
        if (!CanDelete)
        {
            return Forbid();
        }

        await _mediaService.DeleteAsync(mediaId, cancellationToken);
        StatusMessage = "Media deleted.";
        return RedirectToPage();
    }
}
