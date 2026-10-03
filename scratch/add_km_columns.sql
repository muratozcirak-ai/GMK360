IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'CurrentKm' AND Object_ID = Object_ID(N'CompanyVehicles'))
BEGIN
    ALTER TABLE CompanyVehicles ADD CurrentKm int NOT NULL DEFAULT 0;
    ALTER TABLE CompanyVehicles ADD CurrentWorkingHours decimal(18,2) NOT NULL DEFAULT 0.0;
END
GO
IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'StartKm' AND Object_ID = Object_ID(N'VehicleTasks'))
BEGIN
    ALTER TABLE VehicleTasks ADD StartKm int NULL;
    ALTER TABLE VehicleTasks ADD EndKm int NULL;
    ALTER TABLE VehicleTasks ADD WorkingHours decimal(18,2) NULL;
END
GO