BEGIN TRANSACTION;
ALTER TABLE [ProjectOwners] ADD [BlockName] nvarchar(max) NULL;

ALTER TABLE [ProjectOwners] ADD [UnitType] nvarchar(max) NULL;

DECLARE @var nvarchar(max);
SELECT @var = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CrmContacts]') AND [c].[name] = N'PhoneNumber');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [CrmContacts] DROP CONSTRAINT ' + @var + ';');
ALTER TABLE [CrmContacts] ALTER COLUMN [PhoneNumber] nvarchar(max) NULL;

DECLARE @var1 nvarchar(max);
SELECT @var1 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CrmContacts]') AND [c].[name] = N'Notes');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [CrmContacts] DROP CONSTRAINT ' + @var1 + ';');
ALTER TABLE [CrmContacts] ALTER COLUMN [Notes] nvarchar(max) NULL;

DECLARE @var2 nvarchar(max);
SELECT @var2 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CrmContacts]') AND [c].[name] = N'LastName');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [CrmContacts] DROP CONSTRAINT ' + @var2 + ';');
ALTER TABLE [CrmContacts] ALTER COLUMN [LastName] nvarchar(max) NULL;

DECLARE @var3 nvarchar(max);
SELECT @var3 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CrmContacts]') AND [c].[name] = N'Email');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [CrmContacts] DROP CONSTRAINT ' + @var3 + ';');
ALTER TABLE [CrmContacts] ALTER COLUMN [Email] nvarchar(max) NULL;

DECLARE @var4 nvarchar(max);
SELECT @var4 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CrmContacts]') AND [c].[name] = N'ContactType');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [CrmContacts] DROP CONSTRAINT ' + @var4 + ';');
ALTER TABLE [CrmContacts] ALTER COLUMN [ContactType] nvarchar(max) NULL;

DECLARE @var5 nvarchar(max);
SELECT @var5 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ConstructionProjects]') AND [c].[name] = N'Parsel');
IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [ConstructionProjects] DROP CONSTRAINT ' + @var5 + ';');
ALTER TABLE [ConstructionProjects] ALTER COLUMN [Parsel] nvarchar(max) NULL;

DECLARE @var6 nvarchar(max);
SELECT @var6 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ConstructionProjects]') AND [c].[name] = N'Ada');
IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [ConstructionProjects] DROP CONSTRAINT ' + @var6 + ';');
ALTER TABLE [ConstructionProjects] ALTER COLUMN [Ada] nvarchar(max) NULL;

ALTER TABLE [ConstructionProjects] ADD [EskiBodrumKatSayisi] int NULL;

ALTER TABLE [ConstructionProjects] ADD [EskiCatiKatiVarMi] bit NOT NULL DEFAULT CAST(0 AS bit);

ALTER TABLE [ConstructionProjects] ADD [EskiDukkanSayisi] int NULL;

ALTER TABLE [ConstructionProjects] ADD [IsNewDesignForExisting] bit NOT NULL DEFAULT CAST(0 AS bit);

ALTER TABLE [ConstructionProjects] ADD [ProjectType] nvarchar(max) NULL;

ALTER TABLE [Buildings] ADD [AttachedToBlock] nvarchar(max) NULL;

ALTER TABLE [Buildings] ADD [LayoutPattern] nvarchar(max) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260920122556_AddBlockLayoutTypes', N'10.0.11');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [ProjectAmenities] ADD [IsExisting] bit NOT NULL DEFAULT CAST(0 AS bit);

ALTER TABLE [Buildings] ADD [IsExistingBuilding] bit NOT NULL DEFAULT CAST(0 AS bit);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260922125000_AddIsExistingToProjectAmenity', N'10.0.11');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [SystemLegalDocumentTemplates] ADD [Stage] nvarchar(max) NULL;

ALTER TABLE [ProjectLegalDocuments] ADD [Stage] nvarchar(max) NULL;

CREATE TABLE [SystemDocumentDependency] (
    [Id] int NOT NULL IDENTITY,
    [TargetDocumentId] int NOT NULL,
    [PrerequisiteDocumentId] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_SystemDocumentDependency] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SystemDocumentDependency_SystemLegalDocumentTemplates_PrerequisiteDocumentId] FOREIGN KEY ([PrerequisiteDocumentId]) REFERENCES [SystemLegalDocumentTemplates] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_SystemDocumentDependency_SystemLegalDocumentTemplates_TargetDocumentId] FOREIGN KEY ([TargetDocumentId]) REFERENCES [SystemLegalDocumentTemplates] ([Id]) ON DELETE NO ACTION
);

CREATE INDEX [IX_SystemDocumentDependency_PrerequisiteDocumentId] ON [SystemDocumentDependency] ([PrerequisiteDocumentId]);

CREATE INDEX [IX_SystemDocumentDependency_TargetDocumentId] ON [SystemDocumentDependency] ([TargetDocumentId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260922150845_AddDocumentDependencies', N'10.0.11');

COMMIT;
GO

BEGIN TRANSACTION;
DECLARE @var7 nvarchar(max);
SELECT @var7 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ProjectLegalDocuments]') AND [c].[name] = N'AcquiredDate');
IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [ProjectLegalDocuments] DROP CONSTRAINT ' + @var7 + ';');
ALTER TABLE [ProjectLegalDocuments] DROP COLUMN [AcquiredDate];

DECLARE @var8 nvarchar(max);
SELECT @var8 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ProjectLegalDocuments]') AND [c].[name] = N'AppliedTo');
IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [ProjectLegalDocuments] DROP CONSTRAINT ' + @var8 + ';');
ALTER TABLE [ProjectLegalDocuments] DROP COLUMN [AppliedTo];

DECLARE @var9 nvarchar(max);
SELECT @var9 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ProjectLegalDocuments]') AND [c].[name] = N'InstitutionPhone');
IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [ProjectLegalDocuments] DROP CONSTRAINT ' + @var9 + ';');
ALTER TABLE [ProjectLegalDocuments] DROP COLUMN [InstitutionPhone];

EXEC sp_rename N'[ProjectLegalDocuments].[TrackingPerson]', N'IssueNotes', 'COLUMN';

EXEC sp_rename N'[ProjectLegalDocuments].[Notes]', N'AssignedUserId', 'COLUMN';

EXEC sp_rename N'[ProjectLegalDocuments].[ExpiryDate]', N'StartDate', 'COLUMN';

EXEC sp_rename N'[ProjectLegalDocuments].[DocumentCost]', N'EstimatedCost', 'COLUMN';

EXEC sp_rename N'[ProjectLegalDocuments].[ApplicationDate]', N'CompletedDate', 'COLUMN';

ALTER TABLE [ProjectLegalDocuments] ADD [ActualCost] decimal(18,2) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260922163006_UpdateProjectLegalDocuments', N'10.0.11');

COMMIT;
GO

BEGIN TRANSACTION;
INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260922163841_UpdateProjectLegalDocs', N'10.0.11');

COMMIT;
GO

