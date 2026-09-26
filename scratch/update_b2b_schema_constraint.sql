DECLARE @ConstraintName nvarchar(200)
SELECT @ConstraintName = Name FROM sys.default_constraints WHERE parent_object_id = object_id('B2bCompanies') AND parent_column_id = columnproperty(object_id('B2bCompanies'),'CompanyType','ColumnId')
IF @ConstraintName IS NOT NULL
BEGIN
    EXEC('ALTER TABLE B2bCompanies DROP CONSTRAINT ' + @ConstraintName)
END
GO
IF EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'CompanyType' AND Object_ID = Object_ID(N'B2bCompanies'))
BEGIN
    ALTER TABLE B2bCompanies DROP COLUMN CompanyType;
END
GO
