BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260904111519_PageCustomHtml'
)
BEGIN
    ALTER TABLE [Pages] ADD [UseCustomHtml] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260904111519_PageCustomHtml'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260904111519_PageCustomHtml', N'8.0.11');
END;
GO

COMMIT;
GO

