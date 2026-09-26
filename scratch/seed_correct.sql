DELETE FROM ProjectLegalDocuments;
DELETE FROM ModuleDocumentRulePrerequisites;
DELETE FROM ModuleDocumentRules;
DELETE FROM SystemLegalDocumentTemplates;
DBCC CHECKIDENT ('SystemLegalDocumentTemplates', RESEED, 0);
DBCC CHECKIDENT ('ModuleDocumentRules', RESEED, 0);

SET IDENTITY_INSERT SystemLegalDocumentTemplates ON;
INSERT INTO SystemLegalDocumentTemplates (Id, Name, CreatedAt, IsDeleted) VALUES
(1, 'Riskli Yapı Tespit Raporu (Karot Raporları)', GETDATE(), 0),
(2, 'Maliklerle Yapılan Noter Onaylı İnşaat Sözleşmesi', GETDATE(), 0),
(3, 'Elektrik Kesim (Körleme) Yazısı', GETDATE(), 0),
(4, 'Su Kesim (Körleme) Yazısı', GETDATE(), 0),
(5, 'Doğalgaz Kesim (Körleme) Yazısı', GETDATE(), 0),
(6, 'Yıkım ve Hafriyat Firması Taşeron Sözleşmesi', GETDATE(), 0),
(7, 'Belediyeden Alınan Resmi Yıkım Ruhsatı', GETDATE(), 0),
(8, 'Zemin Etüd Raporu, Mimari ve Statik Proje Onayları', GETDATE(), 0),
(9, 'Yeni Proje İçin Alınan Yapı Ruhsatı', GETDATE(), 0),
(10, 'Şantiye Şefi Ataması ve İSG Sözleşmeleri', GETDATE(), 0),
(11, 'Kritik Malzeme İrsaliyeleri ve Laboratuvar Test Sonuçları', GETDATE(), 0),
(12, 'Taşeron Sözleşmeleri ve İmzalı Hakediş Evrakları', GETDATE(), 0),
(13, 'Kat İrtifakı Kurulmuş Yeni Bölüm Tapuları', GETDATE(), 0),
(14, 'Müşterilerle Yapılan Satış Vaadi Sözleşmeleri', GETDATE(), 0),
(15, 'Yapı Kullanma İzin Belgesi (İskan)', GETDATE(), 0),
(16, 'Maliklere ve Alıcılara İmzalatılan Teslim Tutanakları', GETDATE(), 0);
SET IDENTITY_INSERT SystemLegalDocumentTemplates OFF;

SET IDENTITY_INSERT ModuleDocumentRules ON;
INSERT INTO ModuleDocumentRules (Id, SystemLegalDocumentTemplateId, TargetModule, Stage, DisplayOrder, CreatedAt, IsDeleted) VALUES
(1, 1, 'Construction', 'Yıkım Aşaması', 1, GETDATE(), 0),
(2, 2, 'Construction', 'Yıkım Aşaması', 2, GETDATE(), 0),
(3, 3, 'Construction', 'Yıkım Aşaması', 3, GETDATE(), 0),
(4, 4, 'Construction', 'Yıkım Aşaması', 4, GETDATE(), 0),
(5, 5, 'Construction', 'Yıkım Aşaması', 5, GETDATE(), 0),
(6, 6, 'Construction', 'Yıkım Aşaması', 6, GETDATE(), 0),
(7, 7, 'Construction', 'Yıkım Aşaması', 7, GETDATE(), 0),
(8, 8, 'Construction', 'Yapım Aşaması', 8, GETDATE(), 0),
(9, 9, 'Construction', 'Yapım Aşaması', 9, GETDATE(), 0),
(10, 10, 'Construction', 'Yapım Aşaması', 10, GETDATE(), 0),
(11, 11, 'Construction', 'Yapım Aşaması', 11, GETDATE(), 0),
(12, 12, 'Construction', 'Yapım Aşaması', 12, GETDATE(), 0),
(13, 13, 'Construction', 'Satış ve Teslim Aşaması', 13, GETDATE(), 0),
(14, 14, 'Construction', 'Satış ve Teslim Aşaması', 14, GETDATE(), 0),
(15, 15, 'Construction', 'Satış ve Teslim Aşaması', 15, GETDATE(), 0),
(16, 16, 'Construction', 'Satış ve Teslim Aşaması', 16, GETDATE(), 0);
SET IDENTITY_INSERT ModuleDocumentRules OFF;

INSERT INTO ModuleDocumentRulePrerequisites (ModuleDocumentRuleId, PrerequisiteTemplateId, CreatedAt, IsDeleted) VALUES
(2, 1, GETDATE(), 0),
(3, 2, GETDATE(), 0),
(4, 2, GETDATE(), 0),
(5, 2, GETDATE(), 0),
(7, 3, GETDATE(), 0),
(7, 4, GETDATE(), 0),
(7, 5, GETDATE(), 0),
(7, 6, GETDATE(), 0),
(8, 7, GETDATE(), 0),
(9, 8, GETDATE(), 0),
(10, 9, GETDATE(), 0),
(11, 10, GETDATE(), 0),
(12, 9, GETDATE(), 0),
(13, 9, GETDATE(), 0),
(14, 13, GETDATE(), 0),
(15, 11, GETDATE(), 0),
(15, 13, GETDATE(), 0),
(16, 15, GETDATE(), 0);
