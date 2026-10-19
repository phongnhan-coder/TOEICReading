using System.Collections.Generic;

namespace TOEICReading.ToeicExams.Dto;

/// <summary>Returns a saved exam draft with its persisted questions.</summary>
public sealed record ToeicExamDetailsDto(
    long Id,
    string Title,
    string Code,
    int DurationMinutes,
    string SourceFileName,
    string Status,
    IReadOnlyList<SavedToeicQuestionDto> Questions);

/// <summary>Represents a question read back from a saved exam draft.</summary>
public sealed record SavedToeicQuestionDto(
    int Part,
    int Number,
    string QuestionText,
    string Passage,
    string OptionA,
    string OptionB,
    string OptionC,
    string OptionD,
    string CorrectAnswer,
    IReadOnlyList<ToeicQuestionImageDto> Images);
