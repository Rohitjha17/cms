using Cms.Application.Interfaces;
using Cms.Infrastructure.Identity;
using Cms.Infrastructure.Persistence;
using Cms.Infrastructure.Repositories;
using Cms.Infrastructure.Storage;
using Cms.Infrastructure.Tenancy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cms.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<StorageOptions>(configuration.GetSection(StorageOptions.SectionName));
        services.Configure<AwsOptions>(configuration.GetSection(AwsOptions.SectionName));

        services.AddScoped<ITenantContext, TenantContext>();
        services.AddScoped<ISiteContext, SiteContext>();
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

        var storageProvider = configuration.GetValue<string>($"{StorageOptions.SectionName}:Provider") ?? "Local";
        if (string.Equals(storageProvider, "S3", StringComparison.OrdinalIgnoreCase))
        {
            services.AddScoped<IFileStorageService, S3StorageService>();
        }
        else
        {
            services.AddScoped<IFileStorageService, LocalFileStorageService>();
        }

        return services;
    }
}
