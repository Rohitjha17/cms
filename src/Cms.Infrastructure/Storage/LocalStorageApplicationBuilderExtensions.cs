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

    internal static string ResolveRoot(string contentRootPath, string configuredPath) =>
        Path.GetFullPath(Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.Combine(contentRootPath, configuredPath));
}
