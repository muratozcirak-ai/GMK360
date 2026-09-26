IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'IsEnterprise' AND Object_ID = Object_ID(N'B2bCompanies'))
BEGIN
    ALTER TABLE B2bCompanies ADD IsEnterprise BIT NOT NULL DEFAULT 0;
END
GO
