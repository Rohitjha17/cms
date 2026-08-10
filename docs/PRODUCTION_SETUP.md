# Production bootstrap

This is the shortest path from an empty SQL Server database to a working CMS with real
accounts. Everything here is driven by configuration — no code changes are required per
customer, and no secrets belong in `appsettings.json`.

## 1. Why a bootstrap step is needed

Tenant resolution is host-based: an incoming request is matched against `TenantDomains`,
and a host that matches nothing is refused. On an empty database no host matches and no
account exists, so nothing can be reached. `PlatformSeed` solves this by creating one
"platform" tenant bound to your own administration host, plus the first super
administrator. It runs on every start, is idempotent, and skips itself entirely when the
configuration below is absent.

## 2. Required configuration

Supply these as environment variables (or through your secret manager). Double underscores
map to configuration sections.

```bash
# Database
ConnectionStrings__DefaultConnection="Server=...;Database=CmsDb;..."
Database__ApplyMigrationsOnStartup="false"   # apply migrations as a deploy step instead

# API auth
Jwt__Key="at-least-32-random-bytes"
Cors__AllowedOrigins__0="https://admin.yourcompany.com"

# Platform console — this is what makes the CMS reachable at all
Platform__Domain="admin.yourcompany.com"
Platform__SuperAdminEmail="ops@yourcompany.com"
Platform__SuperAdminPassword="a-strong-initial-password"
Platform__SuperAdminName="Platform Administrator"
Platform__AdminBaseUrl="https://admin.yourcompany.com"   # used to build reset links from the API

# Outbound mail (optional but recommended)
Email__Host="smtp.yourprovider.com"
Email__Port="587"
Email__UseSsl="true"
Email__UserName="..."
Email__Password="..."
Email__FromAddress="no-reply@yourcompany.com"
Email__FromName="Your CMS"

# Media
Storage__Provider="S3"
Aws__AccessKey="..."
Aws__SecretKey="..."
Aws__Region="ap-south-1"
Aws__BucketName="your-bucket"
Aws__PublicBaseUrl="https://cdn.yourcompany.com"

# Behind a load balancer or reverse proxy
Proxy__TrustForwardedHeaders="true"
```

Point DNS for `admin.yourcompany.com` at the **Cms.Admin** application.

If `Email__Host` and `Email__FromAddress` are not set, mail is disabled cleanly: the CMS
shows a one-time password link on screen for the administrator to pass on by hand instead
of pretending an email was sent.

## 3. Deploy sequence

```bash
# 1. Apply migrations once, from a single machine
dotnet ef database update --project src/Cms.Infrastructure --startup-project src/Cms.Api

# 2. Start the applications with the configuration above
#    Cms.Admin  -> admin.yourcompany.com
#    Cms.Web    -> each school's public domain
#    Cms.Api    -> internal, or its own host if you expose it

# 3. Sign in at https://admin.yourcompany.com with Platform__SuperAdminEmail
# 4. Change that password immediately (avatar -> Change password)
```

Demo data stays off in production unless you explicitly set `Seed__EnableDemoData=true`,
so no `admin@demo.local` account is ever created on a real deployment.

## 4. Onboarding a school

1. **Tenants** → create the institution: name, code, its sites (School / College, each with
   a site key and home design) and its domains. Bind a domain to a site key when that host
   should serve only that website; leave it unbound to serve every site under `/school`
   and `/college` path prefixes.
2. **User accounts** → invite the school's administrator with the `TenantAdmin` role. Leave
   the password blank so they receive a set-your-password link rather than a shared secret.
3. Point the school's DNS at the **Cms.Web** application.
4. The school signs in on their own host and manages their own branding, pages, menus,
   media, contacts and SEO. They cannot see any other institution's data.

Optionally use **Websites** → provision to create an additional site for an existing tenant
with starter pages already assigned from the page gallery.

## 5. Roles

| Role | Scope | Can do |
|---|---|---|
| `SuperAdmin` | All tenants | Everything, including tenants, the page-gallery catalog and creating other super administrators |
| `TenantAdmin` | Own tenant | Manage that institution's websites, content, media, SEO and its own user accounts |
| `Editor` | Own tenant | Manage content, pages, media and navigation; no account administration |

Guard rails enforced server-side: a tenant administrator can never grant `SuperAdmin`,
never see or touch accounts outside their own tenant, and nobody can change their own role
or deactivate themselves. The last active administrator of an institution cannot be
demoted or disabled, so a workspace can never be locked out by one mistaken click.

## 6. Password flows

- **Invitation** — creating an account issues a single-use link. Emailed when SMTP is
  configured, otherwise shown once in the CMS for manual delivery.
- **Forgot password** — `/Account/ForgotPassword` on the Admin host. The response is
  identical whether or not the address exists, so the form cannot be used to discover who
  has an account. Rate limited per IP.
- **Admin-issued reset** — **User accounts** → *Reset link* generates a fresh single-use
  link for someone who cannot receive mail.
- **Change password** — any signed-in user, from the sidebar.
- **Lockout** — repeated failed sign-ins lock an account; an administrator can clear it
  with *Unlock*, and a completed reset clears it automatically.

## 7. Operational checks

```bash
dotnet test Cms.sln        # includes tenant-isolation and account-privilege coverage
curl https://api-host/health
```

Audit history for a tenant is available at `/api/activity-logs` (TenantAdmin and above);
every entity insert, update and delete is recorded with the acting user.
