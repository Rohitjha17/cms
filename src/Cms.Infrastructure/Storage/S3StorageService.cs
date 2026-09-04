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
        if (_tenantContext.TenantId is null || _siteContext.SiteId is null)
        {
            // Without both, every tenant would share one folder — fail instead of leaking.
            throw new InvalidOperationException("Cannot store media before the tenant and website are resolved.");
        }

        var storedName = $"{Guid.NewGuid():N}{extension}";
        var path = $"{MediaFolder.For(_tenantContext, _siteContext, folder)}/{storedName}";
        var key = Key(_options, path);

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
        var url = PublicUrl(_options, key, path);

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

    /// <param name="key">The object's full name in the bucket, folder prefix and all.</param>
    /// <param name="path">The same thing without the prefix — tenant, site, folder, file.</param>
    public static string PublicUrl(AwsOptions options, string key, string path)
    {
        // A CDN or a public bucket is addressed directly, so it needs the real object name.
        if (!string.IsNullOrWhiteSpace(options.PublicBaseUrl))
        {
            return $"{options.PublicBaseUrl.TrimEnd('/')}/{key}";
        }

        if (options.PublicBucket)
        {
            return $"https://{options.BucketName}.s3.{options.Region}.amazonaws.com/{key}";
        }

        // Served through this application, so the address does not need to name the bucket
        // folder — and must not. This URL is written into the database beside the file and
        // outlives the configuration that produced it: with the folder baked in, changing
        // FolderPath, or moving a site between local storage and S3, silently broke every
        // picture already uploaded. Without it, the address is the same one local storage
        // gives, and the folder is applied when the object is fetched.
        return $"{S3MediaProxyMiddleware.PathPrefix}/{path}";
    }

    public async Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        await _client.DeleteObjectAsync(_options.BucketName, storageKey, cancellationToken);
    }
}
