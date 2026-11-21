-- Idempotent VDS / Production SQL update for migration 20261121111201_AddDuesRulesAndUnitExemptions
IF COL_LENGTH(N'dbo.BuildingUnits', N'CleaningParticipationPercent') IS NULL
    ALTER TABLE [dbo].[BuildingUnits] ADD [CleaningParticipationPercent] decimal(6,2) NOT NULL CONSTRAINT [DF_BuildingUnits_CleaningPct] DEFAULT 100.00;
IF COL_LENGTH(N'dbo.BuildingUnits', N'DuesMultiplier') IS NULL
    ALTER TABLE [dbo].[BuildingUnits] ADD [DuesMultiplier] decimal(8,2) NOT NULL CONSTRAINT [DF_BuildingUnits_DuesMult] DEFAULT 1.00;
IF COL_LENGTH(N'dbo.BuildingUnits', N'DuesRuleNote') IS NULL
    ALTER TABLE [dbo].[BuildingUnits] ADD [DuesRuleNote] nvarchar(max) NULL;
IF COL_LENGTH(N'dbo.BuildingUnits', N'FixedMonthlyDues') IS NULL
    ALTER TABLE [dbo].[BuildingUnits] ADD [FixedMonthlyDues] decimal(18,2) NULL;
IF COL_LENGTH(N'dbo.BuildingUnits', N'IsExemptFromElevator') IS NULL
    ALTER TABLE [dbo].[BuildingUnits] ADD [IsExemptFromElevator] bit NOT NULL CONSTRAINT [DF_BuildingUnits_ExElev] DEFAULT 0;
IF COL_LENGTH(N'dbo.BuildingUnits', N'IsExemptFromStairwell') IS NULL
    ALTER TABLE [dbo].[BuildingUnits] ADD [IsExemptFromStairwell] bit NOT NULL CONSTRAINT [DF_BuildingUnits_ExStair] DEFAULT 0;

IF COL_LENGTH(N'dbo.BuildingFixedExpenses', N'ExpenseItemKey') IS NULL
    ALTER TABLE [dbo].[BuildingFixedExpenses] ADD [ExpenseItemKey] nvarchar(40) NOT NULL CONSTRAINT [DF_BFE_ItemKey] DEFAULT N'Genel';
IF COL_LENGTH(N'dbo.BuildingFixedExpenses', N'OwnerSplitPercent') IS NULL
    ALTER TABLE [dbo].[BuildingFixedExpenses] ADD [OwnerSplitPercent] decimal(6,2) NULL;
IF COL_LENGTH(N'dbo.BuildingFixedExpenses', N'ScopeType') IS NULL
    ALTER TABLE [dbo].[BuildingFixedExpenses] ADD [ScopeType] int NOT NULL CONSTRAINT [DF_BFE_ScopeType] DEFAULT 2;
IF COL_LENGTH(N'dbo.BuildingFixedExpenses', N'ShopParticipationPercent') IS NULL
    ALTER TABLE [dbo].[BuildingFixedExpenses] ADD [ShopParticipationPercent] decimal(6,2) NOT NULL CONSTRAINT [DF_BFE_ShopPct] DEFAULT 100.00;

IF COL_LENGTH(N'dbo.BuildingExpenses', N'DistributionRule') IS NULL
    ALTER TABLE [dbo].[BuildingExpenses] ADD [DistributionRule] nvarchar(30) NOT NULL CONSTRAINT [DF_BE_DistRule] DEFAULT N'Esit';
IF COL_LENGTH(N'dbo.BuildingExpenses', N'ExpenseItemKey') IS NULL
    ALTER TABLE [dbo].[BuildingExpenses] ADD [ExpenseItemKey] nvarchar(40) NOT NULL CONSTRAINT [DF_BE_ItemKey] DEFAULT N'Genel';
IF COL_LENGTH(N'dbo.BuildingExpenses', N'OwnerSplitPercent') IS NULL
    ALTER TABLE [dbo].[BuildingExpenses] ADD [OwnerSplitPercent] decimal(6,2) NULL;
IF COL_LENGTH(N'dbo.BuildingExpenses', N'ShopParticipationPercent') IS NULL
    ALTER TABLE [dbo].[BuildingExpenses] ADD [ShopParticipationPercent] decimal(6,2) NOT NULL CONSTRAINT [DF_BE_ShopPct] DEFAULT 100.00;

IF NOT EXISTS (SELECT 1 FROM [dbo].[__EFMigrationsHistory] WHERE [MigrationId] = N'20261121111201_AddDuesRulesAndUnitExemptions')
BEGIN
    INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261121111201_AddDuesRulesAndUnitExemptions', N'10.0.11');
END
GO
