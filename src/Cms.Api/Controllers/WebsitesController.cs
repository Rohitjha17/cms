using Cms.Application.DTOs.Websites;
using Cms.Application.Interfaces;
using Cms.Domain.Constants;
using Cms.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Cms.Api.Controllers;

[ApiController]
[Route("api/websites")]
public sealed class WebsitesController : ControllerBase
{
    private readonly IWebsiteService _service;

    public WebsitesController(IWebsiteService service) => _service = service;

    [HttpGet("templates")]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.TenantAdmin},{AppRoles.Editor}")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PageTemplateDto>>>> Templates(CancellationToken cancellationToken)
    {
        var data = await _service.GetPageTemplatesAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<PageTemplateDto>>.Ok(data));
    }

    [HttpGet]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.TenantAdmin},{AppRoles.Editor}")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<WebsiteSummaryDto>>>> List(CancellationToken cancellationToken)
    {
        var data = await _service.GetWebsitesAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<WebsiteSummaryDto>>.Ok(data));
    }

    [HttpPost("provision")]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.TenantAdmin}")]
    public async Task<ActionResult<ApiResponse<WebsiteSummaryDto>>> Provision(
        ProvisionWebsiteDto dto, CancellationToken cancellationToken)
    {
        var data = await _service.ProvisionAsync(dto, cancellationToken);
        return Ok(ApiResponse<WebsiteSummaryDto>.Ok(data));
    }

    [HttpPost("templates")]
    [Authorize(Roles = AppRoles.SuperAdmin)]
    public async Task<ActionResult<ApiResponse<PageTemplateDto>>> SaveTemplate(
        [FromQuery] Guid? id, SavePageTemplateDto dto, CancellationToken cancellationToken)
    {
        var data = await _service.SavePageTemplateAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<PageTemplateDto>.Ok(data));
    }

    [HttpPost("assign-templates")]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.TenantAdmin}")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PublicPageDto>>>> AssignTemplates(
        AssignTemplatesDto dto, CancellationToken cancellationToken)
    {
        var data = await _service.AssignTemplatesAsync(dto, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<PublicPageDto>>.Ok(data));
    }

    [HttpPost("sync-menu")]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.TenantAdmin},{AppRoles.Editor}")]
    public async Task<ActionResult<ApiResponse<object>>> SyncMenu(CancellationToken cancellationToken)
    {
        await _service.SyncHeaderMenuAsync(cancellationToken);
        return Ok(ApiResponse<object>.Ok(new { synced = true }));
    }

    [HttpGet("branding")]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.TenantAdmin},{AppRoles.Editor}")]
    public async Task<ActionResult<ApiResponse<SiteBrandingDto>>> GetBranding(CancellationToken cancellationToken)
    {
        var data = await _service.GetBrandingAsync(cancellationToken);
        return Ok(ApiResponse<SiteBrandingDto>.Ok(data));
    }

    [HttpPut("branding")]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.TenantAdmin},{AppRoles.Editor}")]
    public async Task<ActionResult<ApiResponse<SiteBrandingDto>>> SaveBranding(
        SiteBrandingDto dto, CancellationToken cancellationToken)
    {
        var data = await _service.SaveBrandingAsync(dto, cancellationToken);
        return Ok(ApiResponse<SiteBrandingDto>.Ok(data));
    }

    [HttpGet("public")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<PublicWebsiteDto>>> PublicWebsite(CancellationToken cancellationToken)
    {
        var data = await _service.GetPublicWebsiteAsync(cancellationToken);
        return Ok(ApiResponse<PublicWebsiteDto>.Ok(data));
    }

    [HttpGet("public/pages/{slug}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<PublicPageDto>>> PublicPage(string slug, CancellationToken cancellationToken)
    {
        var data = await _service.GetPublicPageAsync(slug, cancellationToken);
        return Ok(ApiResponse<PublicPageDto>.Ok(data));
    }

    [HttpGet("contacts")]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.TenantAdmin},{AppRoles.Editor}")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ContactSubmissionDto>>>> Contacts(CancellationToken cancellationToken)
    {
        var data = await _service.GetContactSubmissionsAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<ContactSubmissionDto>>.Ok(data));
    }

    [HttpPost("contacts")]
    [AllowAnonymous]
    [EnableRateLimiting("public-forms")]
    public async Task<ActionResult<ApiResponse<ContactSubmissionDto>>> SubmitContact(
        SubmitContactDto dto, CancellationToken cancellationToken)
    {
        var data = await _service.SubmitContactAsync(dto, cancellationToken);
        return Ok(ApiResponse<ContactSubmissionDto>.Ok(data));
    }
}
