SET NOCOUNT ON;

-- Delete existing default templates for Phase 0
DELETE FROM SystemLegalDocumentTemplates WHERE TargetModule = 'Phase0' OR TargetModule IS NULL;

-- Re-insert grouped templates
INSERT INTO SystemLegalDocumentTemplates (Name, TargetModule, IsMandatory, LegalReference, IssuedBy, CreatedAt, UpdatedAt, IsDeleted, Stage) VALUES 
-- 1. Yıkım Öncesi ve Yıkım Aşaması Evrakları
('Riskli yapı tespit raporu (Karot raporları)', 'Phase0', 1, NULL, 'Özel Test Laboratuvarı / Bakanlık', GETDATE(), NULL, 0, '1. Yıkım Öncesi ve Yıkım Aşaması Evrakları'),
('Maliklerle yapılan noter onaylı inşaat ve arsa payı sözleşmeleri', 'Phase0', 1, NULL, 'Noter', GETDATE(), NULL, 0, '1. Yıkım Öncesi ve Yıkım Aşaması Evrakları'),
('Elektrik, su ve doğalgaz altyapı kesim (körleme) yazıları', 'Phase0', 1, NULL, 'Altyapı Kurumları', GETDATE(), NULL, 0, '1. Yıkım Öncesi ve Yıkım Aşaması Evrakları'),
('Belediyeden alınan resmi Yıkım Ruhsatı', 'Phase0', 1, NULL, 'İlçe Belediyesi', GETDATE(), NULL, 0, '1. Yıkım Öncesi ve Yıkım Aşaması Evrakları'),
('Yıkım ve hafriyat firmasıyla yapılan taşeron sözleşmeleri', 'Phase0', 1, NULL, 'Taşeron Firma', GETDATE(), NULL, 0, '1. Yıkım Öncesi ve Yıkım Aşaması Evrakları'),

-- 2. Yapım (İnşaat) Aşaması Evrakları
('Zemin etüd raporu, mimari ve statik proje onayları', 'Phase0', 1, NULL, 'Belediye İmar Müdürlüğü', GETDATE(), NULL, 0, '2. Yapım (İnşaat) Aşaması Evrakları'),
('Yeni proje için alınan Yapı Ruhsatı', 'Phase0', 1, NULL, 'Belediye', GETDATE(), NULL, 0, '2. Yapım (İnşaat) Aşaması Evrakları'),
('Şantiye şefi ataması ve İSG (İş Sağlığı ve Güvenliği) sözleşmeleri', 'Phase0', 1, NULL, 'İSG Firması / Şantiye Şefi', GETDATE(), NULL, 0, '2. Yapım (İnşaat) Aşaması Evrakları'),
('Taşeron sözleşmeleri ve imzalı hakediş kapak evrakları', 'Phase0', 1, NULL, 'Taşeronlar', GETDATE(), NULL, 0, '2. Yapım (İnşaat) Aşaması Evrakları'),
('Kritik malzeme irsaliyeleri (beton, demir vb.) ve laboratuvar test sonuçları', 'Phase0', 1, NULL, 'Yapı Denetim / Laboratuvar', GETDATE(), NULL, 0, '2. Yapım (İnşaat) Aşaması Evrakları'),

-- 3. Satış ve Teslim Aşaması Evrakları
('Kat irtifakı kurulmuş yeni bağımsız bölüm tapuları', 'Phase0', 1, NULL, 'Tapu Müdürlüğü', GETDATE(), NULL, 0, '3. Satış ve Teslim Aşaması Evrakları'),
('Müşterilerle yapılan gayrimenkul satış vaadi sözleşmeleri', 'Phase0', 1, NULL, 'Noter', GETDATE(), NULL, 0, '3. Satış ve Teslim Aşaması Evrakları'),
('Yapı Kullanma İzin Belgesi (İskan)', 'Phase0', 1, NULL, 'Belediye', GETDATE(), NULL, 0, '3. Satış ve Teslim Aşaması Evrakları'),
('Maliklere ve alıcılara imzalatılan anahtar/daire teslim tutanakları', 'Phase0', 1, NULL, 'Firma / Müşteri', GETDATE(), NULL, 0, '3. Satış ve Teslim Aşaması Evrakları');

PRINT 'Templates successfully updated.';
