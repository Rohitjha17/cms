using Cms.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Cms.Admin.ViewComponents;

/// <summary>
/// The topbar bell. It used to be a button that did nothing above a red dot that was always
/// lit — so it announced unread items that did not exist and went nowhere when clicked. It now
/// reports the enquiries nobody has opened for the website being edited, and links to them.
/// </summary>
public sealed class NotificationsViewComponent : ViewComponent
{
    private readonly IWebsiteService _websiteService;

    public NotificationsViewComponent(IWebsiteService websiteService) => _websiteService = websiteService;

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var unread = await _websiteService.GetUnreadContactCountAsync(HttpContext.RequestAborted);
        return View(unread);
    }
}
