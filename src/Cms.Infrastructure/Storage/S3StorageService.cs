using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Cms.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cms.Infrastructure.Storage;

public class S3StorageService : IFileStorageService
{
    private readonly AwsOptions _options;
    private readonly ITenantContext _tenantContext;
    private readonly ISiteContext _siteContext;
    private readonly ILogger<S3StorageService> _logger;
    private readonly IAmazonS3 _client;

    public S3StorageService(
        IOptions<AwsOptions> options,
        ITenantContext tenantContext,
        ISiteContext siteContext,
        ILogger<S3StorageService> logger)
    {
        _options = options.Value;
        _tenantContext = tenantContext;
        _siteContext = siteContext;
        _logger = logger;
        _client = new AmazonS3Client(_options.AccessKey, _options.SecretKey, RegionEndpoint.GetBySystemName(_options.Region));
    }

    public async Task<StoredFileResult> UploadAsync(Stream content, string fileName, string contentType, string folder, CancellationToken cancellationToken = default)
    {
        var extension = contentType.ToLowerInvariant() switch
        {
            "image/jpeg" or "image/jpg" => ".jpg",
            "image/png" => ".png",
            "image/webp" => ".webp",
            "image/gif" => ".gif",
            "application/pdf" => ".pdf",
            _ => throw new InvalidOperationException("Unsupported media content type.")
        };
        var storedName = $"{Guid.NewGuid():N}{extension}";
        var key = $"{_tenantContext.TenantId}/{_siteContext.SiteId}/{folder}/{storedName}";

        var request = new PutObjectRequest
        {
            BucketName = _options.BucketName,
            Key = key,
            InputStream = content,
            ContentType = contentType
        };

        await _client.PutObjectAsync(request, cancellationToken);
        var url = string.IsNullOrWhiteSpace(_options.PublicBaseUrl)
            ? $"https://{_options.BucketName}.s3.{_options.Region}.amazonaws.com/{key}"
            : $"{_options.PublicBaseUrl.TrimEnd('/')}/{key}";

        _logger.LogInformation("Uploaded file to S3 key {Key}", key);
        return new StoredFileResult(url, key, storedName);
    }

    public async Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        await _client.DeleteObjectAsync(_options.BucketName, storageKey, cancellationToken);
    }
}
