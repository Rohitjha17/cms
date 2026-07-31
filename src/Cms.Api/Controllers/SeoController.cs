using Cms.Application.DTOs.Content;
using Cms.Application.Interfaces;
using Cms.Domain.Constants;
using Cms.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cms.Api.Controllers;

[ApiController]
[Route("api/seo")]
public sealed class SeoController : ControllerBase
{
    private readonly ISiteContentService _service;

    public SeoController(ISiteContentService service) => _service = service;

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<SeoSettingDto>>> Get(CancellationToken cancellationToken) =>
        Ok(ApiResponse<SeoSettingDto>.Ok(await _service.GetSeoAsync(cancellationToken)));

    [HttpPut]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.TenantAdmin},{AppRoles.Editor}")]
    public async Task<ActionResult<ApiResponse<SeoSettingDto>>> Update(
        SeoSettingDto dto,
        CancellationToken cancellationToken) =>
        Ok(ApiResponse<SeoSettingDto>.Ok(
            await _service.SaveSeoAsync(dto, cancellationToken),
            "SEO settings updated."));
}
