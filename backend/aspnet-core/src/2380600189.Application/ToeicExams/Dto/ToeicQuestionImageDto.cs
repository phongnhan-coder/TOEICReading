namespace TOEICReading.ToeicExams.Dto;

/// <summary>Contains an image extracted from or stored with a TOEIC question.</summary>
public sealed record ToeicQuestionImageDto(string ContentType, byte[] Data);
