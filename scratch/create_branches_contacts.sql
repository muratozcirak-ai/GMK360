CREATE TABLE AgencyPhonebookBranches (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    AgencyPhonebookId INT NOT NULL FOREIGN KEY REFERENCES AgencyPhonebooks(Id) ON DELETE CASCADE,
    BranchName NVARCHAR(100) NOT NULL,
    Address NVARCHAR(500) NULL,
    City NVARCHAR(MAX) NULL,
    District NVARCHAR(MAX) NULL,
    Latitude FLOAT NULL,
    Longitude FLOAT NULL,
    GoogleMapsUrl NVARCHAR(MAX) NULL,
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NULL,
    IsDeleted BIT NOT NULL DEFAULT 0
);

CREATE TABLE AgencyPhonebookContacts (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    AgencyPhonebookId INT NOT NULL FOREIGN KEY REFERENCES AgencyPhonebooks(Id) ON DELETE CASCADE,
    FullName NVARCHAR(100) NOT NULL,
    DepartmentOrRole NVARCHAR(50) NULL,
    PhoneNumber NVARCHAR(20) NULL,
    ExtensionNumber NVARCHAR(10) NULL,
    Email NVARCHAR(100) NULL,
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NULL,
    IsDeleted BIT NOT NULL DEFAULT 0
);
