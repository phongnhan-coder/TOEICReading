using System.Collections.Generic;

namespace TOEICReading.ToeicExams.Dto;

/// <summary>Contains parsed Word content that has not yet been saved as an exam.</summary>
public sealed record ToeicExamImportPreviewDto(
    string SourceFileName,
    string SuggestedTitle,
    IReadOnlyList<ToeicQuestionPreviewDto> Questions);
