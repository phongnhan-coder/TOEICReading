namespace TOEICReading.ToeicExams.Dto;

/// <summary>Identifies a TOEIC exam draft that was saved to the database.</summary>
public sealed record SavedToeicExamDto(
    long Id,
    string Title,
    string Code,
    int DurationMinutes,
    int QuestionCount,
    string Status);
