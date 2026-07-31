using Cms.Application.DTOs.Content;
using Cms.Application.Interfaces;
using Cms.Domain.Constants;
using Cms.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cms.Api.Controllers;

[ApiController]
[Route("api/pages")]
public sealed class PagesController : ControllerBase
{
    private readonly ISiteContentService _service;

    public PagesController(ISiteContentService service) => _service = service;

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PageDto>>>> GetAll(
        [FromQuery] bool includeInactive,
        CancellationToken cancellationToken)
    {
        includeInactive = includeInactive && CanManage();
        var data = await _service.GetPagesAsync(includeInactive, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<PageDto>>.Ok(data));
    }

    [HttpGet("{slug}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<PageDto>>> GetBySlug(string slug, CancellationToken cancellationToken)
    {
        var data = await _service.GetPageBySlugAsync(slug, false, cancellationToken);
        return Ok(ApiResponse<PageDto>.Ok(data));
    }

    [HttpPost]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.TenantAdmin},{AppRoles.Editor}")]
    public async Task<ActionResult<ApiResponse<PageDto>>> Create(SavePageDto dto, CancellationToken cancellationToken)
    {
        var data = await _service.SavePageAsync(null, dto, cancellationToken);
        return StatusCode(201, ApiResponse<PageDto>.Created(data));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.TenantAdmin},{AppRoles.Editor}")]
    public async Task<ActionResult<ApiResponse<PageDto>>> Update(Guid id, SavePageDto dto, CancellationToken cancellationToken)
    {
        var data = await _service.SavePageAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<PageDto>.Ok(data, "Page updated."));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.TenantAdmin}")]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _service.DeletePageAsync(id, cancellationToken);
        return Ok(ApiResponse.Ok("Page deleted."));
    }

    private bool CanManage() =>
        User.IsInRole(AppRoles.SuperAdmin)
        || User.IsInRole(AppRoles.TenantAdmin)
        || User.IsInRole(AppRoles.Editor);
}
