DECLARE @CatId INT = (SELECT Id FROM DefinitionCategories WHERE SystemCode = 'B2BSectors');
DECLARE @MockCompanyId INT = (SELECT TOP 1 Id FROM B2bCompanies);
DECLARE @KarotValueId INT = (SELECT TOP 1 Id FROM DefinitionValues WHERE CategoryId = @CatId AND Name = 'Zemin Etüdü ve Karot');

IF @MockCompanyId IS NOT NULL AND @KarotValueId IS NOT NULL
BEGIN
    INSERT INTO B2bCompanyCategories (B2bCompanyId, DefinitionValueId) VALUES (@MockCompanyId, @KarotValueId);
END
