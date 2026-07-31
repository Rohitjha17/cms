using Cms.Application.DTOs.Content;
using Cms.Application.Interfaces;
using Cms.Domain.Constants;
using Cms.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cms.Api.Controllers;

[ApiController]
[Route("api/content")]
public sealed class ContentController : ControllerBase
{
    private readonly ISiteContentService _service;

    public ContentController(ISiteContentService service) => _service = service;

    [HttpGet("{type}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ContentEntryDto>>>> GetAll(
        string type,
        [FromQuery] bool includeInactive,
        CancellationToken cancellationToken)
    {
        includeInactive = includeInactive && CanManage();
        var data = await _service.GetEntriesAsync(type, includeInactive, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<ContentEntryDto>>.Ok(data));
    }

    [HttpGet("{type}/{key}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<ContentEntryDto>>> Get(
        string type,
        string key,
        CancellationToken cancellationToken)
    {
        var data = await _service.GetEntryByKeyAsync(type, key, false, cancellationToken);
        return Ok(ApiResponse<ContentEntryDto>.Ok(data));
    }

    [HttpPost]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.TenantAdmin},{AppRoles.Editor}")]
    public async Task<ActionResult<ApiResponse<ContentEntryDto>>> Create(
        SaveContentEntryDto dto,
        CancellationToken cancellationToken)
    {
        var data = await _service.SaveEntryAsync(null, dto, cancellationToken);
        return StatusCode(201, ApiResponse<ContentEntryDto>.Created(data));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.TenantAdmin},{AppRoles.Editor}")]
    public async Task<ActionResult<ApiResponse<ContentEntryDto>>> Update(
        Guid id,
        SaveContentEntryDto dto,
        CancellationToken cancellationToken)
    {
        var data = await _service.SaveEntryAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<ContentEntryDto>.Ok(data, "Content updated."));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.TenantAdmin}")]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _service.DeleteEntryAsync(id, cancellationToken);
        return Ok(ApiResponse.Ok("Content deleted."));
    }

    private bool CanManage() =>
        User.IsInRole(AppRoles.SuperAdmin)
        || User.IsInRole(AppRoles.TenantAdmin)
        || User.IsInRole(AppRoles.Editor);
}
