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

COMMIT;
GO

