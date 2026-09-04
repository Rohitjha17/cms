# Database changes, and how they reach the server

## The short version

Migrations **do not run by themselves on your server.** They run automatically
only in development. In production the application starts, expects the new
column, and fails on the queries that use it — which looks like a feature
"disappearing" rather than like a database problem.

So every time a release adds a database change, one of these has to happen:

- run the SQL script in `docs/sql/` against the live database, **or**
- set `Database:ApplyMigrationsOnStartup` to `true` and let the application do
  it on its next start.

---

## Pending: PageCustomHtml (4 September 2026)

Adds one column, `Pages.UseCustomHtml`, for the switch that lets a school build
a page in its own HTML.

**Symptom if it is missing:** the guided fields on Pages — the gallery's
image rows, the disclosure documents — stop appearing, because every query
that reads a page fails on a column the database does not have.

### Option A — run the script (recommended)

1. **Back up the database first.**
2. Open `docs/sql/2026-09-04-PageCustomHtml.sql` in SQL Server Management
   Studio, connected to the CMS database.
3. Run it.

The script is idempotent: it checks whether the migration has already been
applied and does nothing if it has, so running it twice is safe. It adds one
`bit` column defaulting to `0` and nothing else — no data is altered or
removed.

### Option B — let the application apply it

In each application's `web.config`, alongside the other settings:

```xml
<environmentVariable name="Database__ApplyMigrationsOnStartup" value="true" />
```

Then restart the app pool. The application checks the database is in a state
it can migrate before it tries, and refuses with an explanation rather than
half-applying.

Leave this on and future releases apply themselves. Leave it off and every
release with a database change needs its script run by hand.

### Checking it worked

```sql
SELECT name FROM sys.columns WHERE object_id = OBJECT_ID('Pages');
```

`UseCustomHtml` should be in the list. Or:

```sql
SELECT MigrationId FROM __EFMigrationsHistory ORDER BY MigrationId;
```

`20260904111519_PageCustomHtml` should be the last row.

---

## Releasing, in order

1. `git pull origin main` on the server
2. **Back up the database**
3. Apply any pending script from `docs/sql/`
4. Publish **both** `Cms.Web` and `Cms.Admin` — never one without the other,
   because the console rebuilds the whole settings record when it saves and an
   older console silently drops the settings a newer site added
5. Keep each folder's `web.config`; publishing overwrites it, and it holds the
   connection string, the AWS keys and the JWT key
6. Recycle both application pools
7. Check: `curl -s -o /dev/null -w "%{http_code}" https://<console>/js/console-search.js`
   — `200` means the new build is serving
