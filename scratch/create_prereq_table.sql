-- Drop the string column
ALTER TABLE ModuleDocumentRules DROP COLUMN IF EXISTS PrerequisiteTemplateIds;

-- Create the mapping table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ModuleDocumentRulePrerequisites]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ModuleDocumentRulePrerequisites](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [ModuleDocumentRuleId] [int] NOT NULL,
        [PrerequisiteTemplateId] [int] NOT NULL,
        [CreatedAt] [datetime2](7) NOT NULL DEFAULT (getdate()),
        [UpdatedAt] [datetime2](7) NULL,
        [IsActive] [bit] NOT NULL DEFAULT ((1)),
        [IsDeleted] [bit] NOT NULL DEFAULT ((0)),
        CONSTRAINT [PK_ModuleDocumentRulePrerequisites] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[ModuleDocumentRulePrerequisites]  WITH CHECK ADD  CONSTRAINT [FK_RulePrereq_Rule] FOREIGN KEY([ModuleDocumentRuleId])
    REFERENCES [dbo].[ModuleDocumentRules] ([Id])
    ON DELETE CASCADE;

    ALTER TABLE [dbo].[ModuleDocumentRulePrerequisites]  WITH CHECK ADD  CONSTRAINT [FK_RulePrereq_Template] FOREIGN KEY([PrerequisiteTemplateId])
    REFERENCES [dbo].[SystemLegalDocumentTemplates] ([Id])
    ON DELETE CASCADE;
END
