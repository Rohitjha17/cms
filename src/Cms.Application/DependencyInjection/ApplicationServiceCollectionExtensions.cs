using Cms.Application.Interfaces;
using Cms.Application.Mapping;
using Cms.Application.Services;
using Cms.Application.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Cms.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(HomePageMappingProfile).Assembly);
        services.AddValidatorsFromAssemblyContaining<UpdateHomePageSectionValidator>();
        services.AddScoped<UploadImageValidator>();
        services.AddScoped<UploadDocumentValidator>();
        services.AddScoped<IHomePageService, HomePageService>();
        services.AddScoped<IMediaService, MediaService>();
        services.AddScoped<ISiteContentService, SiteContentService>();
        services.AddScoped<ITenantManagementService, TenantManagementService>();
        return services;
    }
}
