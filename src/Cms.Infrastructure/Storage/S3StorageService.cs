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
        IAmazonS3 client,
        ITenantContext tenantContext,
        ISiteContext siteContext,
        ILogger<S3StorageService> logger)
    {
        _options = options.Value;
        _client = client;
        _tenantContext = tenantContext;
        _siteContext = siteContext;
        _logger = logger;
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
            "video/mp4" => ".mp4",
            "video/webm" => ".webm",
            _ => throw new InvalidOperationException("Unsupported media content type.")
        };
        if (_tenantContext.TenantId is not Guid tenantId || _siteContext.SiteId is not Guid siteId)
        {
            // Without both, every tenant would share one folder — fail instead of leaking.
            throw new InvalidOperationException("Cannot store media before the tenant and website are resolved.");
        }

        var storedName = $"{Guid.NewGuid():N}{extension}";
        var key = Key(_options, $"{tenantId}/{siteId}/{folder}/{storedName}");

        var request = new PutObjectRequest
        {
            BucketName = _options.BucketName,
            Key = key,
            InputStream = content,
            ContentType = contentType,
            Headers = { CacheControl = "public, max-age=31536000, immutable" }
        };

        await _client.PutObjectAsync(request, cancellationToken);

        // The address is stored with the content and outlives this upload by years, so it must
        // stay valid no matter what the bucket's own permissions are set to later. Only a CDN or
        // a bucket that is deliberately public gets linked directly; otherwise the file is
        // served through this application, which is the one thing that always has the key.
        var url = PublicUrl(_options, key);

        _logger.LogInformation("Uploaded file to S3 key {Key}", key);
        return new StoredFileResult(url, key, storedName);
    }

    /// <summary>
    /// Where the browser should ask for this file. Same shape as local storage when the bucket
    /// is private, so nothing downstream has to know which provider stored it.
    /// </summary>
    /// <summary>Applies the configured prefix, if the bucket is shared with anything else.</summary>
    public static string Key(AwsOptions options, string path)
    {
        var prefix = options.FolderPath?.Trim().Trim('/');
        return string.IsNullOrEmpty(prefix) ? path : $"{prefix}/{path}";
    }

    public static string PublicUrl(AwsOptions options, string key)
    {
        if (!string.IsNullOrWhiteSpace(options.PublicBaseUrl))
        {
            return $"{options.PublicBaseUrl.TrimEnd('/')}/{key}";
        }

        return options.PublicBucket
            ? $"https://{options.BucketName}.s3.{options.Region}.amazonaws.com/{key}"
            : $"{S3MediaProxyMiddleware.PathPrefix}/{key}";
    }

    public async Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        await _client.DeleteObjectAsync(_options.BucketName, storageKey, cancellationToken);
    }
}
