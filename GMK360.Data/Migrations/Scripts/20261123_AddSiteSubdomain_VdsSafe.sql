-- Idempotent VDS / Production SQL update for migration 20261123103237_AddSiteSubdomainAndIndependence
IF COL_LENGTH(N'dbo.HousingComplexes', N'IsIndependentSite') IS NULL
BEGIN
    ALTER TABLE [dbo].[HousingComplexes] ADD [IsIndependentSite] bit NOT NULL CONSTRAINT [DF_HousingComplexes_IsIndependentSite] DEFAULT (1);
END
GO

IF COL_LENGTH(N'dbo.HousingComplexes', N'Subdomain') IS NULL
BEGIN
    ALTER TABLE [dbo].[HousingComplexes] ADD [Subdomain] nvarchar(100) NULL;
END
GO

IF COL_LENGTH(N'dbo.Buildings', N'Subdomain') IS NULL
BEGIN
    ALTER TABLE [dbo].[Buildings] ADD [Subdomain] nvarchar(100) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[__EFMigrationsHistory] WHERE [MigrationId] = N'20261123103237_AddSiteSubdomainAndIndependence')
BEGIN
    INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261123103237_AddSiteSubdomainAndIndependence', N'10.0.11');
END
GO
