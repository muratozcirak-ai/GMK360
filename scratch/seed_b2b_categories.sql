IF NOT EXISTS (SELECT 1 FROM DefinitionCategories WHERE SystemCode = 'B2BSectors')
BEGIN
    INSERT INTO DefinitionCategories (Name, SystemCode, TargetType, IsMultiSelect, IsMediaTag, CreatedAt, IsDeleted)
    VALUES ('B2B Firma Sektörleri', 'B2BSectors', 0, 1, 0, GETUTCDATE(), 0);
END

DECLARE @CatId INT = (SELECT Id FROM DefinitionCategories WHERE SystemCode = 'B2BSectors');

IF NOT EXISTS (SELECT 1 FROM DefinitionValues WHERE CategoryId = @CatId AND Name = 'Hafriyat ve Yıkım')
BEGIN
    INSERT INTO DefinitionValues (CategoryId, Name, [Order], CreatedAt, IsDeleted, HasCount, RequiresTextInput, SubOptions) VALUES (@CatId, 'Hafriyat ve Yıkım', 1, GETUTCDATE(), 0, 0, 0, '');
    INSERT INTO DefinitionValues (CategoryId, Name, [Order], CreatedAt, IsDeleted, HasCount, RequiresTextInput, SubOptions) VALUES (@CatId, 'Zemin Etüdü ve Karot', 2, GETUTCDATE(), 0, 0, 0, '');
    INSERT INTO DefinitionValues (CategoryId, Name, [Order], CreatedAt, IsDeleted, HasCount, RequiresTextInput, SubOptions) VALUES (@CatId, 'Mimarlık Ofisi', 3, GETUTCDATE(), 0, 0, 0, '');
    INSERT INTO DefinitionValues (CategoryId, Name, [Order], CreatedAt, IsDeleted, HasCount, RequiresTextInput, SubOptions) VALUES (@CatId, 'Hazır Beton', 4, GETUTCDATE(), 0, 0, 0, '');
    INSERT INTO DefinitionValues (CategoryId, Name, [Order], CreatedAt, IsDeleted, HasCount, RequiresTextInput, SubOptions) VALUES (@CatId, 'İş Güvenliği (İSG)', 5, GETUTCDATE(), 0, 0, 0, '');
    INSERT INTO DefinitionValues (CategoryId, Name, [Order], CreatedAt, IsDeleted, HasCount, RequiresTextInput, SubOptions) VALUES (@CatId, 'Yenilenebilir Enerji', 6, GETUTCDATE(), 0, 0, 0, '');
    INSERT INTO DefinitionValues (CategoryId, Name, [Order], CreatedAt, IsDeleted, HasCount, RequiresTextInput, SubOptions) VALUES (@CatId, 'Çatı Ustaları', 7, GETUTCDATE(), 0, 0, 0, '');
END
