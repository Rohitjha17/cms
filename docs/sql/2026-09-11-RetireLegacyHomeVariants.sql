BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260911071500_RetireLegacyHomeVariants'
)
BEGIN
    -- Classic -> Prestige, Modern -> Campus, Academic -> Atrium.
    UPDATE [Sites] SET [HomeVariant] = 5 WHERE [HomeVariant] = 1;
    UPDATE [Sites] SET [HomeVariant] = 3 WHERE [HomeVariant] = 2;
    UPDATE [Sites] SET [HomeVariant] = 7 WHERE [HomeVariant] = 4;

    -- Anything else outside the surviving set is a stray value; Prestige is the default.
    UPDATE [Sites] SET [HomeVariant] = 5 WHERE [HomeVariant] NOT IN (3, 5, 6, 7);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260911071500_RetireLegacyHomeVariants'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260911071500_RetireLegacyHomeVariants', N'8.0.11');
END;
GO

COMMIT;
GO
