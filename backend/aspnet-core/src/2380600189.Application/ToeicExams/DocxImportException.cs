using System;

namespace TOEICReading.ToeicExams;

public sealed class DocxImportException : Exception
{
    public DocxImportException(string message)
        : base(message)
    {
    }

    public DocxImportException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
