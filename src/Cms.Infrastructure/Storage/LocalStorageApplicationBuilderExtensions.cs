using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace Cms.Infrastructure.Storage;

public static class LocalStorageApplicationBuilderExtensions
{
    public static IApplicationBuilder UseLocalMediaFiles(this IApplicationBuilder app)
    {
        var services = app.ApplicationServices;
        var options = services.GetRequiredService<IOptions<StorageOptions>>().Value;
        if (!string.Equals(options.Provider, "Local", StringComparison.OrdinalIgnoreCase))
        {
            return app;
        }

        var environment = services.GetRequiredService<IWebHostEnvironment>();
        var root = ResolveRoot(environment.ContentRootPath, options.LocalRootPath);
        Directory.CreateDirectory(root);

        return app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(root),
            RequestPath = "/uploads"
        });
    }

    /// <summary>
    /// Serves S3-backed media through the application, so the bucket can stay private.
    /// Does nothing when the bucket is public or a CDN is in front of it — those are linked
    /// directly and never reach this application.
    /// </summary>
    public static IApplicationBuilder UseS3MediaFiles(this IApplicationBuilder app)
    {
        var services = app.ApplicationServices;
        var storage = services.GetRequiredService<IOptions<StorageOptions>>().Value;
        var aws = services.GetRequiredService<IOptions<AwsOptions>>().Value;

        if (!string.Equals(storage.Provider, "S3", StringComparison.OrdinalIgnoreCase)
            || aws.PublicBucket
            || !string.IsNullOrWhiteSpace(aws.PublicBaseUrl))
        {
            return app;
        }

        return app.UseMiddleware<S3MediaProxyMiddleware>();
    }

    internal static string ResolveRoot(string contentRootPath, string configuredPath) =>
        Path.GetFullPath(Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.Combine(contentRootPath, configuredPath));
}
