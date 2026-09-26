IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'Description' AND Object_ID = Object_ID(N'B2bCompanies'))
BEGIN
    ALTER TABLE B2bCompanies ADD Description NVARCHAR(MAX) NULL;
END
GO
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'LogoUrl' AND Object_ID = Object_ID(N'B2bCompanies'))
BEGIN
    ALTER TABLE B2bCompanies ADD LogoUrl NVARCHAR(MAX) NULL;
END
GO
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'Website' AND Object_ID = Object_ID(N'B2bCompanies'))
BEGIN
    ALTER TABLE B2bCompanies ADD Website NVARCHAR(MAX) NULL;
END
GO
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'IsVerified' AND Object_ID = Object_ID(N'B2bCompanies'))
BEGIN
    ALTER TABLE B2bCompanies ADD IsVerified BIT NOT NULL DEFAULT 0;
END
GO
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'Rating' AND Object_ID = Object_ID(N'B2bCompanies'))
BEGIN
    ALTER TABLE B2bCompanies ADD Rating FLOAT NOT NULL DEFAULT 0;
END
GO
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'AddedByAgencyId' AND Object_ID = Object_ID(N'B2bCompanies'))
BEGIN
    ALTER TABLE B2bCompanies ADD AddedByAgencyId INT NULL;
END
GO
