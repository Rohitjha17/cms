## Summary

Production-ready **Multi-Tenant School & College CMS** (.NET 8) with a fully dynamic **Home Page CMS**. No homepage content is hardcoded — every section is stored per `TenantId` + `SiteId` and editable via Admin or REST API.

## Solution layout

```
Cms.sln
src/
  Cms.Domain/          Entities, enums, HomePageSectionKeys
  Cms.Application/     DTOs, services, validators, AutoMapper
  Cms.Infrastructure/  EF Core, repositories, S3/local storage, tenant middleware, seed
  Cms.Shared/          ApiResponse, exceptions, helpers
  Cms.Api/             JWT REST API + Swagger
  Cms.Admin/           Razor Pages CMS (Home Page editor)
docs/
  sample-homepage.sql
  sample-homepage-response.json
```

## Design decisions

1. **One `HomePageSections` table** — unlimited sections without schema changes; section-specific data lives in `JsonData`.
2. **Tenant isolation** — EF global query filters + service-layer `ITenantContext` / `ISiteContext` (never trust client tenant/site IDs).
3. **Media outside SQL** — images upload to Amazon S3 (or local `wwwroot/uploads` in Development); only URLs are stored.
4. **Seed on demand** — default section keys are created when a site’s homepage is first requested / seeded.

## Prerequisites

- .NET 8 SDK
- SQL Server (Docker example below)
- Optional: AWS S3 credentials when `Storage:Provider` is `S3`

```bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Your_password123" \
  -p 1433:1433 --name cms-sql -d mcr.microsoft.com/mssql/server:2022-latest
```

## Configure

Update connection strings in:

- `src/Cms.Api/appsettings.json`
- `src/Cms.Admin/appsettings.json`

Production configuration is intentionally secret-free. Supply these values through environment
variables, a secret manager, or deployment configuration:

```bash
ConnectionStrings__DefaultConnection="..."
Jwt__Key="a-random-secret-of-at-least-32-bytes"
Aws__AccessKey="..."
Aws__SecretKey="..."
Cors__AllowedOrigins__0="https://www.example.edu"
Database__ApplyMigrationsOnStartup="false"
```

Demo tenant, content and credentials are seeded automatically only in Development. Production
demo seeding is disabled unless `Seed__EnableDemoData=true` and
`Seed__DemoAdminPassword` is explicitly provided.

Apply EF migrations as a deployment step in production. Startup migration is enabled by
default only in Development to avoid races between multiple application instances.

For S3:

```json
"Storage": { "Provider": "S3" },
"Aws": {
  "AccessKey": "...",
  "SecretKey": "...",
  "Region": "us-east-1",
  "BucketName": "your-bucket",
  "PublicBaseUrl": "https://cdn.example.com"
}
```

## Run

```bash
dotnet restore Cms.sln
dotnet run --project src/Cms.Api
dotnet run --project src/Cms.Admin
```

For a containerized local stack:

```bash
docker compose up --build
```

On Apple Silicon, use an ARM-compatible external SQL Server/Azure SQL Edge instance if the
SQL Server x64 image does not run correctly, then override `ConnectionStrings__DefaultConnection`.

- API Swagger: `https://localhost:7101/swagger`
- Admin: `https://localhost:7201`
- Demo login: `admin@demo.local` / `Admin@12345`
- Development SuperAdmin: `superadmin@demo.local` / `Admin@12345`
- Demo tenant domains: `localhost`, `127.0.0.1`
- Site selection: header `X-Site-Key: school|college` or query `?site=school`

## API

| Method | Route | Auth |
|--------|-------|------|
| POST | `/api/auth/login` | Anonymous |
| GET | `/api/homepage` | Anonymous (tenant host) |
| GET | `/api/homepage/{sectionKey}` | Anonymous |
| POST | `/api/homepage` | JWT Admin/Editor |
| PUT | `/api/homepage/{sectionKey}` | JWT |
| POST | `/api/homepage/upload` | JWT |
| PUT | `/api/homepage/reorder` | JWT |
| PATCH | `/api/homepage/{sectionKey}/status` | JWT |
| DELETE | `/api/homepage/{sectionKey}` | JWT TenantAdmin+ |

Example login + update:

```bash
TOKEN=$(curl -s -X POST https://localhost:7101/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@demo.local","password":"Admin@12345"}' | jq -r .data.token)

curl -s https://localhost:7101/api/homepage -H "X-Site-Key: school"

curl -s -X PUT https://localhost:7101/api/homepage/hero \
  -H "Authorization: Bearer $TOKEN" \
  -H "X-Site-Key: school" \
  -H "Content-Type: application/json" \
  -d '{"title":"Welcome to ABC School","subTitle":"Future Begins Here","buttonText":"Apply Now","buttonLink":"/admissions","jsonData":"{\"students\":1500}","isActive":true,"displayOrder":1}'
```

## Admin CMS

`/CMS/HomePage` provides:

- Premium responsive workspace with persistent School/College switching
- Content readiness metrics, search, status filters and ordered section composition
- Structured editors for hero, statistics, contacts, CTAs and repeatable card collections
- Rich text sanitization, image signature validation and tenant-isolated media uploads
- Custom section creation, publish/hide controls and responsive live preview

Additional self-contained CMS modules are available under the protected Admin workspace:

- Pages with slugs, publishing, rich content and per-page SEO
- Navigation menus with ordered menu items
- Shared media library with image and PDF upload, reuse and deletion
- Site-wide SEO settings
- Flexible Events, News, People, Departments, Settings and Theme content
- SuperAdmin tenant, domain and School/College site provisioning

Public APIs are exposed under `/api/pages`, `/api/navigation`, `/api/seo` and
`/api/content/{type}`. Protected media management is under `/api/media`; SuperAdmin
tenant management is under `/api/tenants`.

## Quality and security

- Authenticated tenant claims are checked against the host-resolved tenant on every request.
- EF tenant/site filters fail closed when context is unresolved.
- Login lockout and per-IP rate limiting protect both API and Admin authentication.
- Upload folders and storage paths are constrained; extensions are derived from verified media types.
- GitHub Actions builds with warnings as errors and runs the test suite.

```bash
dotnet test Cms.sln
```

The suite currently includes 7 application tests and 4 API integration tests covering
authentication, response envelopes and School/College site isolation.

External deployment work is intentionally not embedded in source: production SQL and S3/CDN
credentials, real domains/DNS, trusted HTTPS certificates, hosting, monitoring destinations,
email/SMS providers and backup infrastructure must be supplied by the deployment environment.

## Samples

- SQL: [docs/sample-homepage.sql](docs/sample-homepage.sql)
- JSON response: [docs/sample-homepage-response.json](docs/sample-homepage-response.json)
