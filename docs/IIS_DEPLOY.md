# Deploying to Windows Server + IIS

Written for a fresh Windows Server 2022 box (an AWS EC2 instance, for example) that will run
SQL Server, IIS and the three applications together.

## 1. Publish on the build machine

Publish for Windows only. A portable publish carries Linux and macOS runtime files that this
server never loads — roughly twice the size and twice the copy time over a remote desktop.

```bash
dotnet publish src/Cms.Web   -c Release -r win-x64 --self-contained false -o publish/web
dotnet publish src/Cms.Admin -c Release -r win-x64 --self-contained false -o publish/admin
dotnet publish src/Cms.Api   -c Release -r win-x64 --self-contained false -o publish/api
```

Each folder must contain a `web.config`. If it does not, IIS cannot start the application.

## 2. Install the ASP.NET Core Hosting Bundle

The runtime alone is not enough: IIS also needs the ASP.NET Core Module, which only the
Hosting Bundle installs. Without it every request returns HTTP 500.19 or 500.21.

```powershell
Test-Path "C:\Program Files\IIS\Asp.Net Core Module\V2\aspnetcorev2.dll"
```

`False` means it is missing:

```powershell
Invoke-WebRequest https://aka.ms/dotnet/8.0/dotnet-hosting-win.exe -OutFile "$env:USERPROFILE\Downloads\hosting.exe"
```

Run that installer, then `iisreset`.

## 3. Copy the folders into place

```
C:\inetpub\cms\web
C:\inetpub\cms\admin
C:\inetpub\cms\api
```

## 4. Create the database

The applications create every table themselves on first start. They will **not** touch a
database that already has tables but no migration history — a half-built database from an
earlier failed attempt has to go first:

```sql
DROP DATABASE IF EXISTS CmsDb;
CREATE DATABASE CmsDb;
```

## 5. Put the settings into each web.config

Configuration goes in `web.config` — not in `appsettings.json`, which is overwritten by the
next publish and is tracked in Git. Open each folder's `web.config` and place an
`<environmentVariables>` block inside the existing `<aspNetCore>` element:

```xml
<aspNetCore processPath="dotnet" arguments=".\Cms.Web.dll" stdoutLogEnabled="false"
            stdoutLogFile=".\logs\stdout" hostingModel="inprocess">
  <environmentVariables>
    <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />

    <!-- SQL Server is on this same machine, so localhost — not the public IP. -->
    <environmentVariable name="ConnectionStrings__DefaultConnection"
      value="Server=localhost,1433;Database=CmsDb;User Id=sa;Password=YOUR-SQL-PASSWORD;TrustServerCertificate=True;MultipleActiveResultSets=true" />
    <environmentVariable name="Database__Provider" value="SqlServer" />
    <environmentVariable name="Database__ApplyMigrationsOnStartup" value="true" />

    <!-- The host typed into the browser, without the port. -->
    <environmentVariable name="Platform__Domain" value="YOUR-DOMAIN-OR-IP" />
    <environmentVariable name="Platform__SuperAdminEmail" value="admin@example.com" />
    <environmentVariable name="Platform__SuperAdminPassword" value="A-STRONG-PASSWORD" />
    <environmentVariable name="Platform__SuperAdminName" value="Platform Administrator" />

    <!-- Uploads. The region must be the bucket's own region. -->
    <environmentVariable name="Storage__Provider" value="S3" />
    <environmentVariable name="Aws__BucketName" value="YOUR-BUCKET" />
    <environmentVariable name="Aws__Region" value="ap-south-1" />
    <environmentVariable name="Aws__AccessKey" value="YOUR-ACCESS-KEY" />
    <environmentVariable name="Aws__SecretKey" value="YOUR-SECRET-KEY" />
  </environmentVariables>
</aspNetCore>
```

The same block goes in all three, changing only `arguments` — `.\Cms.Web.dll`,
`.\Cms.Admin.dll`, `.\Cms.Api.dll`. The API needs one more:

```xml
<environmentVariable name="Jwt__Key" value="AT-LEAST-32-RANDOM-CHARACTERS" />
```

Anything required but missing stops the application on startup with a message naming it, so a
half-configured deployment cannot quietly serve a school demo content. Read that message in
Event Viewer → Windows Logs → Application.

## 6. Create the sites in IIS

IIS Manager → **Sites** → right-click → **Add Website**:

| Site name | Physical path | Port |
| --- | --- | --- |
| cms-web | `C:\inetpub\cms\web` | 80 |
| cms-admin | `C:\inetpub\cms\admin` | 8081 |
| cms-api | `C:\inetpub\cms\api` | 8082 |

Then **Application Pools** → each of the three pools → **Basic Settings** → .NET CLR version
= **No Managed Code**. The applications carry their own runtime; the managed pipeline is for
.NET Framework and will fail here.

## 7. Permissions and firewall

```powershell
icacls "C:\inetpub\cms" /grant "IIS_IUSRS:(OI)(CI)RX" /T
New-NetFirewallRule -DisplayName "HTTP"  -Direction Inbound -LocalPort 80   -Protocol TCP -Action Allow
New-NetFirewallRule -DisplayName "Admin" -Direction Inbound -LocalPort 8081 -Protocol TCP -Action Allow
```

On EC2 the instance's **security group** must allow the same ports, or the Windows firewall
rule alone changes nothing.

## 8. Check it

- Console: `http://YOUR-IP:8081` — sign in with `Platform__SuperAdminEmail`.
- Website: `http://YOUR-IP/` and each school at `http://YOUR-IP/site/<site-key>`.

A 502.5 means the application refused to start; the reason is in Event Viewer, or set
`stdoutLogEnabled="true"` and read `logs\stdout*.log`.

## 9. Once a domain is pointed at the server

Change `Platform__Domain` to that domain, add an HTTPS binding in IIS (a certificate from
win-acme is free), and add each school's own domain in the console under **Domains**.
