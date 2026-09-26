IF NOT EXISTS (SELECT 1 FROM DefinitionCategories WHERE SystemCode = 'B2BUstaSectors')
BEGIN
    INSERT INTO DefinitionCategories (Name, SystemCode, TargetType, IsMultiSelect, IsMediaTag, CreatedAt, IsDeleted)
    VALUES ('B2B Ustalık Alanları', 'B2BUstaSectors', 0, 1, 0, GETUTCDATE(), 0);
END
DECLARE @UstaCatId INT = (SELECT Id FROM DefinitionCategories WHERE SystemCode = 'B2BUstaSectors');
IF NOT EXISTS (SELECT 1 FROM DefinitionValues WHERE CategoryId = @UstaCatId)
BEGIN
    INSERT INTO DefinitionValues (CategoryId, Name, SystemCode, [Order], CreatedAt, IsDeleted, HasCount, RequiresTextInput, SubOptions) VALUES (@UstaCatId, 'Boyacı', 'boyaci', 1, GETUTCDATE(), 0, 0, 0, '');
    INSERT INTO DefinitionValues (CategoryId, Name, SystemCode, [Order], CreatedAt, IsDeleted, HasCount, RequiresTextInput, SubOptions) VALUES (@UstaCatId, 'Sıvacı', 'sivaci', 2, GETUTCDATE(), 0, 0, 0, '');
    INSERT INTO DefinitionValues (CategoryId, Name, SystemCode, [Order], CreatedAt, IsDeleted, HasCount, RequiresTextInput, SubOptions) VALUES (@UstaCatId, 'Su Tesisatçısı', 'tesisat', 3, GETUTCDATE(), 0, 0, 0, '');
    INSERT INTO DefinitionValues (CategoryId, Name, SystemCode, [Order], CreatedAt, IsDeleted, HasCount, RequiresTextInput, SubOptions) VALUES (@UstaCatId, 'Elektrik Ustası', 'elektrik', 4, GETUTCDATE(), 0, 0, 0, '');
END
