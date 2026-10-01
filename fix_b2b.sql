UPDATE B2bCompanies SET IsEngineering = 1, IsSubcontractor = 0 WHERE Name LIKE '%Kuzey Zemin%';

INSERT INTO B2bCompanies (Name, IsSupplier, IsSubcontractor, IsEngineering, LegalStatus, Rating, InvitationStatus, CreatedAt, IsDeleted)
VALUES ('Asım Harfiyat ve Yıkım', 0, 1, 0, 2, 5.0, 0, GETDATE(), 0);

INSERT INTO B2bCompanies (Name, IsSupplier, IsSubcontractor, IsEngineering, LegalStatus, Rating, InvitationStatus, CreatedAt, IsDeleted)
VALUES ('Dengehan İnşaat', 0, 0, 1, 2, 5.0, 0, GETDATE(), 0);
