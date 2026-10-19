using System.Collections.Generic;

namespace TOEICReading.ToeicExams;

/// <summary>Recognizes text in an embedded image without sending it to an external service.</summary>
public interface IDocxImageOcrService
{
    /// <summary>Returns recognized text lines in reading order.</summary>
    IReadOnlyList<string> Recognize(byte[] imageData);
}
