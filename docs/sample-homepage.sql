-- Sample HomePageSections schema + seed (aligned with EF migration)

IF OBJECT_ID(N'dbo.HomePageSections', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.HomePageSections
    (
        Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_HomePageSections PRIMARY KEY,
        TenantId UNIQUEIDENTIFIER NOT NULL,
        SiteId UNIQUEIDENTIFIER NOT NULL,
        SectionKey NVARCHAR(100) NOT NULL,
        Title NVARCHAR(250) NULL,
        SubTitle NVARCHAR(500) NULL,
        Description NVARCHAR(MAX) NULL,
        ButtonText NVARCHAR(100) NULL,
        ButtonLink NVARCHAR(1000) NULL,
        ImageUrl NVARCHAR(1000) NULL,
        BackgroundImageUrl NVARCHAR(1000) NULL,
        JsonData NVARCHAR(MAX) NULL,
        DisplayOrder INT NOT NULL,
        IsActive BIT NOT NULL,
        CreatedDate DATETIME2 NOT NULL,
        UpdatedDate DATETIME2 NULL,
        CreatedBy NVARCHAR(MAX) NULL,
        UpdatedBy NVARCHAR(MAX) NULL,
        CONSTRAINT UQ_HomePageSections_Tenant_Site_Key UNIQUE (TenantId, SiteId, SectionKey)
    );

    CREATE INDEX IX_HomePageSections_TenantId_SiteId_DisplayOrder
        ON dbo.HomePageSections (TenantId, SiteId, DisplayOrder);
END
GO

-- Demo IDs (match DatabaseSeeder)
DECLARE @TenantId UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111111';
DECLARE @SiteId   UNIQUEIDENTIFIER = '22222222-2222-2222-2222-222222222221';

MERGE dbo.HomePageSections AS target
USING (VALUES
    (NEWID(), @TenantId, @SiteId, 'hero', 'Hero Banner', 'Welcome to Demo Academy', NULL,
     'Apply Now', '/admissions', NULL, NULL,
     N'{"heading":"Welcome to Demo Academy","description":"Future Begins Here","primaryButton":"Apply Now","secondaryButton":"Contact Us","videoUrl":""}',
     1, 1),
    (NEWID(), @TenantId, @SiteId, 'about', 'About School', NULL, N'<p>We nurture excellence.</p>',
     NULL, NULL, NULL, NULL, NULL, 3, 1),
    (NEWID(), @TenantId, @SiteId, 'statistics', 'Statistics', NULL, NULL,
     NULL, NULL, NULL, NULL,
     N'{"students":1500,"teachers":80,"placements":500,"years":20}',
     6, 1),
    (NEWID(), @TenantId, @SiteId, 'gallery', 'Gallery', NULL, NULL,
     NULL, NULL, NULL, NULL, N'{"items":[]}', 13, 0),
    (NEWID(), @TenantId, @SiteId, 'testimonials', 'Testimonials', NULL, NULL,
     NULL, NULL, NULL, NULL, N'{"items":[]}', 15, 0),
    (NEWID(), @TenantId, @SiteId, 'contact', 'Contact Section', NULL, NULL,
     'Contact Us', '/contact', NULL, NULL,
     N'{"email":"info@demo.local","phone":"+1-555-0100","address":"123 Education Lane","mapEmbedUrl":""}',
     19, 1)
) AS src (Id, TenantId, SiteId, SectionKey, Title, SubTitle, Description, ButtonText, ButtonLink, ImageUrl, BackgroundImageUrl, JsonData, DisplayOrder, IsActive)
ON target.TenantId = src.TenantId AND target.SiteId = src.SiteId AND target.SectionKey = src.SectionKey
WHEN NOT MATCHED THEN
    INSERT (Id, TenantId, SiteId, SectionKey, Title, SubTitle, Description, ButtonText, ButtonLink, ImageUrl, BackgroundImageUrl, JsonData, DisplayOrder, IsActive, CreatedDate, CreatedBy)
    VALUES (src.Id, src.TenantId, src.SiteId, src.SectionKey, src.Title, src.SubTitle, src.Description, src.ButtonText, src.ButtonLink, src.ImageUrl, src.BackgroundImageUrl, src.JsonData, src.DisplayOrder, src.IsActive, SYSUTCDATETIME(), 'sample-sql');
GO
