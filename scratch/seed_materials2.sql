IF NOT EXISTS (SELECT 1 FROM DefinitionCategories WHERE SystemCode = 'B2BMaterialCategories')
BEGIN
    INSERT INTO DefinitionCategories (Name, SystemCode, IsMultiSelect, TargetType, IsMediaTag, CreatedAt, IsDeleted)
    VALUES ('B2B Malzeme Tedarik Sektörleri', 'B2BMaterialCategories', 1, 0, 0, GETUTCDATE(), 0);
    
    DECLARE @CatId INT = SCOPE_IDENTITY();
    
    INSERT INTO DefinitionValues (CategoryId, Name, SystemCode, [Order], HasCount, SubOptions, RequiresTextInput, CreatedAt, IsDeleted) VALUES 
    (@CatId, 'Hırdavat', 'CAT_HIRDAVAT', 1, 0, '', 0, GETUTCDATE(), 0),
    (@CatId, 'Orman Ürünleri', 'CAT_ORMAN', 2, 0, '', 0, GETUTCDATE(), 0),
    (@CatId, 'Yapı Malzemeleri (Çimento, Kum vb.)', 'CAT_YAPI', 3, 0, '', 0, GETUTCDATE(), 0),
    (@CatId, 'İzolasyon ve Yalıtım', 'CAT_IZOLASYON', 4, 0, '', 0, GETUTCDATE(), 0),
    (@CatId, 'Havuz Ekipmanları', 'CAT_HAVUZ', 5, 0, '', 0, GETUTCDATE(), 0),
    (@CatId, 'Elektrik Malzemeleri', 'CAT_ELEKTRIK', 6, 0, '', 0, GETUTCDATE(), 0),
    (@CatId, 'Boya ve Kimyasallar', 'CAT_BOYA', 7, 0, '', 0, GETUTCDATE(), 0);
END
