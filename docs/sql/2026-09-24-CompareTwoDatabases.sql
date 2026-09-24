/*
    Everything that decides what a web address opens, from two databases at once, side by side.

    For comparing a database that answers "No active website is configured for this domain" with one
    that works. That message comes from one lookup and one lookup only: an address row that is live,
    whose institution is live. If the address is missing, switched off, or its institution is
    switched off, every visitor gets that message.

    HOW TO RUN, on the server:
        1. Put the two database names in @Old and @New below.
        2. Run the whole file once. It only reads — nothing is changed.
        3. Read the results in order; section 1 is the answer to "why does the old one refuse".

    Both databases must be on this same SQL Server instance.
*/

SET NOCOUNT ON;

DECLARE @Old sysname = N'MutiTenantSite_Old';   -- <<< the database that refuses
DECLARE @New sysname = N'MutiTenantSite';       -- <<< the database that works

DECLARE @sql nvarchar(max);
DECLARE @template nvarchar(max);

-- ===========================================================================
-- 1. Addresses, and whether each one can actually be reached
-- ===========================================================================
SET @template = N'
SELECT  ''{label}'' AS Source,
        d.DomainName                                   AS [Address],
        CASE WHEN d.IsActive = 1 AND t.IsActive = 1 AND s.Id IS NOT NULL THEN ''Opens '' + s.Name
             WHEN d.IsActive = 1 AND t.IsActive = 1 THEN ''Opens the default website''
             WHEN d.IsActive = 0 THEN ''REFUSED — this address is switched off''
             ELSE ''REFUSED — the institution is switched off''
        END                                            AS [What a visitor gets],
        d.IsActive                                     AS AddressIsLive,
        t.Name                                         AS Institution,
        t.IsActive                                     AS InstitutionIsLive,
        ISNULL(s.Name, ''(not bound — default website)'') AS BoundWebsite,
        s.IsActive                                     AS WebsiteIsLive,
        d.IsPrimary
FROM    {db}.dbo.TenantDomains d
        JOIN {db}.dbo.Tenants t ON t.Id = d.TenantId
        LEFT JOIN {db}.dbo.Sites s ON s.Id = d.SiteId';

SET @sql = REPLACE(REPLACE(@template, '{db}', QUOTENAME(@Old)), '{label}', 'OLD')
         + N' UNION ALL '
         + REPLACE(REPLACE(@template, '{db}', QUOTENAME(@New)), '{label}', 'NEW')
         + N' ORDER BY Source DESC, [Address];';
EXEC sp_executesql @sql;

-- ===========================================================================
-- 2. Institutions, and the website each one opens by default
-- ===========================================================================
SET @template = N'
SELECT  ''{label}'' AS Source,
        t.Name AS Institution, t.Code, t.IsActive AS InstitutionIsLive,
        (SELECT COUNT(*) FROM {db}.dbo.Sites s WHERE s.TenantId = t.Id) AS Websites,
        (SELECT COUNT(*) FROM {db}.dbo.TenantDomains d WHERE d.TenantId = t.Id) AS Addresses,
        ISNULL((SELECT TOP 1 s.Name FROM {db}.dbo.Sites s
                WHERE s.TenantId = t.Id AND s.IsDefault = 1 AND s.IsActive = 1),
               ''(none — an unbound address opens nothing)'') AS DefaultWebsite
FROM    {db}.dbo.Tenants t';

SET @sql = REPLACE(REPLACE(@template, '{db}', QUOTENAME(@Old)), '{label}', 'OLD')
         + N' UNION ALL '
         + REPLACE(REPLACE(@template, '{db}', QUOTENAME(@New)), '{label}', 'NEW')
         + N' ORDER BY Source DESC, Institution;';
EXEC sp_executesql @sql;

-- ===========================================================================
-- 3. Websites, and how much is actually in each one
--    A website with 0 pages opens its home page and 404s everywhere else.
--    A website with 0 sections opens a blank home page.
-- ===========================================================================
SET @template = N'
SELECT  ''{label}'' AS Source,
        t.Name AS Institution, s.Name AS Website, s.SiteKey, s.WebsiteType, s.HomeVariant,
        s.IsActive AS WebsiteIsLive, s.IsDefault,
        (SELECT COUNT(*) FROM {db}.dbo.Pages p WHERE p.SiteId = s.Id) AS Pages,
        (SELECT COUNT(*) FROM {db}.dbo.HomePageSections h WHERE h.SiteId = s.Id) AS HomeSections,
        (SELECT COUNT(*) FROM {db}.dbo.ContentEntries c WHERE c.SiteId = s.Id) AS NewsEventsPeople,
        (SELECT COUNT(*) FROM {db}.dbo.MenuItems m WHERE m.SiteId = s.Id) AS MenuLinks,
        (SELECT COUNT(*) FROM {db}.dbo.MediaFiles f WHERE f.SiteId = s.Id) AS MediaFiles,
        (SELECT COUNT(*) FROM {db}.dbo.TenantDomains d WHERE d.SiteId = s.Id) AS BoundAddresses
FROM    {db}.dbo.Sites s
        JOIN {db}.dbo.Tenants t ON t.Id = s.TenantId';

SET @sql = REPLACE(REPLACE(@template, '{db}', QUOTENAME(@Old)), '{label}', 'OLD')
         + N' UNION ALL '
         + REPLACE(REPLACE(@template, '{db}', QUOTENAME(@New)), '{label}', 'NEW')
         + N' ORDER BY Source DESC, Institution, Website;';
EXEC sp_executesql @sql;

-- ===========================================================================
-- 4. Who can sign in
-- ===========================================================================
SET @template = N'
SELECT  ''{label}'' AS Source,
        u.Email, u.IsActive AS CanSignIn,
        ISNULL(t.Name, ''(platform — every institution)'') AS Institution,
        STUFF((SELECT '', '' + r.Name
               FROM {db}.dbo.AspNetUserRoles ur
                    JOIN {db}.dbo.AspNetRoles r ON r.Id = ur.RoleId
               WHERE ur.UserId = u.Id FOR XML PATH('''')), 1, 2, '''') AS Roles
FROM    {db}.dbo.AspNetUsers u
        LEFT JOIN {db}.dbo.Tenants t ON t.Id = u.TenantId';

SET @sql = REPLACE(REPLACE(@template, '{db}', QUOTENAME(@Old)), '{label}', 'OLD')
         + N' UNION ALL '
         + REPLACE(REPLACE(@template, '{db}', QUOTENAME(@New)), '{label}', 'NEW')
         + N' ORDER BY Source DESC, Email;';
EXEC sp_executesql @sql;

-- ===========================================================================
-- 5. One line per database, to see at a glance which one holds the real content
-- ===========================================================================
SET @template = N'
SELECT  ''{label}'' AS Source, ''{db}'' AS [Database],
        (SELECT COUNT(*) FROM {db}.dbo.Tenants)        AS Institutions,
        (SELECT COUNT(*) FROM {db}.dbo.Sites)          AS Websites,
        (SELECT COUNT(*) FROM {db}.dbo.TenantDomains)  AS Addresses,
        (SELECT COUNT(*) FROM {db}.dbo.Pages)          AS Pages,
        (SELECT COUNT(*) FROM {db}.dbo.HomePageSections) AS HomeSections,
        (SELECT COUNT(*) FROM {db}.dbo.ContentEntries) AS ContentRows,
        (SELECT COUNT(*) FROM {db}.dbo.MediaFiles)     AS MediaFiles,
        (SELECT COUNT(*) FROM {db}.dbo.AspNetUsers)    AS Users,
        (SELECT COUNT(*) FROM {db}.dbo.__EFMigrationsHistory) AS MigrationsApplied';

SET @sql = REPLACE(REPLACE(@template, '{db}', QUOTENAME(@Old)), '{label}', 'OLD')
         + N' UNION ALL '
         + REPLACE(REPLACE(@template, '{db}', QUOTENAME(@New)), '{label}', 'NEW')
         + N';';
EXEC sp_executesql @sql;

-- ===========================================================================
-- 6. Database changes applied to each, so a structure difference shows up
-- ===========================================================================
SET @template = N'
SELECT ''{label}'' AS Source, MigrationId FROM {db}.dbo.__EFMigrationsHistory';

SET @sql = REPLACE(REPLACE(@template, '{db}', QUOTENAME(@Old)), '{label}', 'OLD')
         + N' UNION ALL '
         + REPLACE(REPLACE(@template, '{db}', QUOTENAME(@New)), '{label}', 'NEW')
         + N' ORDER BY MigrationId, Source DESC;';
EXEC sp_executesql @sql;
