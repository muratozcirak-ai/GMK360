-- Idempotent VDS / Production SQL update for migration 20261123113639_AddBaseDuesAndUnitExemptions
IF COL_LENGTH(N'dbo.Buildings', N'DefaultBaseMonthlyDues') IS NULL
    ALTER TABLE [dbo].[Buildings] ADD [DefaultBaseMonthlyDues] decimal(18,2) NULL;
GO

IF COL_LENGTH(N'dbo.Buildings', N'DuesPaymentDay') IS NULL
    ALTER TABLE [dbo].[Buildings] ADD [DuesPaymentDay] int NULL;
GO

IF COL_LENGTH(N'dbo.BuildingUnits', N'IsDuesExempt') IS NULL
    ALTER TABLE [dbo].[BuildingUnits] ADD [IsDuesExempt] bit NOT NULL CONSTRAINT [DF_BuildingUnits_IsDuesExempt] DEFAULT (0);
GO

IF COL_LENGTH(N'dbo.BuildingUnits', N'DuesAdjustmentAmount') IS NULL
    ALTER TABLE [dbo].[BuildingUnits] ADD [DuesAdjustmentAmount] decimal(18,2) NOT NULL CONSTRAINT [DF_BuildingUnits_DuesAdjustmentAmount] DEFAULT (0);
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[__EFMigrationsHistory] WHERE [MigrationId] = N'20261123113639_AddBaseDuesAndUnitExemptions')
BEGIN
    INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261123113639_AddBaseDuesAndUnitExemptions', N'10.0.11');
END
GO
