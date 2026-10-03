IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'PhotoPath' AND Object_ID = Object_ID(N'VehicleExpenses'))
BEGIN
    ALTER TABLE VehicleExpenses ADD PhotoPath nvarchar(max) NULL;
END
GO
IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'StartTime' AND Object_ID = Object_ID(N'VehicleTasks'))
BEGIN
    ALTER TABLE VehicleTasks ADD StartTime nvarchar(max) NULL;
    ALTER TABLE VehicleTasks ADD EndTime nvarchar(max) NULL;
END
GO