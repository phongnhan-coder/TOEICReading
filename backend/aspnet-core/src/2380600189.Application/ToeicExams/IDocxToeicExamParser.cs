using TOEICReading.ToeicExams.Dto;
using System.IO;

namespace TOEICReading.ToeicExams;

public interface IDocxToeicExamParser
{
    ToeicExamImportPreviewDto Parse(Stream stream, string fileName);
}
