using Cms.Admin.Middleware;
using Cms.Application.DependencyInjection;
using Cms.Domain.Constants;
using Cms.Infrastructure.DependencyInjection;
using Cms.Infrastructure.Persistence.Seed;
using Cms.Infrastructure.Storage;
using Cms.Infrastructure.Tenancy;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHttpClient("DemoApiGateway");

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CmsAccess", policy =>
        policy.RequireRole(AppRoles.SuperAdmin, AppRoles.TenantAdmin, AppRoles.Editor));
});
var trustForwardedHeaders = builder.Configuration.GetValue<bool>("Proxy:TrustForwardedHeaders");
if (trustForwardedHeaders)
{
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();
    });
}
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("auth", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeAreaFolder("CMS", "/", "CmsAccess");
    options.Conventions.AuthorizePage("/Index", "CmsAccess");
    options.Conventions.AllowAnonymousToPage("/Account/Login");
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/Login";
});

var app = builder.Build();

if (!builder.Configuration.GetValue<bool>("Seed:SkipStartup"))
{
    await DatabaseSeeder.SeedAsync(app.Services);
}

if (trustForwardedHeaders)
{
    app.UseForwardedHeaders();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseMiddleware<DemoApiGatewayMiddleware>();
app.UseStaticFiles();
app.UseLocalMediaFiles();
app.UseRouting();
app.UseRateLimiter();
app.UseMiddleware<TenantResolutionMiddleware>();
app.UseAuthentication();
app.UseMiddleware<TenantAuthorizationMiddleware>();
app.UseAuthorization();
app.MapRazorPages();

app.Run();
