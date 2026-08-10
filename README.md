## Summary

Multi-tenant **School & College CMS** (.NET 8) with a dynamic public website, Admin workspace, REST API, tenant/site isolation, and configurable SQL Server and Amazon S3 infrastructure.

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

On a clean production database, set the platform console values so the CMS is reachable and
has a first administrator — without them no host resolves and no account exists:

```bash
Platform__Domain="admin.yourcompany.com"
Platform__SuperAdminEmail="ops@yourcompany.com"
Platform__SuperAdminPassword="a-strong-initial-password"
Platform__AdminBaseUrl="https://admin.yourcompany.com"
Email__Host="smtp.yourprovider.com"
Email__FromAddress="no-reply@yourcompany.com"
```

Full walkthrough, including onboarding a school: [docs/PRODUCTION_SETUP.md](docs/PRODUCTION_SETUP.md).

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
export MSSQL_SA_PASSWORD="replace-with-a-strong-local-password"
export JWT_KEY="replace-with-at-least-32-random-bytes"
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
- Faculty and staff directory grouped by leadership, teaching, administration and support
- News, notices and circulars with categories, featured pinning and PDF attachments
- Events with start/end times, venue and registration links, split into upcoming and past
- Site settings: announcement bar, admissions status and contacts, social links
- Flexible Departments content for anything outside those modules
- SuperAdmin tenant, domain and School/College site provisioning
- User accounts: invite administrators and editors, activate/deactivate, unlock, issue
  single-use password links; tenant-scoped and unable to escalate privileges
- Self-service forgot password, reset password and change password

Public APIs are exposed under `/api/pages`, `/api/navigation`, `/api/seo` and
`/api/content/{type}`. Protected media management is under `/api/media`; SuperAdmin
tenant management is under `/api/tenants`. Account administration is under `/api/users`,
with anonymous, rate-limited `POST /api/users/forgot-password` and
`POST /api/users/reset-password`.

### Public URL shapes

Both hosting models are served by the same deployment, decided by whether a `TenantDomains`
row is bound to a site:

| Domain binding | Example host | Page URL |
|----------------|--------------|----------|
| Bound to one site | `noida.cambridgeschool.edu.in` | `/about`, `/admission` |
| Shared by the tenant's sites | `abc.com` | `/school/about`, `/college/departments` |

On a shared host the leading site segment is moved into the request's path base, so any site
key works — `/junior-wing/about` is as valid as `/school/about` — and generated links, static
assets, `robots.txt` and `sitemap.xml` stay correct in both shapes. Site-bound custom domains
remain authoritative and cannot be overridden by anonymous headers, query strings, or cookies.

`GET /robots.txt`, `GET /sitemap.xml` and `GET /health` are served per resolved website.

Content modules are rendered publicly at `/faculty`, `/news`, `/news/{key}`, `/events` and
`/events/{key}` (behind the site prefix on a shared domain). Site settings drive the
announcement bar and the footer's social links on every page.

Media APIs support image, PDF and MP4/WebM uploads plus list, detail, metadata update,
active/inactive status and deletion. Tenant administrator audit history is available at
`/api/activity-logs`. Public contact submissions are rate limited.

## Quality and security

- Authenticated tenant claims are checked against the host-resolved tenant on every request.
- EF tenant/site filters fail closed when context is unresolved.
- Login lockout and per-IP rate limiting protect both API and Admin authentication. Public
  form submissions are throttled per IP; reading pages never is.
- Every response carries `Content-Security-Policy`, `X-Content-Type-Options`,
  `X-Frame-Options`, `Referrer-Policy` and `Permissions-Policy`.
- Host-to-tenant resolution is cached per process for `Tenancy:ResolutionCacheSeconds`
  (default 30, `0` disables); a newly bound domain becomes reachable within that window.
- Upload folders and storage paths are constrained; extensions are derived from verified media types.
- GitHub Actions builds with warnings as errors and runs the test suite.

```bash
dotnet test Cms.sln
```

The suite covers authentication, response envelopes, School/College site isolation,
password and account validation rules, cross-tenant data isolation asserted through the
HTTP boundary with distinct hosts, and the privilege boundaries around account
administration (no self-service privilege escalation, no cross-tenant visibility, no
self-lockout).

External deployment work is intentionally not embedded in source: production SQL and S3/CDN
credentials, real domains/DNS, trusted HTTPS certificates, hosting, monitoring destinations,
email/SMS providers and backup infrastructure must be supplied by the deployment environment.

## Samples

- SQL: [docs/sample-homepage.sql](docs/sample-homepage.sql)
- JSON response: [docs/sample-homepage-response.json](docs/sample-homepage-response.json)
