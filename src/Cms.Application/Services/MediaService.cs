using Cms.Application.DTOs.HomePage;
using Cms.Application.DTOs.Media;
using Cms.Application.Interfaces;
using Cms.Application.Validators;
using Cms.Domain.Entities;
using Cms.Domain.Enums;
using Cms.Shared.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace Cms.Application.Services;

public class MediaService : IMediaService
{
    private readonly IFileStorageService _storage;
    private readonly IMediaRepository _mediaRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ISiteContext _siteContext;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly UploadImageValidator _imageValidator;
    private readonly UploadDocumentValidator _documentValidator;
    private readonly ILogger<MediaService> _logger;

    public MediaService(
        IFileStorageService storage,
        IMediaRepository mediaRepository,
        ITenantContext tenantContext,
        ISiteContext siteContext,
        ICurrentUserContext currentUserContext,
        UploadImageValidator imageValidator,
        UploadDocumentValidator documentValidator,
        ILogger<MediaService> logger)
    {
        _storage = storage;
        _mediaRepository = mediaRepository;
        _tenantContext = tenantContext;
        _siteContext = siteContext;
        _currentUserContext = currentUserContext;
        _imageValidator = imageValidator;
        _documentValidator = documentValidator;
        _logger = logger;
    }

    public async Task<UploadImageResultDto> UploadImageAsync(IFormFile file, string? folder = null, CancellationToken cancellationToken = default)
    {
        var validation = await _imageValidator.ValidateAsync(file, cancellationToken);
        if (!validation.IsValid)
        {
            throw new ValidationAppException("Invalid image upload.", validation.Errors.Select(e => e.ErrorMessage));
        }

        return await UploadValidatedAsync(file, folder ?? "homepage", MediaType.Image, cancellationToken);
    }

    public async Task<UploadImageResultDto> UploadDocumentAsync(
        IFormFile file,
        string? folder = null,
        CancellationToken cancellationToken = default)
    {
        var validation = await _documentValidator.ValidateAsync(file, cancellationToken);
        if (!validation.IsValid)
        {
            throw new ValidationAppException(
                "Invalid document upload.",
                validation.Errors.Select(e => e.ErrorMessage));
        }

        return await UploadValidatedAsync(file, folder ?? "documents", MediaType.Document, cancellationToken);
    }

    public async Task<IReadOnlyList<MediaFileDto>> GetAllAsync(
        string? mediaType = null,
        CancellationToken cancellationToken = default)
    {
        var (tenantId, siteId) = RequireTenantSite();
        var media = await _mediaRepository.GetAllAsync(tenantId, siteId, cancellationToken);
        if (!string.IsNullOrWhiteSpace(mediaType)
            && Enum.TryParse<MediaType>(mediaType, true, out var parsedType))
        {
            media = media.Where(x => x.MediaType == parsedType).ToList();
        }

        return media.Select(ToDto).ToList();
    }

    public async Task DeleteAsync(Guid mediaId, CancellationToken cancellationToken = default)
    {
        var (tenantId, siteId) = RequireTenantSite();
        var media = await _mediaRepository.GetByIdAsync(tenantId, siteId, mediaId, cancellationToken)
            ?? throw new NotFoundException("Media file was not found.");

        await _storage.DeleteAsync(media.StorageKey, cancellationToken);
        _mediaRepository.Delete(media);
        await _mediaRepository.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Deleted media {MediaId} from tenant {TenantId} site {SiteId}", mediaId, tenantId, siteId);
    }

    private async Task<UploadImageResultDto> UploadValidatedAsync(
        IFormFile file,
        string folder,
        MediaType mediaType,
        CancellationToken cancellationToken)
    {
        var (tenantId, siteId) = RequireTenantSite();
        var targetFolder = folder.Trim().ToLowerInvariant();
        if (!Regex.IsMatch(targetFolder, "^[a-z0-9][a-z0-9_-]{0,49}$"))
        {
            throw new ValidationAppException(
                "Invalid media folder. Use letters, numbers, hyphens, or underscores only.");
        }
        await using var stream = file.OpenReadStream();
        var stored = await _storage.UploadAsync(stream, file.FileName, file.ContentType, targetFolder, cancellationToken);

        var media = new MediaFile
        {
            TenantId = tenantId,
            SiteId = siteId,
            FileName = stored.FileName,
            OriginalFileName = Path.GetFileName(file.FileName),
            ContentType = file.ContentType,
            FileSizeBytes = file.Length,
            Url = stored.Url,
            StorageKey = stored.StorageKey,
            MediaType = mediaType,
            Folder = targetFolder,
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = _currentUserContext.UserId ?? "system"
        };

        await _mediaRepository.AddAsync(media, cancellationToken);
        await _mediaRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Uploaded media {MediaId} to {Url}", media.Id, media.Url);

        return new UploadImageResultDto
        {
            MediaId = media.Id,
            Url = media.Url,
            FileName = media.FileName,
            ContentType = media.ContentType,
            FileSizeBytes = media.FileSizeBytes
        };
    }

    private (Guid TenantId, Guid SiteId) RequireTenantSite()
    {
        if (!_tenantContext.IsResolved || _tenantContext.TenantId is null
            || !_siteContext.IsResolved || _siteContext.SiteId is null)
        {
            throw new TenantNotResolvedException();
        }

        return (_tenantContext.TenantId.Value, _siteContext.SiteId.Value);
    }

    private static MediaFileDto ToDto(MediaFile media) => new()
    {
        Id = media.Id,
        FileName = media.FileName,
        OriginalFileName = media.OriginalFileName,
        ContentType = media.ContentType,
        FileSizeBytes = media.FileSizeBytes,
        Url = media.Url,
        MediaType = media.MediaType.ToString(),
        Folder = media.Folder,
        CreatedDate = media.CreatedDate
    };
}
