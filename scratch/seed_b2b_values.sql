DECLARE @CatId INT = (SELECT Id FROM DefinitionCategories WHERE SystemCode = 'B2BSectors');

IF NOT EXISTS (SELECT 1 FROM DefinitionValues WHERE CategoryId = @CatId AND Name = 'Hafriyat ve Yıkım')
BEGIN
    INSERT INTO DefinitionValues (CategoryId, Name, SystemCode, [Order], CreatedAt, IsDeleted, HasCount, RequiresTextInput, SubOptions) VALUES (@CatId, 'Hafriyat ve Yıkım', 'hafriyat', 1, GETUTCDATE(), 0, 0, 0, '');
    INSERT INTO DefinitionValues (CategoryId, Name, SystemCode, [Order], CreatedAt, IsDeleted, HasCount, RequiresTextInput, SubOptions) VALUES (@CatId, 'Zemin Etüdü ve Karot', 'karot', 2, GETUTCDATE(), 0, 0, 0, '');
    INSERT INTO DefinitionValues (CategoryId, Name, SystemCode, [Order], CreatedAt, IsDeleted, HasCount, RequiresTextInput, SubOptions) VALUES (@CatId, 'Mimarlık Ofisi', 'mimarlik', 3, GETUTCDATE(), 0, 0, 0, '');
    INSERT INTO DefinitionValues (CategoryId, Name, SystemCode, [Order], CreatedAt, IsDeleted, HasCount, RequiresTextInput, SubOptions) VALUES (@CatId, 'Hazır Beton', 'beton', 4, GETUTCDATE(), 0, 0, 0, '');
    INSERT INTO DefinitionValues (CategoryId, Name, SystemCode, [Order], CreatedAt, IsDeleted, HasCount, RequiresTextInput, SubOptions) VALUES (@CatId, 'İş Güvenliği (İSG)', 'isg', 5, GETUTCDATE(), 0, 0, 0, '');
    INSERT INTO DefinitionValues (CategoryId, Name, SystemCode, [Order], CreatedAt, IsDeleted, HasCount, RequiresTextInput, SubOptions) VALUES (@CatId, 'Yenilenebilir Enerji', 'enerji', 6, GETUTCDATE(), 0, 0, 0, '');
    INSERT INTO DefinitionValues (CategoryId, Name, SystemCode, [Order], CreatedAt, IsDeleted, HasCount, RequiresTextInput, SubOptions) VALUES (@CatId, 'Çatı Ustaları', 'cati', 7, GETUTCDATE(), 0, 0, 0, '');
END
