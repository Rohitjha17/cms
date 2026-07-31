using Cms.Application.DTOs.Content;
using Cms.Application.Interfaces;
using Cms.Domain.Constants;
using Cms.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cms.Api.Controllers;

[ApiController]
[Route("api/navigation")]
public sealed class NavigationController : ControllerBase
{
    private readonly ISiteContentService _service;

    public NavigationController(ISiteContentService service) => _service = service;

    [HttpGet("{location}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<MenuDto>>> Get(string location, CancellationToken cancellationToken)
    {
        var data = await _service.GetMenuByLocationAsync(location, cancellationToken);
        return Ok(ApiResponse<MenuDto>.Ok(data));
    }

    [HttpGet]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.TenantAdmin},{AppRoles.Editor}")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MenuDto>>>> GetAll(CancellationToken cancellationToken)
    {
        var data = await _service.GetMenusAsync(true, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<MenuDto>>.Ok(data));
    }

    [HttpPost]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.TenantAdmin},{AppRoles.Editor}")]
    public async Task<ActionResult<ApiResponse<MenuDto>>> Create(SaveMenuDto dto, CancellationToken cancellationToken)
    {
        var data = await _service.SaveMenuAsync(null, dto, cancellationToken);
        return StatusCode(201, ApiResponse<MenuDto>.Created(data));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.TenantAdmin},{AppRoles.Editor}")]
    public async Task<ActionResult<ApiResponse<MenuDto>>> Update(Guid id, SaveMenuDto dto, CancellationToken cancellationToken)
    {
        var data = await _service.SaveMenuAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<MenuDto>.Ok(data, "Menu updated."));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.TenantAdmin}")]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _service.DeleteMenuAsync(id, cancellationToken);
        return Ok(ApiResponse.Ok("Menu deleted."));
    }
}
