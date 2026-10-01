IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ConstructionProjectExpenses]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[ConstructionProjectExpenses] (
    [Id] int NOT NULL IDENTITY,
    [AgencyId] int NOT NULL,
    [ProjectId] int NOT NULL,
    [ExpenseType] int NOT NULL,
    [Title] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NULL,
    [Amount] decimal(18,2) NOT NULL,
    [ExpenseDate] datetime2 NOT NULL,
    [IsPaid] bit NOT NULL,
    [DocumentNo] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_ConstructionProjectExpenses] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ConstructionProjectExpenses_ConstructionProjects_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [ConstructionProjects] ([Id]) ON DELETE CASCADE
);
CREATE INDEX [IX_ConstructionProjectExpenses_ProjectId] ON [ConstructionProjectExpenses] ([ProjectId]);
END
GO