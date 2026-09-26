IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[SystemLegalDocumentTemplates]') AND name = 'Stage'
)
BEGIN
    ALTER TABLE [SystemLegalDocumentTemplates] ADD [Stage] nvarchar(max) NULL;
    PRINT 'Added Stage to SystemLegalDocumentTemplates'
END

IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[ProjectLegalDocuments]') AND name = 'Stage'
)
BEGIN
    ALTER TABLE [ProjectLegalDocuments] ADD [Stage] nvarchar(max) NULL;
    PRINT 'Added Stage to ProjectLegalDocuments'
END
