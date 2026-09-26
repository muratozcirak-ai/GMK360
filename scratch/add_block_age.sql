IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'BuildingAge' AND Object_ID = Object_ID(N'Buildings'))
BEGIN
    ALTER TABLE Buildings ADD BuildingAge INT NULL;
END
