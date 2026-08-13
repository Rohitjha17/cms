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
COPY --from=build /out/demo/cms.db ./demo-seed/cms.db
COPY --from=build /out/demo/home/.aspnet/DataProtection-Keys ./demo-seed/dataprotection
COPY vercel/nginx.conf /etc/nginx/nginx.conf
COPY --chmod=755 vercel/entrypoint.sh /app/entrypoint.sh

ENV ASPNETCORE_ENVIRONMENT=Development \
    DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE=false \
    Database__Provider=Sqlite \
    Database__ApplyMigrationsOnStartup=false \
    ConnectionStrings__Sqlite="Data Source=/data/cms.db;Cache=Shared;Default Timeout=30" \
    Storage__Provider=Local \
    Storage__LocalRootPath=/data/uploads \
    Storage__LocalBaseUrl=/uploads \
    DemoMode__Enabled=true \
    Proxy__TrustForwardedHeaders=true \
    PublicSite__BaseUrl=/site \
    Seed__EnableDemoData=true \
    Seed__SkipStartup=true \
    PORT=80

EXPOSE 80
ENTRYPOINT ["/app/entrypoint.sh"]
