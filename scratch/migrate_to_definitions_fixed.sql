IF OBJECT_ID('B2bCompanyCategories', 'U') IS NOT NULL DROP TABLE B2bCompanyCategories;
IF OBJECT_ID('B2bCategories', 'U') IS NOT NULL DROP TABLE B2bCategories;

CREATE TABLE B2bCompanyCategories (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    B2bCompanyId INT NOT NULL FOREIGN KEY REFERENCES B2bCompanies(Id) ON DELETE CASCADE,
    DefinitionValueId INT NOT NULL FOREIGN KEY REFERENCES DefinitionValues(Id) ON DELETE CASCADE,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    IsDeleted BIT NOT NULL DEFAULT 0
);

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

DECLARE @MockCompanyId INT = (SELECT TOP 1 Id FROM B2bCompanies);
DECLARE @KarotValueId INT = (SELECT TOP 1 Id FROM DefinitionValues WHERE CategoryId = @CatId AND Name = 'Zemin Etüdü ve Karot');

IF @MockCompanyId IS NOT NULL AND @KarotValueId IS NOT NULL
BEGIN
    INSERT INTO B2bCompanyCategories (B2bCompanyId, DefinitionValueId) VALUES (@MockCompanyId, @KarotValueId);
END
