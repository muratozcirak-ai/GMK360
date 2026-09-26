-- Elektrik, Su, Gaz, Karot, Yıkım Firması Taşeron Sözleşmesi Ana Kuralını Sil
DELETE FROM ModuleDocumentRulePrerequisites WHERE ModuleDocumentRuleId IN (1,3,4,5,6);
DELETE FROM ModuleDocumentRules WHERE SystemLegalDocumentTemplateId IN (1,3,4,5,6);
