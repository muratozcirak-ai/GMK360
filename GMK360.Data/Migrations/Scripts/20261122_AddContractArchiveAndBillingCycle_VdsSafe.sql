-- Idempotent VDS / Production SQL update for migration 20261122103419_AddContractArchiveAndBillingCycle
IF COL_LENGTH(N'dbo.BuildingFixedExpenses', N'BillingCycleMonths') IS NULL
    ALTER TABLE [dbo].[BuildingFixedExpenses] ADD [BillingCycleMonths] int NOT NULL CONSTRAINT [DF_BFE_BillingCycleMonths] DEFAULT 1;
IF COL_LENGTH(N'dbo.BuildingFixedExpenses', N'BillingMonth') IS NULL
    ALTER TABLE [dbo].[BuildingFixedExpenses] ADD [BillingMonth] int NULL;
IF COL_LENGTH(N'dbo.BuildingFixedExpenses', N'ContractDocumentUrl') IS NULL
    ALTER TABLE [dbo].[BuildingFixedExpenses] ADD [ContractDocumentUrl] nvarchar(500) NULL;
IF COL_LENGTH(N'dbo.BuildingFixedExpenses', N'ContractEndDate') IS NULL
    ALTER TABLE [dbo].[BuildingFixedExpenses] ADD [ContractEndDate] datetime2 NULL;
IF COL_LENGTH(N'dbo.BuildingFixedExpenses', N'PhysicalArchiveLocation') IS NULL
    ALTER TABLE [dbo].[BuildingFixedExpenses] ADD [PhysicalArchiveLocation] nvarchar(250) NULL;
GO

UPDATE [dbo].[BuildingFixedExpenses]
SET [BillingCycleMonths] = 1
WHERE [BillingCycleMonths] <= 0;
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[__EFMigrationsHistory] WHERE [MigrationId] = N'20261122103419_AddContractArchiveAndBillingCycle')
BEGIN
    INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261122103419_AddContractArchiveAndBillingCycle', N'10.0.11');
END
GO
