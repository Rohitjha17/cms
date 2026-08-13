using Amazon;
using Amazon.S3;
using Cms.Application.Interfaces;
using Cms.Infrastructure.Email;
using Cms.Infrastructure.Identity;
using Cms.Infrastructure.Persistence;
using Cms.Infrastructure.Repositories;
using Cms.Infrastructure.Storage;
using Cms.Infrastructure.Tenancy;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cms.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<StorageOptions>(configuration.GetSection(StorageOptions.SectionName));
        services.Configure<AwsOptions>(configuration.GetSection(AwsOptions.SectionName));
        services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.SectionName));

        services.AddScoped<ITenantContext, TenantContext>();
        services.AddScoped<ISiteContext, SiteContext>();
        services.AddMemoryCache();
        services.AddScoped<ITenantHostResolver, TenantHostResolver>();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserContext, CurrentUserContext>();

        var databaseProvider = configuration.GetValue<string>("Database:Provider") ?? "SqlServer";
        var connectionString = string.Equals(databaseProvider, "Sqlite", StringComparison.OrdinalIgnoreCase)
            ? configuration.GetConnectionString("Sqlite")
            : configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"A connection string must be configured for database provider '{databaseProvider}'.");
        }

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            if (string.Equals(databaseProvider, "Sqlite", StringComparison.OrdinalIgnoreCase))
            {
                options.UseSqlite(connectionString);
            }
            else
            {
                options.UseSqlServer(connectionString);
            }
        });

        // Shared key ring across API, Admin and Web, and across every instance of each.
        // Without this the keys live on local disk: on ephemeral or scaled hosting each
        // container issues cookies the others cannot read, so sign-ins appear to "not work
        // on some devices". SetApplicationName must match in all three applications.
        services.AddDataProtection()
            .PersistKeysToDbContext<ApplicationDbContext>()
            .SetApplicationName("Cms");

        services
            .AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddClaimsPrincipalFactory<TenantUserClaimsPrincipalFactory>()
            .AddDefaultTokenProviders();

        services.AddScoped<IHomePageRepository, HomePageRepository>();
        services.AddScoped<IMediaRepository, MediaRepository>();
        services.AddScoped<ISiteContentRepository, SiteContentRepository>();
        services.AddScoped<ITenantManagementRepository, TenantManagementRepository>();
        services.AddScoped<IWebsiteRepository, WebsiteRepository>();
        services.AddScoped<IUserManagementService, UserManagementService>();

        // Real SMTP only when configured; otherwise the CMS surfaces one-time reset links
        // in the UI rather than pretending mail was delivered.
        var smtpHost = configuration.GetValue<string>($"{EmailOptions.SectionName}:Host");
        var smtpFrom = configuration.GetValue<string>($"{EmailOptions.SectionName}:FromAddress");
        if (!string.IsNullOrWhiteSpace(smtpHost) && !string.IsNullOrWhiteSpace(smtpFrom))
        {
            services.AddScoped<IEmailSender, SmtpEmailSender>();
        }
        else
        {
            services.AddScoped<IEmailSender, NullEmailSender>();
        }

        var storageProvider = configuration.GetValue<string>($"{StorageOptions.SectionName}:Provider") ?? "Local";
        if (string.Equals(storageProvider, "S3", StringComparison.OrdinalIgnoreCase))
        {
            // One S3 client for the process: it is thread-safe, pools connections and must not
            // be rebuilt (and leaked) per request.
            services.AddSingleton<IAmazonS3>(provider =>
            {
                var aws = provider.GetRequiredService<IOptions<AwsOptions>>().Value;
                var region = RegionEndpoint.GetBySystemName(aws.Region);
                return string.IsNullOrWhiteSpace(aws.AccessKey) || string.IsNullOrWhiteSpace(aws.SecretKey)
                    ? new AmazonS3Client(region)
                    : new AmazonS3Client(aws.AccessKey, aws.SecretKey, region);
            });
            services.AddScoped<IFileStorageService, S3StorageService>();
        }
        else
        {
            services.AddScoped<IFileStorageService, LocalFileStorageService>();
        }

        return services;
    }
}
