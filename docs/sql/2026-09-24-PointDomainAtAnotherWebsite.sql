/*
    Point one address at one website.

    For the case where an address was added against the institution rather than against one of its
    websites ("All websites"). Such an address opens the institution's default website, and a
    website created afterwards is not the default — so the address keeps opening the old website
    whatever is done on the new one.

    The console does this without SQL: Domains -> Edit -> Serves -> the website -> Save domain, or
    Websites -> Make default. Use this only when the console is not available.

    HOW TO RUN, on the server:
        1. Fill in the two values under "WHAT TO CHANGE" below.
        2. Run STEP 1 on its own and read what it prints.
        3. Only if STEP 1 looks right, run STEP 2.
        4. Wait 30 seconds before refreshing the website: the address is cached that long.

    Nothing here deletes a row.
*/

USE MutiTenantSite;
GO

-- ---------------------------------------------------------------------------
-- WHAT TO CHANGE
-- ---------------------------------------------------------------------------
DECLARE @DomainName nvarchar(255) = 'site4.shubhsoftsolution.com';  -- the address
DECLARE @SiteKey    nvarchar(50)  = 'new';                          -- key of the website it should open

-- ---------------------------------------------------------------------------
-- STEP 1 — look before changing anything
-- ---------------------------------------------------------------------------
SELECT  d.DomainName,
        d.IsActive              AS AddressIsLive,
        t.Name                  AS Institution,
        ISNULL(s.Name, '(all websites — opens the default one)') AS OpensNow
FROM    TenantDomains d
        JOIN Tenants t ON t.Id = d.TenantId
        LEFT JOIN Sites s ON s.Id = d.SiteId
WHERE   d.DomainName = @DomainName;

-- Every website of that address's institution. Check the key you chose is in this list, is
-- Active, and belongs to the same institution as the address above.
SELECT  s.SiteKey, s.Name AS Website, s.IsActive, s.IsDefault, t.Name AS Institution,
        (SELECT COUNT(*) FROM Pages p WHERE p.SiteId = s.Id) AS Pages
FROM    Sites s
        JOIN Tenants t ON t.Id = s.TenantId
WHERE   s.TenantId = (SELECT TOP 1 TenantId FROM TenantDomains WHERE DomainName = @DomainName)
ORDER BY s.Name;
GO

-- ---------------------------------------------------------------------------
-- STEP 2 — bind the address to that website
--
-- Runs inside a transaction and prints the result before committing. The join on TenantId is
-- deliberate: it makes it impossible to bind an address to a website of another institution,
-- which would serve one school's website on another school's address. If it reports 0 rows
-- changed, the website key belongs to a different institution — stop, and check STEP 1 again.
-- ---------------------------------------------------------------------------
DECLARE @DomainName nvarchar(255) = 'site4.shubhsoftsolution.com';
DECLARE @SiteKey    nvarchar(50)  = 'new';

BEGIN TRANSACTION;

UPDATE  d
SET     d.SiteId = s.Id,
        d.UpdatedDate = SYSUTCDATETIME(),
        d.UpdatedBy = 'sql-fix'
FROM    TenantDomains d
        JOIN Sites s ON s.TenantId = d.TenantId AND s.SiteKey = @SiteKey
WHERE   d.DomainName = @DomainName;

PRINT CONCAT('Rows changed: ', @@ROWCOUNT);

SELECT  d.DomainName, s.Name AS NowOpens, s.IsActive AS WebsiteIsLive
FROM    TenantDomains d
        LEFT JOIN Sites s ON s.Id = d.SiteId
WHERE   d.DomainName = @DomainName;

-- Right? Keep it:
COMMIT;
-- Wrong? Undo it instead, by running this on its own:
-- ROLLBACK;
GO
