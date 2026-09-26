IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[ProjectAmenities]') AND name = 'IsExisting'
)
BEGIN
    ALTER TABLE [ProjectAmenities] ADD [IsExisting] bit NOT NULL DEFAULT 0;
    PRINT 'Added IsExisting column'
END
ELSE
BEGIN
    PRINT 'IsExisting column already exists'
END
