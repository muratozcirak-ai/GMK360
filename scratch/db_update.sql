CREATE TABLE InstitutionRecords (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(MAX),
    Category INT NOT NULL,
    City NVARCHAR(MAX),
    District NVARCHAR(MAX),
    Address NVARCHAR(MAX),
    GooglePlaceId NVARCHAR(MAX),
    GoogleMapsUrl NVARCHAR(MAX)
);

CREATE TABLE InstitutionContacts (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    InstitutionRecordId INT NOT NULL,
    FullName NVARCHAR(MAX),
    Role NVARCHAR(MAX),
    PhoneNumber NVARCHAR(MAX),
    Notes NVARCHAR(MAX),
    CONSTRAINT FK_InstitutionContacts_InstitutionRecords FOREIGN KEY (InstitutionRecordId) REFERENCES InstitutionRecords(Id) ON DELETE CASCADE
);

ALTER TABLE SystemLegalDocumentTemplates ADD Category INT NULL;

ALTER TABLE ProjectLegalDocuments ADD TargetInstitutionId INT NULL;
ALTER TABLE ProjectLegalDocuments ADD TargetContactId INT NULL;

ALTER TABLE ProjectLegalDocuments ADD CONSTRAINT FK_ProjectLegalDocuments_TargetInstitution FOREIGN KEY (TargetInstitutionId) REFERENCES InstitutionRecords(Id);
ALTER TABLE ProjectLegalDocuments ADD CONSTRAINT FK_ProjectLegalDocuments_TargetContact FOREIGN KEY (TargetContactId) REFERENCES InstitutionContacts(Id);
