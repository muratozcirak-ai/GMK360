BEGIN TRANSACTION;

CREATE TABLE [SystemDocumentDependencies] (
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

CREATE INDEX [IX_SystemDocumentDependency_PrerequisiteDocumentId] ON [SystemDocumentDependencies] ([PrerequisiteDocumentId]);

CREATE INDEX [IX_SystemDocumentDependency_TargetDocumentId] ON [SystemDocumentDependencies] ([TargetDocumentId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260922150845_AddDocumentDependencies', N'10.0.11');

COMMIT;
