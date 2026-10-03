CREATE TABLE SystemPhaseTemplates (
    Id int IDENTITY(1,1) PRIMARY KEY,
    PhaseCategory int NOT NULL,
    SubCategory nvarchar(100) NOT NULL,
    ItemName nvarchar(255) NOT NULL,
    IsQuoteRequired bit NOT NULL DEFAULT 0,
    CreatedAt datetime2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt datetime2 NULL,
    IsDeleted bit NOT NULL DEFAULT 0
);

INSERT INTO SystemPhaseTemplates (PhaseCategory, SubCategory, ItemName, IsQuoteRequired) VALUES 
(2, '1.1 İdari ve Altyapı Hazırlıkları', 'Şantiye Suyu Aboneliği', 0),
(2, '1.1 İdari ve Altyapı Hazırlıkları', 'Şantiye Elektriği Aboneliği', 0),
(2, '1.1 İdari ve Altyapı Hazırlıkları', 'Asbest Temizleme ve Raporu', 1),
(2, '1.2 Mobilizasyon ve Güvenlik', 'Çevre Kapatması (Sac/OSB)', 1),
(2, '1.2 Mobilizasyon ve Güvenlik', 'Şantiye Ofisi (Konteyner) Kurulumu', 1),
(2, '1.2 Mobilizasyon ve Güvenlik', 'Kamera ve Güvenlik Sistemleri', 1),
(2, '1.3 Yıkım Operasyonu', 'Söküm İşlemleri (Hurda Ayırma)', 0),
(2, '1.3 Yıkım Operasyonu', 'Yıkım ve Hafriyat', 1),
(2, '1.4 Zemin Hazırlığı', 'Eksi Kota İnme (Kazı)', 1),
(2, '1.4 Zemin Hazırlığı', 'Grobeton Dökümü', 1);