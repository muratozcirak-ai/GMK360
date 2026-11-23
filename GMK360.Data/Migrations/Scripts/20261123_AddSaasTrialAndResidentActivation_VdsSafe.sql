-- Idempotent VDS / Production SQL update for migration 20261123062544_AddSaasTrialAndResidentActivation
IF COL_LENGTH(N'dbo.tbl_gmk_KullaniciAbonelikleri', N'AdminNotes') IS NULL
    ALTER TABLE [dbo].[tbl_gmk_KullaniciAbonelikleri] ADD [AdminNotes] nvarchar(max) NULL;
IF COL_LENGTH(N'dbo.tbl_gmk_KullaniciAbonelikleri', N'DiscountPolicyId') IS NULL
    ALTER TABLE [dbo].[tbl_gmk_KullaniciAbonelikleri] ADD [DiscountPolicyId] int NULL;
IF COL_LENGTH(N'dbo.tbl_gmk_KullaniciAbonelikleri', N'LastPaymentDate') IS NULL
    ALTER TABLE [dbo].[tbl_gmk_KullaniciAbonelikleri] ADD [LastPaymentDate] datetime2 NULL;
IF COL_LENGTH(N'dbo.tbl_gmk_KullaniciAbonelikleri', N'PaymentStatus') IS NULL
    ALTER TABLE [dbo].[tbl_gmk_KullaniciAbonelikleri] ADD [PaymentStatus] nvarchar(max) NOT NULL CONSTRAINT [DF_KA_PaymentStatus] DEFAULT N'Trial';
GO

UPDATE [dbo].[tbl_gmk_KullaniciAbonelikleri]
SET [PaymentStatus] = CASE WHEN [PricePaid] > 0 THEN N'Paid' ELSE N'Trial' END
WHERE [PaymentStatus] = N'' OR [PaymentStatus] IS NULL;
GO

IF COL_LENGTH(N'dbo.DiscountPolicies', N'Description') IS NOT NULL
    ALTER TABLE [dbo].[DiscountPolicies] ALTER COLUMN [Description] nvarchar(max) NULL;
IF COL_LENGTH(N'dbo.DiscountPolicies', N'ExtraTrialDays') IS NULL
    ALTER TABLE [dbo].[DiscountPolicies] ADD [ExtraTrialDays] int NOT NULL CONSTRAINT [DF_DP_ExtraTrialDays] DEFAULT 0;
IF COL_LENGTH(N'dbo.DiscountPolicies', N'PromoCode') IS NULL
    ALTER TABLE [dbo].[DiscountPolicies] ADD [PromoCode] nvarchar(50) NULL;
IF COL_LENGTH(N'dbo.DiscountPolicies', N'TargetModule') IS NULL
    ALTER TABLE [dbo].[DiscountPolicies] ADD [TargetModule] nvarchar(max) NULL;
GO

IF COL_LENGTH(N'dbo.AspNetUsers', N'ActivationToken') IS NULL
    ALTER TABLE [dbo].[AspNetUsers] ADD [ActivationToken] nvarchar(max) NULL;
IF COL_LENGTH(N'dbo.AspNetUsers', N'ActivationTokenExpiresAt') IS NULL
    ALTER TABLE [dbo].[AspNetUsers] ADD [ActivationTokenExpiresAt] datetime2 NULL;
IF COL_LENGTH(N'dbo.AspNetUsers', N'MustChangePassword') IS NULL
    ALTER TABLE [dbo].[AspNetUsers] ADD [MustChangePassword] bit NOT NULL CONSTRAINT [DF_AU_MustChangePassword] DEFAULT 0;
IF COL_LENGTH(N'dbo.AspNetUsers', N'PhoneVerifiedAt') IS NULL
    ALTER TABLE [dbo].[AspNetUsers] ADD [PhoneVerifiedAt] datetime2 NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_tbl_gmk_KullaniciAbonelikleri_DiscountPolicyId' AND object_id = OBJECT_ID(N'dbo.tbl_gmk_KullaniciAbonelikleri'))
    CREATE INDEX [IX_tbl_gmk_KullaniciAbonelikleri_DiscountPolicyId] ON [dbo].[tbl_gmk_KullaniciAbonelikleri] ([DiscountPolicyId]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_tbl_gmk_KullaniciAbonelikleri_DiscountPolicies_DiscountPolicyId')
    ALTER TABLE [dbo].[tbl_gmk_KullaniciAbonelikleri] WITH CHECK ADD CONSTRAINT [FK_tbl_gmk_KullaniciAbonelikleri_DiscountPolicies_DiscountPolicyId]
    FOREIGN KEY([DiscountPolicyId]) REFERENCES [dbo].[DiscountPolicies] ([Id]) ON DELETE SET NULL;
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[__EFMigrationsHistory] WHERE [MigrationId] = N'20261123062544_AddSaasTrialAndResidentActivation')
BEGIN
    INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261123062544_AddSaasTrialAndResidentActivation', N'10.0.11');
END
GO
