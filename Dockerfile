FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore Cms.sln
RUN dotnet publish src/Cms.Api/Cms.Api.csproj -c Release -o /out/api --no-restore
RUN dotnet publish src/Cms.Admin/Cms.Admin.csproj -c Release -o /out/admin --no-restore
RUN dotnet publish src/Cms.Web/Cms.Web.csproj -c Release -o /out/web --no-restore
RUN mkdir -p /out/demo/home /tmp/cms-demo-build/uploads \
    && cd /out/api \
    && env HOME=/out/demo/home \
       ASPNETCORE_ENVIRONMENT=Development \
       Database__Provider=Sqlite \
       Database__ApplyMigrationsOnStartup=false \
       'ConnectionStrings__Sqlite=Data Source=/out/demo/cms.db' \
       Storage__Provider=Local \
       Storage__LocalRootPath=/tmp/cms-demo-build/uploads \
       Storage__LocalBaseUrl=/uploads \
       DemoMode__Enabled=true \
       DemoMode__SeedOnly=true \
       Seed__EnableDemoData=true \
       Seed__DemoAdminPassword=Admin@12345 \
       Jwt__Key=CmsVercelImageBuildOnlyKey_AtLeast_32_Bytes \
       dotnet Cms.Api.dll

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
RUN apt-get update \
    && apt-get install -y --no-install-recommends nginx \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=build /out/api ./api
COPY --from=build /out/admin ./admin
COPY --from=build /out/web ./web
# Data Protection keys are generated into this seed database during the build step
# above, so there is no key folder on disk to copy any more.
COPY --from=build /out/demo/cms.db ./demo-seed/cms.db
COPY vercel/nginx.single-host.conf /app/nginx.single-host.conf
COPY vercel/nginx.multi-host.conf.template /app/nginx.multi-host.conf.template
COPY --chmod=755 vercel/entrypoint.sh /app/entrypoint.sh

# Production, not Development. Development served the raw exception page to visitors — a school
# saw a stack trace instead of an error page — and printed the demo sign-in credentials on the
# login screen. Seed__DemoAdminPassword must be supplied explicitly outside Development; override
# it in the hosting environment before anyone real uses this.
#
# DOTNET_USE_POLLING_FILE_WATCHER stops the file providers behind asset versioning from opening
# inotify instances. The container's allowance is small and shared by all three applications, and
# exhausting it made every page fail with "the configured user limit (128) on the number of
# inotify instances has been reached".
#
# Tenancy__ResolutionCacheSeconds is short here because the console and the public website are
# separate processes: creating a website invalidates the cache in the process that made the
# change, and the other one has to notice by expiry. Three seconds keeps "create a site, open
# its link" instant without giving up the cache on every public request.
#
# PublicSite__PathBase says the public website is served by this same container under /site.
# It takes precedence over PublicSite__BaseUrl, so a stale absolute URL left in the hosting
# environment cannot send editors to a dead address.
ENV ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE=false \
    DOTNET_USE_POLLING_FILE_WATCHER=true \
    Database__Provider=Sqlite \
    Database__ApplyMigrationsOnStartup=false \
    ConnectionStrings__Sqlite="Data Source=/data/cms.db;Cache=Shared;Default Timeout=30" \
    Storage__Provider=Local \
    Storage__LocalRootPath=/data/uploads \
    Storage__LocalBaseUrl=/uploads \
    DemoMode__Enabled=true \
    Proxy__TrustForwardedHeaders=true \
    PublicSite__PathBase=/site \
    Tenancy__ResolutionCacheSeconds=3 \
    Seed__EnableDemoData=true \
    Seed__DemoAdminPassword=Admin@12345 \
    Seed__SkipStartup=true \
    PORT=80

EXPOSE 80
ENTRYPOINT ["/app/entrypoint.sh"]
