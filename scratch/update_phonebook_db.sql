ALTER TABLE AgencyPhonebooks ADD Address NVARCHAR(500) NULL;
ALTER TABLE AgencyPhonebooks ADD City NVARCHAR(MAX) NULL;
ALTER TABLE AgencyPhonebooks ADD District NVARCHAR(MAX) NULL;
ALTER TABLE AgencyPhonebooks ADD Latitude FLOAT NULL;
ALTER TABLE AgencyPhonebooks ADD Longitude FLOAT NULL;
ALTER TABLE AgencyPhonebooks ADD GoogleMapsUrl NVARCHAR(MAX) NULL;

ALTER TABLE AgencyPhonebooks ADD MobilePhone2 NVARCHAR(20) NULL;
ALTER TABLE AgencyPhonebooks ADD LandlinePhone NVARCHAR(20) NULL;
ALTER TABLE AgencyPhonebooks ADD ExtensionNumber NVARCHAR(10) NULL;
ALTER TABLE AgencyPhonebooks ADD WebsiteUrl NVARCHAR(100) NULL;

ALTER TABLE AgencyPhonebooks ADD AuthorizedPerson NVARCHAR(100) NULL;
ALTER TABLE AgencyPhonebooks ADD AuthorizedPersonRole NVARCHAR(50) NULL;
ALTER TABLE AgencyPhonebooks ADD TaxOffice NVARCHAR(100) NULL;
ALTER TABLE AgencyPhonebooks ADD TaxNumber NVARCHAR(50) NULL;
ALTER TABLE AgencyPhonebooks ADD Iban NVARCHAR(50) NULL;
