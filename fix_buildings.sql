ALTER TABLE Buildings ADD CreatedByUserId nvarchar(450) NULL;
ALTER TABLE Buildings ADD IsSystemVerified bit NOT NULL DEFAULT 0;
