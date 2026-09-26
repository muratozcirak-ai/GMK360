DECLARE @TableName NVARCHAR(MAX) = 'SystemLegalDocumentTemplates';
DECLARE @cols TABLE (ColName NVARCHAR(MAX));
INSERT INTO @cols VALUES ('IsMandatory'), ('TargetModule'), ('Stage'), ('DisplayOrder');

DECLARE @ColName NVARCHAR(MAX);
DECLARE col_cursor CURSOR FOR SELECT ColName FROM @cols;
OPEN col_cursor;
FETCH NEXT FROM col_cursor INTO @ColName;
WHILE @@FETCH_STATUS = 0
BEGIN
    DECLARE @ConstraintName NVARCHAR(MAX) = NULL;
    SELECT @ConstraintName = name 
    FROM sys.default_constraints 
    WHERE parent_object_id = object_id(@TableName) 
      AND parent_column_id = columnproperty(object_id(@TableName), @ColName, 'ColumnId');
      
    IF @ConstraintName IS NOT NULL 
        EXEC('ALTER TABLE ' + @TableName + ' DROP CONSTRAINT ' + @ConstraintName);
        
    IF EXISTS(SELECT * FROM sys.columns WHERE Name = @ColName AND Object_ID = Object_ID(@TableName))
        EXEC('ALTER TABLE ' + @TableName + ' DROP COLUMN ' + @ColName);
        
    FETCH NEXT FROM col_cursor INTO @ColName;
END
CLOSE col_cursor;
DEALLOCATE col_cursor;
