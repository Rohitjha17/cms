using Cms.Application.DTOs.Tenancy;
using Cms.Application.Interfaces;
using Cms.Domain.Constants;
using Cms.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cms.Api.Controllers;

[ApiController]
[Route("api/tenants")]
[Authorize(Roles = AppRoles.SuperAdmin)]
public sealed class TenantsController : ControllerBase
{
    private readonly ITenantManagementService _service;

    public TenantsController(ITenantManagementService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<TenantManagementDto>>>> GetAll(
        CancellationToken cancellationToken) =>
        Ok(ApiResponse<IReadOnlyList<TenantManagementDto>>.Ok(
            await _service.GetAllAsync(cancellationToken)));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<TenantManagementDto>>> Get(
        Guid id, CancellationToken cancellationToken) =>
        Ok(ApiResponse<TenantManagementDto>.Ok(await _service.GetAsync(id, cancellationToken)));

    [HttpPost]
    public async Task<ActionResult<ApiResponse<TenantManagementDto>>> Create(
        SaveTenantDto dto, CancellationToken cancellationToken)
    {
        var data = await _service.SaveAsync(null, dto, cancellationToken);
        return StatusCode(201, ApiResponse<TenantManagementDto>.Created(data));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<TenantManagementDto>>> Update(
        Guid id, SaveTenantDto dto, CancellationToken cancellationToken) =>
        Ok(ApiResponse<TenantManagementDto>.Ok(
            await _service.SaveAsync(id, dto, cancellationToken),
            "Tenant updated."));
}
