BEGIN TRY
    EXEC sp_rename 'ProjectLegalDocuments.TrackingPerson', 'AssignedUserId', 'COLUMN';
    EXEC sp_rename 'ProjectLegalDocuments.ApplicationDate', 'StartDate', 'COLUMN';
    EXEC sp_rename 'ProjectLegalDocuments.AcquiredDate', 'CompletedDate', 'COLUMN';
    EXEC sp_rename 'ProjectLegalDocuments.DocumentCost', 'ActualCost', 'COLUMN';
    EXEC sp_rename 'ProjectLegalDocuments.Notes', 'IssueNotes', 'COLUMN';
    EXEC sp_rename 'ProjectLegalDocuments.AppliedTo', 'InstitutionContact', 'COLUMN';
    PRINT 'Renamed columns successfully.';
END TRY
BEGIN CATCH
    PRINT 'Some columns might already be renamed or missing.';
END CATCH

BEGIN TRY
    ALTER TABLE ProjectLegalDocuments DROP COLUMN InstitutionPhone;
    ALTER TABLE ProjectLegalDocuments DROP COLUMN ExpiryDate;
    PRINT 'Dropped unused columns successfully.';
END TRY
BEGIN CATCH
    PRINT 'Columns already dropped or missing.';
END CATCH

BEGIN TRY
    ALTER TABLE ProjectLegalDocuments ADD EstimatedCost decimal(18,2) NULL;
    PRINT 'Added EstimatedCost successfully.';
END TRY
BEGIN CATCH
    PRINT 'EstimatedCost already exists.';
END CATCH
