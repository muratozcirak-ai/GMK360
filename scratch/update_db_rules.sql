USE GMK360Db;
GO
CREATE TABLE ModuleDocumentRules (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    TargetModule NVARCHAR(MAX) NOT NULL,
    Stage NVARCHAR(MAX) NOT NULL,
    DisplayOrder INT NOT NULL DEFAULT 0,
    SystemLegalDocumentTemplateId INT NOT NULL,
    IsMandatory BIT NOT NULL DEFAULT 1,
    PrerequisiteTemplateId INT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (SystemLegalDocumentTemplateId) REFERENCES SystemLegalDocumentTemplates(Id),
    FOREIGN KEY (PrerequisiteTemplateId) REFERENCES SystemLegalDocumentTemplates(Id)
);
GO
