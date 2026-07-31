using Cms.Application.DTOs.HomePage;
using Cms.Application.DTOs.Media;
using Microsoft.AspNetCore.Http;

namespace Cms.Application.Interfaces;

public interface IMediaService
{
    Task<UploadImageResultDto> UploadImageAsync(IFormFile file, string? folder = null, CancellationToken cancellationToken = default);
    Task<UploadImageResultDto> UploadDocumentAsync(IFormFile file, string? folder = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MediaFileDto>> GetAllAsync(string? mediaType = null, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid mediaId, CancellationToken cancellationToken = default);
}
