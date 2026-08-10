using Cms.Application.DTOs.Websites;
using Cms.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Cms.Admin.Areas.CMS.Pages.Contacts;

public sealed class IndexModel : PageModel
{
    private readonly IWebsiteService _service;

    public IndexModel(IWebsiteService service) => _service = service;

    public IReadOnlyList<ContactSubmissionDto> Submissions { get; private set; } = [];

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task OnGetAsync(CancellationToken cancellationToken) =>
        Submissions = await _service.GetContactSubmissionsAsync(cancellationToken);

    public async Task<IActionResult> OnPostMarkReadAsync(Guid id, bool isRead, CancellationToken cancellationToken)
    {
        await _service.MarkContactReadAsync(id, isRead, cancellationToken);
        StatusMessage = isRead ? "Marked as read." : "Marked as unread.";
        return RedirectToPage();
    }
}
