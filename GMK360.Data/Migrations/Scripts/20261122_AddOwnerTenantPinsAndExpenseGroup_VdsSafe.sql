-- Idempotent VDS / Production SQL update for migration 20261122084648_AddOwnerTenantPinsAndExpenseGroup
IF COL_LENGTH(N'dbo.BuildingUnits', N'OwnerQuickPinHash') IS NULL
    ALTER TABLE [dbo].[BuildingUnits] ADD [OwnerQuickPinHash] nvarchar(max) NULL;
IF COL_LENGTH(N'dbo.BuildingUnits', N'TenantQuickPinHash') IS NULL
    ALTER TABLE [dbo].[BuildingUnits] ADD [TenantQuickPinHash] nvarchar(max) NULL;

IF COL_LENGTH(N'dbo.BuildingFixedExpenses', N'ExpenseGroup') IS NULL
    ALTER TABLE [dbo].[BuildingFixedExpenses] ADD [ExpenseGroup] nvarchar(40) NOT NULL CONSTRAINT [DF_BFE_ExpenseGroup] DEFAULT N'FaturaAbonelik';
IF COL_LENGTH(N'dbo.BuildingExpenses', N'ExpenseGroup') IS NULL
    ALTER TABLE [dbo].[BuildingExpenses] ADD [ExpenseGroup] nvarchar(40) NOT NULL CONSTRAINT [DF_BE_ExpenseGroup] DEFAULT N'FaturaAbonelik';
GO

-- Backfill existing records created before ExpenseGroup existed
UPDATE [dbo].[BuildingExpenses]
SET [ExpenseGroup] = CASE
    WHEN [ExpenseType] = 1 OR [ExpenseItemKey] = N'Demirbas' THEN N'DemirbasYatirim'
    WHEN [ExpenseItemKey] = N'Personel' THEN N'Personel'
    WHEN [ExpenseItemKey] IN (N'Asansor', N'Merdiven', N'CopTemizlik') THEN N'BakimOnarim'
    ELSE N'FaturaAbonelik'
END
WHERE [ExpenseGroup] = N'' OR [ExpenseGroup] IS NULL;

UPDATE [dbo].[BuildingFixedExpenses]
SET [ExpenseGroup] = CASE
    WHEN [ExpenseScope] = N'Demirbas' OR [ExpenseItemKey] = N'Demirbas' THEN N'DemirbasYatirim'
    WHEN [ExpenseItemKey] = N'Personel' THEN N'Personel'
    WHEN [ExpenseItemKey] IN (N'Asansor', N'Merdiven', N'CopTemizlik') THEN N'BakimOnarim'
    ELSE N'FaturaAbonelik'
END
WHERE [ExpenseGroup] = N'' OR [ExpenseGroup] IS NULL;
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[__EFMigrationsHistory] WHERE [MigrationId] = N'20261122084648_AddOwnerTenantPinsAndExpenseGroup')
BEGIN
    INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261122084648_AddOwnerTenantPinsAndExpenseGroup', N'10.0.11');
END
GO
