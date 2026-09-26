IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'BuildingAge' AND Object_ID = Object_ID(N'ConstructionProjects'))
BEGIN
    ALTER TABLE ConstructionProjects ADD BuildingAge INT NULL;
END
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'EskiBinaOturumAlani' AND Object_ID = Object_ID(N'ConstructionProjects'))
BEGIN
    ALTER TABLE ConstructionProjects ADD EskiBinaOturumAlani FLOAT NULL;
END
