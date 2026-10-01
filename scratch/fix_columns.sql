IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'UserId' AND Object_ID = Object_ID(N'AgencyWorkers'))
BEGIN
    ALTER TABLE AgencyWorkers ADD UserId nvarchar(450) NULL;
    ALTER TABLE AgencyWorkers ADD CONSTRAINT FK_AgencyWorkers_AspNetUsers_UserId FOREIGN KEY (UserId) REFERENCES AspNetUsers (Id);
    CREATE INDEX IX_AgencyWorkers_UserId ON AgencyWorkers (UserId);
END
GO
IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'PendingFieldExpense' AND Object_ID = Object_ID(N'DailyTimesheets'))
BEGIN
    ALTER TABLE DailyTimesheets ADD PendingFieldExpense decimal(18,2) NOT NULL DEFAULT 0.0;
    ALTER TABLE DailyTimesheets ADD FieldExpenseDescription nvarchar(max) NULL;
    ALTER TABLE DailyTimesheets ADD PhaseName nvarchar(max) NULL;
END
GO