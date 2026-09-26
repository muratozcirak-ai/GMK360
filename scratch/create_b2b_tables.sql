CREATE TABLE B2bCategories (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    IsDeleted BIT NOT NULL DEFAULT 0
);

CREATE TABLE B2bCompanies (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    AddedByAgencyId INT NOT NULL FOREIGN KEY REFERENCES Agencies(Id),
    Name NVARCHAR(200) NOT NULL,
    TaxOffice NVARCHAR(100) NULL,
    TaxNumber NVARCHAR(50) NULL,
    Website NVARCHAR(100) NULL,
    GeneralEmail NVARCHAR(100) NULL,
    Rating TINYINT NOT NULL DEFAULT 0,
    IsBlacklisted BIT NOT NULL DEFAULT 0,
    VerificationStatus TINYINT NOT NULL DEFAULT 0,
    LinkedUserId NVARCHAR(MAX) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    IsDeleted BIT NOT NULL DEFAULT 0
);

CREATE TABLE B2bCompanyCategories (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    B2bCompanyId INT NOT NULL FOREIGN KEY REFERENCES B2bCompanies(Id) ON DELETE CASCADE,
    B2bCategoryId INT NOT NULL FOREIGN KEY REFERENCES B2bCategories(Id) ON DELETE CASCADE,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    IsDeleted BIT NOT NULL DEFAULT 0
);

CREATE TABLE B2bBranches (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    B2bCompanyId INT NOT NULL FOREIGN KEY REFERENCES B2bCompanies(Id) ON DELETE CASCADE,
    BranchName NVARCHAR(100) NOT NULL,
    Address NVARCHAR(500) NULL,
    City NVARCHAR(MAX) NULL,
    District NVARCHAR(MAX) NULL,
    Latitude FLOAT NULL,
    Longitude FLOAT NULL,
    GoogleMapsUrl NVARCHAR(MAX) NULL,
    LandlinePhone NVARCHAR(20) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    IsDeleted BIT NOT NULL DEFAULT 0
);

CREATE TABLE B2bContacts (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    B2bCompanyId INT NOT NULL FOREIGN KEY REFERENCES B2bCompanies(Id) ON DELETE CASCADE,
    FullName NVARCHAR(100) NOT NULL,
    DepartmentOrRole NVARCHAR(50) NULL,
    MobilePhone NVARCHAR(20) NULL,
    LandlinePhone NVARCHAR(20) NULL,
    ExtensionNumber NVARCHAR(10) NULL,
    Email NVARCHAR(100) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    IsDeleted BIT NOT NULL DEFAULT 0
);

-- Seed Categories
INSERT INTO B2bCategories (Name) VALUES 
('Hafriyat ve Yıkım'), 
('Zemin Etüdü ve Karot'), 
('Mimarlık Ofisi'), 
('Hazır Beton'),
('İş Güvenliği (İSG)');

-- Mock Firma
INSERT INTO B2bCompanies (AddedByAgencyId, Name, VerificationStatus, Rating)
VALUES (1, 'Kuzey Zemin & Karot Laboratuvarı', 1, 5);

INSERT INTO B2bCompanyCategories (B2bCompanyId, B2bCategoryId)
VALUES (1, 2); -- Zemin Etüdü ve Karot

INSERT INTO B2bContacts (B2bCompanyId, FullName, DepartmentOrRole, MobilePhone)
VALUES (1, 'Ahmet Yılmaz', 'Laboratuvar Müdürü', '0555 555 5555');

