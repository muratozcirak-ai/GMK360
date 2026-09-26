DELETE FROM ProjectLegalDocuments;
DELETE FROM ModuleDocumentRules;
DELETE FROM SystemLegalDocumentTemplates;

DBCC CHECKIDENT ('SystemLegalDocumentTemplates', RESEED, 0);

SET IDENTITY_INSERT SystemLegalDocumentTemplates ON;
INSERT INTO SystemLegalDocumentTemplates (Id, Name, Stage, DocumentType, IsRequired, CreatedAt, IsDeleted) VALUES
(1, 'Riskli Yapı Tespit Raporu (Karot Raporları)', 'Yıkım Aşaması', 1, 1, GETDATE(), 0),
(2, 'Maliklerle Yapılan Noter Onaylı İnşaat Sözleşmesi', 'Yıkım Aşaması', 1, 1, GETDATE(), 0),
(3, 'Elektrik Kesim (Körleme) Yazısı', 'Yıkım Aşaması', 1, 1, GETDATE(), 0),
(4, 'Su Kesim (Körleme) Yazısı', 'Yıkım Aşaması', 1, 1, GETDATE(), 0),
(5, 'Doğalgaz Kesim (Körleme) Yazısı', 'Yıkım Aşaması', 1, 1, GETDATE(), 0),
(6, 'Yıkım ve Hafriyat Firması Taşeron Sözleşmesi', 'Yıkım Aşaması', 1, 1, GETDATE(), 0),
(7, 'Belediyeden Alınan Resmi Yıkım Ruhsatı', 'Yıkım Aşaması', 1, 1, GETDATE(), 0),
(8, 'Zemin Etüd Raporu, Mimari ve Statik Proje Onayları', 'Yapım Aşaması', 1, 1, GETDATE(), 0),
(9, 'Yeni Proje İçin Alınan Yapı Ruhsatı', 'Yapım Aşaması', 1, 1, GETDATE(), 0),
(10, 'Şantiye Şefi Ataması ve İSG Sözleşmeleri', 'Yapım Aşaması', 1, 1, GETDATE(), 0),
(11, 'Kritik Malzeme İrsaliyeleri ve Laboratuvar Test Sonuçları', 'Yapım Aşaması', 1, 1, GETDATE(), 0),
(12, 'Taşeron Sözleşmeleri ve İmzalı Hakediş Evrakları', 'Yapım Aşaması', 1, 1, GETDATE(), 0),
(13, 'Kat İrtifakı Kurulmuş Yeni Bölüm Tapuları', 'Satış ve Teslim Aşaması', 1, 1, GETDATE(), 0),
(14, 'Müşterilerle Yapılan Satış Vaadi Sözleşmeleri', 'Satış ve Teslim Aşaması', 1, 1, GETDATE(), 0),
(15, 'Yapı Kullanma İzin Belgesi (İskan)', 'Satış ve Teslim Aşaması', 1, 1, GETDATE(), 0),
(16, 'Maliklere ve Alıcılara İmzalatılan Teslim Tutanakları', 'Satış ve Teslim Aşaması', 1, 1, GETDATE(), 0);
SET IDENTITY_INSERT SystemLegalDocumentTemplates OFF;

INSERT INTO ModuleDocumentRules (MainDocumentTemplateId, PrerequisiteTemplateId, ModuleType, ConditionType, CreatedAt, IsDeleted) VALUES
(2, 1, 0, 0, GETDATE(), 0),  
(3, 2, 0, 0, GETDATE(), 0),  
(4, 2, 0, 0, GETDATE(), 0),  
(5, 2, 0, 0, GETDATE(), 0),  
(7, 3, 0, 0, GETDATE(), 0),  
(7, 4, 0, 0, GETDATE(), 0),  
(7, 5, 0, 0, GETDATE(), 0),  
(7, 6, 0, 0, GETDATE(), 0),  
(8, 7, 0, 0, GETDATE(), 0),  
(9, 8, 0, 0, GETDATE(), 0),  
(10, 9, 0, 0, GETDATE(), 0), 
(11, 10, 0, 0, GETDATE(), 0),
(12, 9, 0, 0, GETDATE(), 0), 
(13, 9, 0, 0, GETDATE(), 0), 
(14, 13, 0, 0, GETDATE(), 0),
(15, 11, 0, 0, GETDATE(), 0),
(15, 13, 0, 0, GETDATE(), 0),
(16, 15, 0, 0, GETDATE(), 0);
