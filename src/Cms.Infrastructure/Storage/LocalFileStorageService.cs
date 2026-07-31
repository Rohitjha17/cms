using Cms.Application.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cms.Infrastructure.Storage;

public class LocalFileStorageService : IFileStorageService
{
    private readonly StorageOptions _options;
    private readonly ITenantContext _tenantContext;
    private readonly ISiteContext _siteContext;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<LocalFileStorageService> _logger;

    public LocalFileStorageService(
        IOptions<StorageOptions> options,
        ITenantContext tenantContext,
        ISiteContext siteContext,
        IWebHostEnvironment env,
        ILogger<LocalFileStorageService> logger)
    {
        _options = options.Value;
        _tenantContext = tenantContext;
        _siteContext = siteContext;
        _env = env;
        _logger = logger;
    }

    public async Task<StoredFileResult> UploadAsync(Stream content, string fileName, string contentType, string folder, CancellationToken cancellationToken = default)
    {
        var extension = ExtensionFor(contentType);
        var storedName = $"{Guid.NewGuid():N}{extension}";
        var relativeFolder = Path.Combine(
            _tenantContext.TenantId?.ToString() ?? "unknown",
            _siteContext.SiteId?.ToString() ?? "unknown",
            folder);

        var root = LocalStorageApplicationBuilderExtensions.ResolveRoot(
            _env.ContentRootPath, _options.LocalRootPath);
        var absoluteFolder = Path.GetFullPath(Path.Combine(root, relativeFolder));
        EnsureInsideRoot(root, absoluteFolder);
        Directory.CreateDirectory(absoluteFolder);

        var absolutePath = Path.Combine(absoluteFolder, storedName);
        await using (var fs = File.Create(absolutePath))
        {
            await content.CopyToAsync(fs, cancellationToken);
        }

        var url = $"{_options.LocalBaseUrl.TrimEnd('/')}/{relativeFolder.Replace('\\', '/')}/{storedName}";
        var storageKey = $"{relativeFolder.Replace('\\', '/')}/{storedName}";

        _logger.LogInformation("Stored local file {Path}", absolutePath);
        return new StoredFileResult(url, storageKey, storedName);
    }

    public Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        var root = LocalStorageApplicationBuilderExtensions.ResolveRoot(
            _env.ContentRootPath, _options.LocalRootPath);
        var path = Path.GetFullPath(Path.Combine(root, storageKey.Replace('/', Path.DirectorySeparatorChar)));
        EnsureInsideRoot(root, path);
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        return Task.CompletedTask;
    }

    private static string ExtensionFor(string contentType) => contentType.ToLowerInvariant() switch
    {
        "image/jpeg" or "image/jpg" => ".jpg",
        "image/png" => ".png",
        "image/webp" => ".webp",
        "image/gif" => ".gif",
        "application/pdf" => ".pdf",
        _ => throw new InvalidOperationException("Unsupported media content type.")
    };

    private static void EnsureInsideRoot(string root, string path)
    {
        var rootPrefix = root.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (!path.StartsWith(rootPrefix, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Media path resolved outside the configured storage root.");
        }
    }
}
