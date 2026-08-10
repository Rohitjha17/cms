using Cms.Application.Interfaces;
using Cms.Domain.Constants;
using Cms.Domain.Entities;
using Cms.Infrastructure.Persistence;
using Cms.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cms.Api.Controllers;

[ApiController]
[Route("api/activity-logs")]
[Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.TenantAdmin}")]
public sealed class ActivityLogsController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ActivityLog>>>> Get(
        [FromServices] ApplicationDbContext db,
        [FromServices] ISiteContext siteContext,
        [FromQuery] int take = 100,
        CancellationToken cancellationToken = default)
    {
        take = Math.Clamp(take, 1, 200);
        var query = db.ActivityLogs.AsNoTracking();
        if (siteContext.SiteId.HasValue)
        {
            query = query.Where(x => x.SiteId == siteContext.SiteId.Value || x.SiteId == null);
        }

        var items = await query.OrderByDescending(x => x.CreatedDate).Take(take).ToListAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<ActivityLog>>.Ok(items));
    }
}
