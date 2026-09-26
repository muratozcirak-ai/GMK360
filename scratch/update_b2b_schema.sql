IF EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'CompanyType' AND Object_ID = Object_ID(N'B2bCompanies'))
BEGIN
    ALTER TABLE B2bCompanies DROP COLUMN CompanyType;
END
GO
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'LegalStatus' AND Object_ID = Object_ID(N'B2bCompanies'))
BEGIN
    ALTER TABLE B2bCompanies ADD LegalStatus INT NOT NULL DEFAULT 1;
END
GO
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'IsSupplier' AND Object_ID = Object_ID(N'B2bCompanies'))
BEGIN
    ALTER TABLE B2bCompanies ADD IsSupplier BIT NOT NULL DEFAULT 0;
END
GO
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'IsSubcontractor' AND Object_ID = Object_ID(N'B2bCompanies'))
BEGIN
    ALTER TABLE B2bCompanies ADD IsSubcontractor BIT NOT NULL DEFAULT 0;
END
GO
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'IsEngineering' AND Object_ID = Object_ID(N'B2bCompanies'))
BEGIN
    ALTER TABLE B2bCompanies ADD IsEngineering BIT NOT NULL DEFAULT 0;
END
GO
UPDATE B2bCompanies SET IsSubcontractor = 1;
GO
