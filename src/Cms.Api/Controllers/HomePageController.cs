using Cms.Application.DTOs.HomePage;
using Cms.Application.Interfaces;
using Cms.Domain.Constants;
using Cms.Shared.Responses;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cms.Api.Controllers;

[ApiController]
[Route("api/homepage")]
public class HomePageController : ControllerBase
{
    private readonly IHomePageService _homePageService;
    private readonly IMediaService _mediaService;
    private readonly IValidator<UpdateHomePageSectionDto> _updateValidator;
    private readonly IValidator<CreateHomePageSectionDto> _createValidator;
    private readonly IValidator<ReorderHomePageSectionsDto> _reorderValidator;
    private readonly ILogger<HomePageController> _logger;

    public HomePageController(
        IHomePageService homePageService,
        IMediaService mediaService,
        IValidator<UpdateHomePageSectionDto> updateValidator,
        IValidator<CreateHomePageSectionDto> createValidator,
        IValidator<ReorderHomePageSectionsDto> reorderValidator,
        ILogger<HomePageController> logger)
    {
        _homePageService = homePageService;
        _mediaService = mediaService;
        _updateValidator = updateValidator;
        _createValidator = createValidator;
        _reorderValidator = reorderValidator;
        _logger = logger;
    }

    /// <summary>Returns the complete homepage for the current tenant/site.</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<HomePageResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HomePageResponseDto>>> GetHomePage(
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var data = await _homePageService.GetHomePageAsync(
            includeInactive && CanManageContent(),
            cancellationToken);
        return Ok(ApiResponse<HomePageResponseDto>.Ok(data));
    }

    /// <summary>Returns a single homepage section by key.</summary>
    [HttpGet("{sectionKey}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<HomePageSectionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<HomePageSectionDto>>> GetSection(
        string sectionKey,
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var data = await _homePageService.GetSectionAsync(
            sectionKey,
            includeInactive && CanManageContent(),
            cancellationToken);
        return Ok(ApiResponse<HomePageSectionDto>.Ok(data));
    }

    /// <summary>Creates a custom homepage section.</summary>
    [HttpPost]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.TenantAdmin},{AppRoles.Editor}")]
    [ProducesResponseType(typeof(ApiResponse<HomePageSectionDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<HomePageSectionDto>>> CreateSection(
        [FromBody] CreateHomePageSectionDto dto,
        CancellationToken cancellationToken = default)
    {
        await _createValidator.ValidateAndThrowAsync(dto, cancellationToken);
        var data = await _homePageService.CreateSectionAsync(dto, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<HomePageSectionDto>.Created(data));
    }

    /// <summary>Updates a homepage section.</summary>
    [HttpPut("{sectionKey}")]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.TenantAdmin},{AppRoles.Editor}")]
    [ProducesResponseType(typeof(ApiResponse<HomePageSectionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<HomePageSectionDto>>> UpdateSection(
        string sectionKey,
        [FromBody] UpdateHomePageSectionDto dto,
        CancellationToken cancellationToken = default)
    {
        await _updateValidator.ValidateAndThrowAsync(dto, cancellationToken);
        var data = await _homePageService.UpdateSectionAsync(sectionKey, dto, cancellationToken);
        return Ok(ApiResponse<HomePageSectionDto>.Ok(data, "Section updated."));
    }

    /// <summary>Uploads an image to storage and returns the public URL.</summary>
    [HttpPost("upload")]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.TenantAdmin},{AppRoles.Editor}")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    [ProducesResponseType(typeof(ApiResponse<UploadImageResultDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<UploadImageResultDto>>> Upload(
        IFormFile file,
        [FromForm] string? folder,
        CancellationToken cancellationToken = default)
    {
        var data = await _mediaService.UploadImageAsync(file, folder, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<UploadImageResultDto>.Created(data, "Image uploaded."));
    }

    /// <summary>Reorders homepage sections.</summary>
    [HttpPut("reorder")]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.TenantAdmin},{AppRoles.Editor}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse>> Reorder(
        [FromBody] ReorderHomePageSectionsDto dto,
        CancellationToken cancellationToken = default)
    {
        await _reorderValidator.ValidateAndThrowAsync(dto, cancellationToken);
        await _homePageService.ReorderAsync(dto, cancellationToken);
        return Ok(ApiResponse.Ok("Sections reordered."));
    }

    /// <summary>Enables or disables a section.</summary>
    [HttpPatch("{sectionKey}/status")]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.TenantAdmin},{AppRoles.Editor}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse>> SetStatus(
        string sectionKey,
        [FromBody] SetHomePageSectionStatusDto dto,
        CancellationToken cancellationToken = default)
    {
        await _homePageService.SetStatusAsync(sectionKey, dto.IsActive, cancellationToken);
        return Ok(ApiResponse.Ok(dto.IsActive ? "Section enabled." : "Section disabled."));
    }

    /// <summary>Deletes (soft by default) a homepage section.</summary>
    [HttpDelete("{sectionKey}")]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.TenantAdmin}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> Delete(
        string sectionKey,
        [FromQuery] bool hardDelete = false,
        CancellationToken cancellationToken = default)
    {
        await _homePageService.DeleteSectionAsync(sectionKey, hardDelete, cancellationToken);
        _logger.LogInformation("Deleted homepage section {SectionKey} (hard={Hard})", sectionKey, hardDelete);
        return Ok(ApiResponse.Ok("Section deleted."));
    }

    private bool CanManageContent() =>
        User.IsInRole(AppRoles.SuperAdmin)
        || User.IsInRole(AppRoles.TenantAdmin)
        || User.IsInRole(AppRoles.Editor);
}
