-- Idempotent VDS / Production SQL update for migration 20261123081258_AddPlatformFeedbacks
IF OBJECT_ID(N'dbo.PlatformFeedbacks', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[PlatformFeedbacks] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [ModuleArea] nvarchar(60) NOT NULL,
        [FeedbackType] nvarchar(40) NOT NULL,
        [Subject] nvarchar(200) NOT NULL,
        [Message] nvarchar(4000) NOT NULL,
        [PageUrl] nvarchar(500) NULL,
        [DeviceInfo] nvarchar(300) NULL,
        [UserId] nvarchar(450) NULL,
        [UserFullName] nvarchar(150) NULL,
        [UserEmail] nvarchar(200) NULL,
        [UserRoleLabel] nvarchar(100) NULL,
        [ReferenceEntityId] int NULL,
        [Status] nvarchar(30) NOT NULL,
        [AdminNote] nvarchar(2000) NULL,
        [ResolvedAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_PlatformFeedbacks] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PlatformFeedbacks_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id])
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PlatformFeedbacks_UserId' AND object_id = OBJECT_ID(N'dbo.PlatformFeedbacks'))
    CREATE INDEX [IX_PlatformFeedbacks_UserId] ON [dbo].[PlatformFeedbacks] ([UserId]);
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[__EFMigrationsHistory] WHERE [MigrationId] = N'20261123081258_AddPlatformFeedbacks')
BEGIN
    INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261123081258_AddPlatformFeedbacks', N'10.0.11');
END
GO
