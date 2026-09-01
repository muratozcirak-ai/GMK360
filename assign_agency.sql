USE GMK360Db;

DECLARE @UserId NVARCHAR(450) = (SELECT TOP 1 Id FROM AspNetUsers WHERE Email = 'insaat@gmk360.com');
DECLARE @AgencyId INT;

-- Eğer Agency tablosunda örnek bir firma yoksa oluşturalım
IF NOT EXISTS (SELECT 1 FROM Agencies WHERE Name = 'Örnek İnşaat A.Ş.')
BEGIN
    INSERT INTO Agencies (Name, Phone, Address, CityId, DistrictId, CreatedAt, IsActive)
    VALUES ('Örnek İnşaat A.Ş.', '5550000000', 'Merkez Mah.', 1, 1, GETDATE(), 1);
END

SET @AgencyId = (SELECT TOP 1 Id FROM Agencies WHERE Name = 'Örnek İnşaat A.Ş.');

-- Eğer kullanıcı bu firmaya bağlı değilse bağlayalım
IF NOT EXISTS (SELECT 1 FROM AgencyConsultants WHERE UserId = @UserId)
BEGIN
    INSERT INTO AgencyConsultants (AgencyId, UserId, Status, RoleInAgency, JoinedAt)
    VALUES (@AgencyId, @UserId, 1, 'Owner', GETDATE());
END
