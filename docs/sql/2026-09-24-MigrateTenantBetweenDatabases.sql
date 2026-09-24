/*
    Copy one institution — and everything inside it — from one database into another.

    Written for the live server, which has the earlier database (MultiSiteData) holding the
    schools that were built first, and the multi-tenant database (MutiTenantSite) the console now
    runs on. It copies an institution's websites, pages, home page sections, news/events/people,
    menus, SEO settings, media records, messages and addresses.

    ONE INSTITUTION AT A TIME, ON PURPOSE. Two things in this data must be unique across the whole
    platform, and both collide between these two databases:

      * a tenant's Code — both databases have 'platform'
      * a domain name  — site1/site4/site5/cms.shubhsoftsolution.com are in both

    So the institution is copied under a code you choose, and an address already present in the
    target is skipped and reported rather than silently moved. Everything is matched on Id, so
    running the script twice copies nothing twice.

    HOW TO RUN, on the server:
        1. BACK UP BOTH DATABASES FIRST. This writes into the live one.
        2. Fill in the five values under "WHAT TO CHANGE".
        3. Run PART 1 alone. It changes nothing; read every result.
        4. Only if PART 1 looks right, run PART 2, read what it prints, then COMMIT.
        5. Open the console, switch to the institution, and check a website opens.

    Media files are not copied: they already live in the S3 bucket, and the addresses stored with
    them are relative, so they keep working. Sign-in accounts are not copied either — see PART 3.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

-- ===========================================================================
-- WHAT TO CHANGE
-- ===========================================================================
DECLARE @OldDb      sysname        = N'MultiSiteData';    -- copy FROM
DECLARE @NewDb      sysname        = N'MutiTenantSite';   -- copy INTO
DECLARE @FromCode   nvarchar(50)   = N'platform';         -- the institution's code in @OldDb
DECLARE @ToCode     nvarchar(50)   = N'shubh-legacy';     -- code it gets in @NewDb (must be unused)
DECLARE @ToName     nvarchar(200)  = N'Shubh Soft (earlier websites)';  -- name it gets in @NewDb

-- ===========================================================================
-- PART 1 — look before touching anything
-- ===========================================================================
DECLARE @sql nvarchar(max);

-- 1a. The institution to copy, and what is in it.
SET @sql = N'
SELECT  t.Id AS TenantId, t.Name AS Institution, t.Code, t.IsActive,
        (SELECT COUNT(*) FROM ' + QUOTENAME(@OldDb) + N'.dbo.Sites s WHERE s.TenantId = t.Id) AS Websites,
        (SELECT COUNT(*) FROM ' + QUOTENAME(@OldDb) + N'.dbo.Pages p WHERE p.TenantId = t.Id) AS Pages,
        (SELECT COUNT(*) FROM ' + QUOTENAME(@OldDb) + N'.dbo.HomePageSections h WHERE h.TenantId = t.Id) AS Sections,
        (SELECT COUNT(*) FROM ' + QUOTENAME(@OldDb) + N'.dbo.ContentEntries c WHERE c.TenantId = t.Id) AS ContentRows,
        (SELECT COUNT(*) FROM ' + QUOTENAME(@OldDb) + N'.dbo.MediaFiles m WHERE m.TenantId = t.Id) AS MediaRows,
        (SELECT COUNT(*) FROM ' + QUOTENAME(@OldDb) + N'.dbo.TenantDomains d WHERE d.TenantId = t.Id) AS Addresses
FROM    ' + QUOTENAME(@OldDb) + N'.dbo.Tenants t
WHERE   t.Code = @FromCode;';
EXEC sp_executesql @sql, N'@FromCode nvarchar(50)', @FromCode;

-- 1b. Is the code you chose free in the target?
SET @sql = N'
SELECT CASE WHEN EXISTS (SELECT 1 FROM ' + QUOTENAME(@NewDb) + N'.dbo.Tenants WHERE Code = @ToCode)
            THEN ''STOP — @ToCode is already used in the target. Choose another.''
            ELSE ''OK — @ToCode is free.'' END AS CodeCheck;';
EXEC sp_executesql @sql, N'@ToCode nvarchar(50)', @ToCode;

-- 1c. Addresses: which will come across, which will be left behind.
SET @sql = N'
SELECT  d.DomainName AS [Address], d.IsActive,
        CASE WHEN EXISTS (SELECT 1 FROM ' + QUOTENAME(@NewDb) + N'.dbo.TenantDomains n WHERE n.DomainName = d.DomainName)
             THEN ''SKIPPED — this address already exists in the target''
             ELSE ''will be copied'' END AS Outcome
FROM    ' + QUOTENAME(@OldDb) + N'.dbo.TenantDomains d
        JOIN ' + QUOTENAME(@OldDb) + N'.dbo.Tenants t ON t.Id = d.TenantId
WHERE   t.Code = @FromCode
ORDER BY d.DomainName;';
EXEC sp_executesql @sql, N'@FromCode nvarchar(50)', @FromCode;

-- 1d. Rows that exist in BOTH databases under the same Id. There must be none: it would mean the
--     two databases share history, and copying would collide with rows already there.
SET @sql = N'
SELECT ''Sites'' AS [Table], COUNT(*) AS SameIdInBoth
FROM   ' + QUOTENAME(@OldDb) + N'.dbo.Sites o JOIN ' + QUOTENAME(@NewDb) + N'.dbo.Sites n ON n.Id = o.Id
UNION ALL SELECT ''Pages'', COUNT(*)
FROM   ' + QUOTENAME(@OldDb) + N'.dbo.Pages o JOIN ' + QUOTENAME(@NewDb) + N'.dbo.Pages n ON n.Id = o.Id
UNION ALL SELECT ''MediaFiles'', COUNT(*)
FROM   ' + QUOTENAME(@OldDb) + N'.dbo.MediaFiles o JOIN ' + QUOTENAME(@NewDb) + N'.dbo.MediaFiles n ON n.Id = o.Id;';
EXEC sp_executesql @sql;

PRINT 'PART 1 done. Read the four results above. If "SameIdInBoth" is not 0, stop and ask before running PART 2.';
GO


-- ===========================================================================
-- PART 2 — the copy. Run this only after PART 1 looked right.
-- ===========================================================================
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @OldDb      sysname        = N'MultiSiteData';
DECLARE @NewDb      sysname        = N'MutiTenantSite';
DECLARE @FromCode   nvarchar(50)   = N'platform';
DECLARE @ToCode     nvarchar(50)   = N'shubh-legacy';
DECLARE @ToName     nvarchar(200)  = N'Shubh Soft (earlier websites)';

DECLARE @TenantId uniqueidentifier;
DECLARE @sql nvarchar(max);

SET @sql = N'SELECT @id = Id FROM ' + QUOTENAME(@OldDb) + N'.dbo.Tenants WHERE Code = @FromCode;';
EXEC sp_executesql @sql, N'@FromCode nvarchar(50), @id uniqueidentifier OUTPUT', @FromCode, @TenantId OUTPUT;

IF @TenantId IS NULL
BEGIN
    RAISERROR('No institution with that code in the source database. Nothing was changed.', 16, 1);
    RETURN;
END

BEGIN TRANSACTION;

-- The institution itself, under the code and name you chose.
SET @sql = N'
INSERT INTO ' + QUOTENAME(@NewDb) + N'.dbo.Tenants (Id, Name, Code, LogoUrl, IsActive, CreatedDate, CreatedBy, UpdatedDate, UpdatedBy)
SELECT t.Id, @ToName, @ToCode, t.LogoUrl, t.IsActive, t.CreatedDate, t.CreatedBy, SYSUTCDATETIME(), ''migrated''
FROM   ' + QUOTENAME(@OldDb) + N'.dbo.Tenants t
WHERE  t.Id = @TenantId
  AND  NOT EXISTS (SELECT 1 FROM ' + QUOTENAME(@NewDb) + N'.dbo.Tenants n WHERE n.Id = t.Id);';
EXEC sp_executesql @sql,
     N'@TenantId uniqueidentifier, @ToCode nvarchar(50), @ToName nvarchar(200)',
     @TenantId, @ToCode, @ToName;
PRINT CONCAT('Institution rows copied: ', @@ROWCOUNT);

-- Everything that belongs to it, parents before children. Columns are read from the target, so a
-- column added later is carried across without editing this script.
DECLARE @plan TABLE (Seq int, TableName sysname, Extra nvarchar(max));
INSERT INTO @plan (Seq, TableName, Extra) VALUES
    (1,  'Sites',              NULL),
    (2,  'TenantDomains',      'AND NOT EXISTS (SELECT 1 FROM {new}.dbo.TenantDomains dup WHERE dup.DomainName = src.DomainName)'),
    (3,  'Menus',              NULL),
    (4,  'Pages',              NULL),
    (5,  'MenuItems',          NULL),
    (6,  'HomePageSections',   NULL),
    (7,  'ContentEntries',     NULL),
    (8,  'SeoSettings',        NULL),
    (9,  'MediaFiles',         NULL),
    (10, 'ContactSubmissions', NULL);

DECLARE @seq int = 1, @table sysname, @extra nvarchar(max), @cols nvarchar(max), @srcCols nvarchar(max);

WHILE EXISTS (SELECT 1 FROM @plan WHERE Seq = @seq)
BEGIN
    SELECT @table = TableName, @extra = ISNULL(Extra, N'') FROM @plan WHERE Seq = @seq;

    -- The column list is read from the target database's own catalogue: sys.columns only ever
    -- describes the database it is read from, so it has to be reached through that database's name.
    SET @sql = N'
SELECT @c = STRING_AGG(QUOTENAME(c.name), N'', '') WITHIN GROUP (ORDER BY c.column_id),
       @s = STRING_AGG(N''src.'' + QUOTENAME(c.name), N'', '') WITHIN GROUP (ORDER BY c.column_id)
FROM   ' + QUOTENAME(@NewDb) + N'.sys.columns c
WHERE  c.object_id = OBJECT_ID(''' + QUOTENAME(@NewDb) + N'.dbo.' + QUOTENAME(@table) + N''');';
    EXEC sp_executesql @sql,
         N'@c nvarchar(max) OUTPUT, @s nvarchar(max) OUTPUT', @cols OUTPUT, @srcCols OUTPUT;

    IF @cols IS NULL
    BEGIN
        RAISERROR('Could not read the columns of %s. Nothing is committed yet — run ROLLBACK.', 16, 1, @table);
        RETURN;
    END

    SET @sql = N'
INSERT INTO ' + QUOTENAME(@NewDb) + N'.dbo.' + QUOTENAME(@table) + N' (' + @cols + N')
SELECT ' + @srcCols + N'
FROM   ' + QUOTENAME(@OldDb) + N'.dbo.' + QUOTENAME(@table) + N' AS src
WHERE  src.TenantId = @TenantId
  AND  NOT EXISTS (SELECT 1 FROM ' + QUOTENAME(@NewDb) + N'.dbo.' + QUOTENAME(@table) + N' tgt WHERE tgt.Id = src.Id)
  ' + REPLACE(@extra, '{new}', QUOTENAME(@NewDb)) + N';';

    EXEC sp_executesql @sql, N'@TenantId uniqueidentifier', @TenantId;
    PRINT CONCAT(@table, ' rows copied: ', @@ROWCOUNT);

    SET @seq += 1;
END

-- What the target now holds for this institution.
SET @sql = N'
SELECT  t.Name AS Institution, t.Code, s.Name AS Website, s.SiteKey, s.IsActive, s.IsDefault,
        (SELECT COUNT(*) FROM ' + QUOTENAME(@NewDb) + N'.dbo.Pages p WHERE p.SiteId = s.Id) AS Pages,
        (SELECT COUNT(*) FROM ' + QUOTENAME(@NewDb) + N'.dbo.HomePageSections h WHERE h.SiteId = s.Id) AS Sections,
        (SELECT COUNT(*) FROM ' + QUOTENAME(@NewDb) + N'.dbo.MediaFiles m WHERE m.SiteId = s.Id) AS MediaRows
FROM    ' + QUOTENAME(@NewDb) + N'.dbo.Sites s
        JOIN ' + QUOTENAME(@NewDb) + N'.dbo.Tenants t ON t.Id = s.TenantId
WHERE   s.TenantId = @TenantId
ORDER BY s.Name;';
EXEC sp_executesql @sql, N'@TenantId uniqueidentifier', @TenantId;

PRINT 'Read the counts above. Right? Run COMMIT. Wrong? Run ROLLBACK.';
-- COMMIT;
-- ROLLBACK;
GO


-- ===========================================================================
-- PART 3 — sign-in accounts (optional, run after PART 2 is committed)
--
-- Accounts are not copied by PART 2 because an email address may exist in both databases, and an
-- account is cheap to create in the console — where it also gets a fresh set-your-password link.
-- If you would rather carry them over, this copies only the accounts of the institution just
-- migrated whose email is not already in the target, with their passwords intact.
-- ===========================================================================
/*
DECLARE @OldDb sysname = N'MultiSiteData';
DECLARE @NewDb sysname = N'MutiTenantSite';
DECLARE @FromCode nvarchar(50) = N'platform';
DECLARE @sql nvarchar(max);

SET @sql = N'
INSERT INTO ' + QUOTENAME(@NewDb) + N'.dbo.AspNetUsers
SELECT u.*
FROM   ' + QUOTENAME(@OldDb) + N'.dbo.AspNetUsers u
       JOIN ' + QUOTENAME(@OldDb) + N'.dbo.Tenants t ON t.Id = u.TenantId
WHERE  t.Code = @FromCode
  AND  NOT EXISTS (SELECT 1 FROM ' + QUOTENAME(@NewDb) + N'.dbo.AspNetUsers n WHERE n.NormalizedEmail = u.NormalizedEmail);

INSERT INTO ' + QUOTENAME(@NewDb) + N'.dbo.AspNetUserRoles (UserId, RoleId)
SELECT ur.UserId, r2.Id
FROM   ' + QUOTENAME(@OldDb) + N'.dbo.AspNetUserRoles ur
       JOIN ' + QUOTENAME(@OldDb) + N'.dbo.AspNetRoles r1 ON r1.Id = ur.RoleId
       JOIN ' + QUOTENAME(@NewDb) + N'.dbo.AspNetRoles r2 ON r2.NormalizedName = r1.NormalizedName
       JOIN ' + QUOTENAME(@NewDb) + N'.dbo.AspNetUsers nu ON nu.Id = ur.UserId
WHERE  NOT EXISTS (SELECT 1 FROM ' + QUOTENAME(@NewDb) + N'.dbo.AspNetUserRoles x
                   WHERE x.UserId = ur.UserId AND x.RoleId = r2.Id);';
EXEC sp_executesql @sql, N'@FromCode nvarchar(50)', @FromCode;
*/
