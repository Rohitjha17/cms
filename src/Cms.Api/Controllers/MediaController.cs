using Cms.Application.DTOs.HomePage;
using Cms.Application.DTOs.Media;
using Cms.Application.Interfaces;
using Cms.Domain.Constants;
using Cms.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cms.Api.Controllers;

[ApiController]
[Route("api/media")]
[Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.TenantAdmin},{AppRoles.Editor}")]
public sealed class MediaController : ControllerBase
{
    private readonly IMediaService _mediaService;

    public MediaController(IMediaService mediaService)
    {
        _mediaService = mediaService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<MediaFileDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MediaFileDto>>>> GetAll(
        [FromQuery] string? type,
        CancellationToken cancellationToken)
    {
        var data = await _mediaService.GetAllAsync(type, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<MediaFileDto>>.Ok(data));
    }

    [HttpGet("{mediaId:guid}")]
    public async Task<ActionResult<ApiResponse<MediaFileDto>>> Get(
        Guid mediaId, CancellationToken cancellationToken) =>
        Ok(ApiResponse<MediaFileDto>.Ok(await _mediaService.GetAsync(mediaId, cancellationToken)));

    [HttpPost("images")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<ActionResult<ApiResponse<UploadImageResultDto>>> UploadImage(
        IFormFile file,
        [FromForm] string? folder,
        CancellationToken cancellationToken)
    {
        var data = await _mediaService.UploadImageAsync(file, folder, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<UploadImageResultDto>.Created(data));
    }

    [HttpPost("documents")]
    [RequestSizeLimit(16 * 1024 * 1024)]
    public async Task<ActionResult<ApiResponse<UploadImageResultDto>>> UploadDocument(
        IFormFile file,
        [FromForm] string? folder,
        CancellationToken cancellationToken)
    {
        var data = await _mediaService.UploadDocumentAsync(file, folder, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<UploadImageResultDto>.Created(data));
    }

    [HttpPost("videos")]
    [RequestSizeLimit(101 * 1024 * 1024)]
    public async Task<ActionResult<ApiResponse<UploadImageResultDto>>> UploadVideo(
        IFormFile file, [FromForm] string? folder, CancellationToken cancellationToken)
    {
        var data = await _mediaService.UploadVideoAsync(file, folder, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<UploadImageResultDto>.Created(data));
    }

    [HttpPut("{mediaId:guid}")]
    public async Task<ActionResult<ApiResponse<MediaFileDto>>> Update(
        Guid mediaId, UpdateMediaDto dto, CancellationToken cancellationToken) =>
        Ok(ApiResponse<MediaFileDto>.Ok(
            await _mediaService.UpdateAsync(mediaId, dto, cancellationToken), "Media updated."));

    [HttpPatch("{mediaId:guid}/status")]
    public async Task<ActionResult<ApiResponse>> SetStatus(
        Guid mediaId, SetMediaStatusDto dto, CancellationToken cancellationToken)
    {
        await _mediaService.SetStatusAsync(mediaId, dto.IsActive, cancellationToken);
        return Ok(ApiResponse.Ok(dto.IsActive ? "Media activated." : "Media deactivated."));
    }

    [HttpDelete("{mediaId:guid}")]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.TenantAdmin}")]
    public async Task<ActionResult<ApiResponse>> Delete(Guid mediaId, CancellationToken cancellationToken)
    {
        await _mediaService.DeleteAsync(mediaId, cancellationToken);
        return Ok(ApiResponse.Ok("Media deleted."));
    }
}
