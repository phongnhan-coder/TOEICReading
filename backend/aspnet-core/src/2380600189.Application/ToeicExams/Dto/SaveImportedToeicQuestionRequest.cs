using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TOEICReading.ToeicExams.Dto;

/// <summary>Represents a reviewed TOEIC question submitted with a draft exam.</summary>
public sealed record SaveImportedToeicQuestionRequest
{
    /// <summary>Gets the TOEIC Reading part number.</summary>
    [Range(5, 7)]
    public int Part { get; init; }

    /// <summary>Gets the source question number.</summary>
    [Range(1, 300)]
    public int Number { get; init; }

    /// <summary>Gets the question text.</summary>
    [Required, MinLength(1), MaxLength(4000)]
    public required string QuestionText { get; init; }

    /// <summary>Gets the optional passage shared by reading questions.</summary>
    [MaxLength(20000)]
    public string Passage { get; init; }

    /// <summary>Gets answer choice A.</summary>
    [Required, MinLength(1), MaxLength(2000)]
    public required string OptionA { get; init; }

    /// <summary>Gets answer choice B.</summary>
    [Required, MinLength(1), MaxLength(2000)]
    public required string OptionB { get; init; }

    /// <summary>Gets answer choice C.</summary>
    [Required, MinLength(1), MaxLength(2000)]
    public required string OptionC { get; init; }

    /// <summary>Gets answer choice D.</summary>
    [Required, MinLength(1), MaxLength(2000)]
    public required string OptionD { get; init; }

    /// <summary>Gets the optional correct answer identified in the source document.</summary>
    [RegularExpression(@"^[ABCD]$")]
    public string CorrectAnswer { get; init; }

    /// <summary>Gets the images associated with this question.</summary>
    public IReadOnlyList<ToeicQuestionImageDto> Images { get; init; } = new List<ToeicQuestionImageDto>();
}
