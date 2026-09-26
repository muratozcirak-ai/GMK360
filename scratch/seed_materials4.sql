IF NOT EXISTS (SELECT 1 FROM MaterialCatalogs)
BEGIN
    INSERT INTO MaterialCatalogs (AgencyId, Name, Type, DefaultUnit, DefaultBrand, CreatedAt, IsDeleted) VALUES 
    (1, 'Hafriyat ve Taşıma', 1, 'm3', '', GETUTCDATE(), 0),
    (1, 'Yıkım İşçiliği', 1, 'm2', '', GETUTCDATE(), 0),
    (1, 'C30 Hazır Beton', 1, 'm3', '', GETUTCDATE(), 0),
    (1, '14lük Nervürlü İnşaat Demiri', 1, 'Ton', '', GETUTCDATE(), 0),
    (1, 'Ahşap Kalıp', 1, 'm2', '', GETUTCDATE(), 0);
END
