using System.Collections.Generic;

namespace TOEICReading.ToeicExams.Dto;

/// <summary>Represents one parsed question in an imported TOEIC exam preview.</summary>
public sealed record ToeicQuestionPreviewDto(
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
