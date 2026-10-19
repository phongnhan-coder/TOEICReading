-- HISTORICAL PREVIEW ONLY: both TOEIC migrations are already recorded in the target database.
-- DO NOT execute this script against that database; CREATE TABLE statements are not idempotent.
-- Generated with EF Core tools 9.0.5 from migration 20250528073026_Upgraded_To_Abp_10_2
-- through 20261018214107_AddToeicQuestionImages.
IF CONVERT(nvarchar(128), SERVERPROPERTY('ServerName')) <> N'DESKTOP-DK9UO5F\SQLEXPRESS'
BEGIN
    THROW 51000, 'Wrong SQL Server instance; expected DESKTOP-DK9UO5F\SQLEXPRESS.', 1;
END;
GO
USE [2380600189Db];
GO
IF OBJECT_ID(N'dbo.__EFMigrationsHistory', N'U') IS NULL
BEGIN
    THROW 51001, 'Expected EF migration history table is missing.', 1;
END;
GO
IF NOT EXISTS (SELECT 1 FROM dbo.__EFMigrationsHistory WHERE MigrationId = N'20250528073026_Upgraded_To_Abp_10_2')
BEGIN
    THROW 51002, 'Expected baseline migration is not applied; refusing to continue.', 1;
END;
GO
IF EXISTS (SELECT 1 FROM dbo.__EFMigrationsHistory WHERE MigrationId IN (N'20261018204103_AddToeicExams', N'20261018214107_AddToeicQuestionImages'))
BEGIN
    THROW 51003, 'A TOEIC migration is already applied; refusing to create duplicate tables.', 1;
END;
GO
BEGIN TRANSACTION;
CREATE TABLE [ToeicExams] (
    [Id] bigint NOT NULL IDENTITY,
    [Title] nvarchar(200) NOT NULL,
    [Code] nvarchar(64) NOT NULL,
    [DurationMinutes] int NOT NULL,
    [IsPublished] bit NOT NULL,
    [SourceFileName] nvarchar(260) NULL,
    [TenantId] int NULL,
    [CreationTime] datetime2 NOT NULL,
    [CreatorUserId] bigint NULL,
    CONSTRAINT [PK_ToeicExams] PRIMARY KEY ([Id])
);

CREATE TABLE [ToeicQuestions] (
    [Id] bigint NOT NULL IDENTITY,
    [Part] int NOT NULL,
    [Number] int NOT NULL,
    [QuestionText] nvarchar(max) NOT NULL,
    [Passage] nvarchar(max) NULL,
    [OptionA] nvarchar(max) NOT NULL,
    [OptionB] nvarchar(max) NOT NULL,
    [OptionC] nvarchar(max) NOT NULL,
    [OptionD] nvarchar(max) NOT NULL,
    [CorrectAnswer] nvarchar(1) NULL,
    [ToeicExamId] bigint NOT NULL,
    [TenantId] int NULL,
    CONSTRAINT [PK_ToeicQuestions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ToeicQuestions_ToeicExams_ToeicExamId] FOREIGN KEY ([ToeicExamId]) REFERENCES [ToeicExams] ([Id]) ON DELETE CASCADE
);

CREATE UNIQUE INDEX [IX_ToeicExams_TenantId_Code] ON [ToeicExams] ([TenantId], [Code]) WHERE [TenantId] IS NOT NULL;

CREATE UNIQUE INDEX [IX_ToeicQuestions_TenantId_ToeicExamId_Part_Number] ON [ToeicQuestions] ([TenantId], [ToeicExamId], [Part], [Number]) WHERE [TenantId] IS NOT NULL;

CREATE INDEX [IX_ToeicQuestions_ToeicExamId] ON [ToeicQuestions] ([ToeicExamId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261018204103_AddToeicExams', N'9.0.5');

CREATE TABLE [ToeicQuestionImages] (
    [Id] bigint NOT NULL IDENTITY,
    [ContentType] nvarchar(100) NOT NULL,
    [Data] varbinary(max) NOT NULL,
    [SortOrder] int NOT NULL,
    [ToeicQuestionId] bigint NOT NULL,
    CONSTRAINT [PK_ToeicQuestionImages] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ToeicQuestionImages_ToeicQuestions_ToeicQuestionId] FOREIGN KEY ([ToeicQuestionId]) REFERENCES [ToeicQuestions] ([Id]) ON DELETE CASCADE
);

CREATE UNIQUE INDEX [IX_ToeicQuestionImages_ToeicQuestionId_SortOrder] ON [ToeicQuestionImages] ([ToeicQuestionId], [SortOrder]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261018214107_AddToeicQuestionImages', N'9.0.5');

COMMIT;
GO

