IF NOT EXISTS (SELECT 1 FROM MaterialCatalogs)
BEGIN
    INSERT INTO MaterialCatalogs (Name, Category, DefaultUnit, Description, EstimatedUnitPrice, CreatedAt, IsDeleted) VALUES 
    ('Hafriyat ve Taşıma', 'Hafriyat', 'm3', 'Temel kazı ve nakliyesi', 0, GETUTCDATE(), 0),
    ('Yıkım İşçiliği', 'Yıkım', 'm2', 'Bina yıkım bedeli (Hurdalar karşılığı genelde eksi bakiye)', 0, GETUTCDATE(), 0),
    ('C30 Hazır Beton', 'Beton', 'm3', 'Standart C30 Pompalı', 3000, GETUTCDATE(), 0),
    ('14lük Nervürlü İnşaat Demiri', 'Demir', 'Ton', 'BGS veya muadili', 25000, GETUTCDATE(), 0),
    ('Ahşap Kalıp', 'Kalıp', 'm2', 'Plywood', 0, GETUTCDATE(), 0);
END
