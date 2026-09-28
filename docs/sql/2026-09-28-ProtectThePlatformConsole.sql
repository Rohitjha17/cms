/*
    Stop the platform console from being deleted or switched off.

    The console has been lost more than once: its website removed from the Websites screen, or its
    address switched off. Every screen that could put any of that back is inside the console
    itself, so losing it locks the operator out with no way back in but SQL.

    These three triggers refuse the change at the database, whoever makes it — the console, a
    script, or somebody in SSMS. The caller sees an error naming what was refused and why.

    Protected:
      * the tenant with code 'platform'        — delete, or IsActive = 0
      * the website with key 'platform' in it  — delete, or IsActive = 0
      * the console's own address              — delete, or IsActive = 0

    Nothing else: a school's own institution, websites and addresses stay entirely deletable.

    BEFORE RUNNING: if the console is not on cms.shubhsoftsolution.com, change the host in
    trigger 3 below — it appears twice.

    HOW TO RUN: open this file in SSMS and run it. Running it again is safe; each trigger is
    replaced. To remove the protection later:

        USE [MultiSiteData];
        DROP TRIGGER dbo.trgProtectPlatformTenant;
        DROP TRIGGER dbo.trgProtectPlatformSite;
        DROP TRIGGER dbo.trgProtectConsoleDomain;
*/

USE [MultiSiteData];   -- <<< the database the console runs on
GO

-- ---------------------------------------------------------------------------
-- 1. The platform institution
-- ---------------------------------------------------------------------------
CREATE OR ALTER TRIGGER dbo.trgProtectPlatformTenant
ON dbo.Tenants
AFTER DELETE, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM deleted d
               WHERE d.Code = 'platform'
                 AND NOT EXISTS (SELECT 1 FROM inserted i WHERE i.Id = d.Id))
    BEGIN
        ROLLBACK TRANSACTION;
        THROW 51001, 'The platform console institution cannot be deleted. It is the institution the console itself signs in to.', 1;
    END

    IF EXISTS (SELECT 1 FROM inserted i
               JOIN deleted d ON d.Id = i.Id
               WHERE i.Code = 'platform' AND i.IsActive = 0 AND d.IsActive = 1)
    BEGIN
        ROLLBACK TRANSACTION;
        THROW 51002, 'The platform console institution cannot be switched off. Everything that could switch it back on is inside the console.', 1;
    END
END
GO

-- ---------------------------------------------------------------------------
-- 2. The console's own website
-- ---------------------------------------------------------------------------
CREATE OR ALTER TRIGGER dbo.trgProtectPlatformSite
ON dbo.Sites
AFTER DELETE, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM deleted d
               JOIN dbo.Tenants t ON t.Id = d.TenantId
               WHERE t.Code = 'platform' AND d.SiteKey = 'platform'
                 AND NOT EXISTS (SELECT 1 FROM inserted i WHERE i.Id = d.Id))
    BEGIN
        ROLLBACK TRANSACTION;
        THROW 51003, 'The platform console website cannot be deleted. The console needs it to open its own screens.', 1;
    END

    IF EXISTS (SELECT 1 FROM inserted i
               JOIN deleted d ON d.Id = i.Id
               JOIN dbo.Tenants t ON t.Id = i.TenantId
               WHERE t.Code = 'platform' AND i.SiteKey = 'platform'
                 AND i.IsActive = 0 AND d.IsActive = 1)
    BEGIN
        ROLLBACK TRANSACTION;
        THROW 51004, 'The platform console website cannot be closed.', 1;
    END
END
GO

-- ---------------------------------------------------------------------------
-- 3. The console's own address
--    Change 'cms.shubhsoftsolution.com' below — both places — if the console moves.
-- ---------------------------------------------------------------------------
CREATE OR ALTER TRIGGER dbo.trgProtectConsoleDomain
ON dbo.TenantDomains
AFTER DELETE, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM deleted d
               WHERE d.DomainName = 'cms.shubhsoftsolution.com'
                 AND NOT EXISTS (SELECT 1 FROM inserted i WHERE i.Id = d.Id))
    BEGIN
        ROLLBACK TRANSACTION;
        THROW 51005, 'The console address cannot be removed. Without it the console cannot be opened at all.', 1;
    END

    IF EXISTS (SELECT 1 FROM inserted i
               JOIN deleted d ON d.Id = i.Id
               WHERE i.DomainName = 'cms.shubhsoftsolution.com'
                 AND i.IsActive = 0 AND d.IsActive = 1)
    BEGIN
        ROLLBACK TRANSACTION;
        THROW 51006, 'The console address cannot be switched off.', 1;
    END
END
GO

-- ---------------------------------------------------------------------------
-- 4. Check all three are in place
-- ---------------------------------------------------------------------------
SELECT name AS TriggerName, is_disabled
FROM   sys.triggers
WHERE  name IN ('trgProtectPlatformTenant', 'trgProtectPlatformSite', 'trgProtectConsoleDomain');
GO

/*
    5. Try it, safely. Each of these must fail with the message above, and the ROLLBACK leaves
       nothing changed either way.

    BEGIN TRANSACTION;
    UPDATE TenantDomains SET IsActive = 0 WHERE DomainName = 'cms.shubhsoftsolution.com';
    ROLLBACK;

    BEGIN TRANSACTION;
    DELETE FROM Sites WHERE SiteKey = 'platform';
    ROLLBACK;
*/
