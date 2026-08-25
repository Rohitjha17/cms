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

# The image is production-first: SQL Server, migrations applied on the first run, and no demo
# content or demo password anywhere. A deployment supplies its own connection string, the
# console's domain and the first administrator through the hosting environment — see
# deploy/production.env.example for the full list. Anything missing stops the application on
# startup with a message naming it, rather than quietly serving a school demo data.
#
# To run the sample workspace instead, set DemoMode__Enabled=true, Seed__EnableDemoData=true,
# Seed__DemoAdminPassword, Database__Provider=Sqlite and the Sqlite connection string.
#
# Seeding is deliberately NOT skipped here: the console applies the migrations and creates the
# first administrator, and the entrypoint stops only the other two from racing it.
#
# ASPNETCORE_ENVIRONMENT is Production so a visitor never sees a stack trace and the sign-in
# credentials are not printed on the login screen.
#
# DOTNET_USE_POLLING_FILE_WATCHER stops the file providers behind asset versioning from opening
# inotify instances; the container's allowance is small and shared by all three applications.
#
ENV ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE=false \
    DOTNET_USE_POLLING_FILE_WATCHER=true \
    Database__Provider=SqlServer \
    Database__ApplyMigrationsOnStartup=true \
    Storage__Provider=Local \
    Storage__LocalRootPath=/data/uploads \
    Storage__LocalBaseUrl=/uploads \
    Proxy__TrustForwardedHeaders=true \
    PublicSite__PathBase=/site \
    Tenancy__ResolutionCacheSeconds=3 \
    PublicCache__Seconds=0 \
    PORT=80

EXPOSE 80
ENTRYPOINT ["/app/entrypoint.sh"]
