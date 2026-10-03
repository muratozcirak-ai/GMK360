IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'OdometerAtExpense' AND Object_ID = Object_ID(N'VehicleExpenses'))
BEGIN
    ALTER TABLE VehicleExpenses ADD OdometerAtExpense int NULL;
END
GO