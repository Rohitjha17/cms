# Vercel client demo

This deployment profile runs the API and Admin UI in one Vercel container. It
uses SQLite and local uploads under `/tmp`, so it does not require SQL Server or
Amazon S3.

## Deploy

1. Install and authenticate the Vercel CLI:

   ```bash
   npm install --global vercel
   vercel login
   ```

2. Deploy from the repository root:

   ```bash
   vercel --prod
   ```

Vercel detects `Dockerfile.vercel`, builds the container, and routes the public
URL to nginx. The Admin UI is at `/`, Swagger is at `/swagger`, and API routes
are under `/api`.

## Demo accounts

- Tenant admin: `admin@demo.local`
- Super admin: `superadmin@demo.local`
- Local default password: `Admin@12345`

Set `Seed__DemoAdminPassword` and `Jwt__Key` as Vercel environment variables
before sharing a public deployment. `Jwt__Key` must contain at least 32 bytes.

## Demo-only limitations

- Database changes and uploaded files disappear when the container is replaced
  or scaled to a new instance.
- Concurrent instances do not share data or authentication keys.
- Do not enter production, confidential, or personal data.
- Use SQL Server, S3, and shared Data Protection keys for a real deployment.
