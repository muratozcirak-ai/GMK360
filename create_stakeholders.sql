CREATE TABLE ProjectStakeholders (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ProjectId INT NOT NULL,
    UserId NVARCHAR(450) NOT NULL,
    Role INT NOT NULL DEFAULT 1,
    SharePercentage DECIMAL(5,2) NULL,
    Notes NVARCHAR(500) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_ProjectStakeholders_Projects FOREIGN KEY (ProjectId) REFERENCES ConstructionProjects(Id) ON DELETE CASCADE,
    CONSTRAINT FK_ProjectStakeholders_Users FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
);
