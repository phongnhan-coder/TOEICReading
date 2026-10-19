using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TOEICReading.ToeicExams.Dto;

/// <summary>Contains an edited Word import preview for explicit draft creation.</summary>
public sealed record SaveImportedToeicExamRequest
{
    /// <summary>Gets the exam title.</summary>
    [Required, StringLength(ToeicExam.MaxTitleLength)]
    public required string Title { get; init; }

    /// <summary>Gets the unique exam code.</summary>
    [Required, RegularExpression(@"^[A-Za-z0-9_-]+$"), StringLength(ToeicExam.MaxCodeLength)]
    public required string Code { get; init; }

    /// <summary>Gets the exam duration in minutes.</summary>
    [Range(1, 300)]
    public int DurationMinutes { get; init; } = 75;

    /// <summary>Gets the original uploaded file name for audit display.</summary>
    [Required, StringLength(260)]
    public required string SourceFileName { get; init; }

    /// <summary>Gets the questions reviewed in the preview.</summary>
    [Required, MinLength(1), MaxLength(300)]
    public required IReadOnlyList<SaveImportedToeicQuestionRequest> Questions { get; init; }
}
