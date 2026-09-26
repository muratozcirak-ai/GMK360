-- First, drop default constraints on Rating if any
DECLARE @ConstraintName nvarchar(200)
SELECT @ConstraintName = Name FROM sys.default_constraints WHERE parent_object_id = object_id('B2bCompanies') AND parent_column_id = columnproperty(object_id('B2bCompanies'),'Rating','ColumnId')
IF @ConstraintName IS NOT NULL
BEGIN
    EXEC('ALTER TABLE B2bCompanies DROP CONSTRAINT ' + @ConstraintName)
END
GO
ALTER TABLE B2bCompanies ALTER COLUMN Rating FLOAT NOT NULL;
GO
