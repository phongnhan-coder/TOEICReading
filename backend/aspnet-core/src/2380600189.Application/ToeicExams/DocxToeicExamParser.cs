using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using TOEICReading.ToeicExams.Dto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using Blip = DocumentFormat.OpenXml.Drawing.Blip;

namespace TOEICReading.ToeicExams;

public sealed class DocxToeicExamParser : IDocxToeicExamParser
{
    private static readonly Regex PartPattern = new(@"^\s*part\s*[-:]?\s*(\d+)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    private static readonly Regex QuestionPattern = new(@"^\s*(?:question\s*)?(\d{1,3})\s*[.)]\s*(.+)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    private static readonly Regex QuestionNumberOnlyPattern = new(@"^\s*(?:question\s*)?(\d{1,3})\s*[.)]\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    private static readonly Regex ChoicePattern = new(@"^\s*\(?([A-D])[\).]\s*(.+)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    private static readonly Regex OcrChoicePattern = new(@"^\s*[\(\[]?([A-D])\s*[\).,:;|]\s*(.+)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    private static readonly Regex EmbeddedChoiceAPattern = new(@"^(.*?)\s+\(?A[\).,]\s+(.+)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    private static readonly Regex AnswerPattern = new(@"^\s*(?:answer|ans|correct\s+answer|correct|đáp\s*án)\s*[:\-]?\s*([A-D])\s*\.?\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    private static readonly Regex PassageReferencePattern = new(@"^\s*questions?\s+\d+\s*[-–]\s*\d+\s+refer\s+to\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    private const int MaximumOcrImages = 100;
    private readonly IDocxImageOcrService _imageOcrService;

    /// <summary>Creates a DOCX parser with the local OCR service used for image-only documents.</summary>
    public DocxToeicExamParser(IDocxImageOcrService imageOcrService)
    {
        _imageOcrService = imageOcrService ?? throw new ArgumentNullException(nameof(imageOcrService));
    }

    /// <inheritdoc />
    public ToeicExamImportPreviewDto Parse(Stream stream, string fileName)
    {
        ArgumentNullException.ThrowIfNull(stream);

        WordprocessingDocument document;
        try
        {
            document = WordprocessingDocument.Open(stream, false);
        }
        catch (OpenXmlPackageException exception)
        {
            throw new DocxImportException("Không thể đọc cấu trúc tài liệu Word (.docx).", exception);
        }
        catch (XmlException exception)
        {
            throw new DocxImportException("Tài liệu Word chứa XML không hợp lệ.", exception);
        }
        catch (InvalidDataException exception)
        {
            throw new DocxImportException("Tệp không phải tài liệu Word .docx hợp lệ.", exception);
        }

        using (document)
        {
            var body = document.MainDocumentPart?.Document?.Body;
            if (body is null)
            {
                throw new DocxImportException("Tài liệu Word không có nội dung chính.");
            }

            var imageCache = new Dictionary<string, ToeicQuestionImageDto>(StringComparer.Ordinal);
            var paragraphs = new List<ParsedParagraph>();
            var sourceImages = new List<ToeicQuestionImageDto>();
            long totalImageBytes = 0;

            foreach (var paragraph in body.Descendants<Paragraph>())
            {
                var text = NormalizeParagraph(paragraph);
                var paragraphImages = ExtractImages(
                    paragraph,
                    document.MainDocumentPart,
                    imageCache,
                    ref totalImageBytes);
                paragraphs.Add(new ParsedParagraph(text, paragraphImages));
                foreach (var image in paragraphImages)
                {
                    if (!sourceImages.Contains(image))
                    {
                        sourceImages.Add(image);
                    }
                }
            }

            var questions = ParseParagraphs(paragraphs);
            if (questions.Count == 0 && sourceImages.Count > 0)
            {
                if (sourceImages.Count > MaximumOcrImages)
                {
                    throw new DocxImportException("Tài liệu có quá nhiều ảnh để xử lý OCR an toàn (tối đa 100 ảnh).");
                }

                var ocrParagraphs = new List<ParsedParagraph>();
                foreach (var paragraph in paragraphs)
                {
                    if (paragraph.Text.Length > 0)
                    {
                        ocrParagraphs.Add(paragraph);
                        continue;
                    }

                    foreach (var image in paragraph.Images)
                    {
                        var lines = _imageOcrService.Recognize(image.Data);
                        var imageAttached = false;
                        var sawPart = false;
                        foreach (var line in lines)
                        {
                            var normalizedLine = Regex.Replace(line, @"\s+", " ").Trim();
                            if (normalizedLine.Length == 0)
                            {
                                continue;
                            }

                            if (PartPattern.IsMatch(normalizedLine))
                            {
                                sawPart = true;
                            }

                            var attachImage = !imageAttached &&
                                (PassageReferencePattern.IsMatch(normalizedLine) ||
                                 QuestionPattern.IsMatch(normalizedLine) ||
                                 sawPart && !PartPattern.IsMatch(normalizedLine));
                            ocrParagraphs.Add(new ParsedParagraph(
                                normalizedLine,
                                attachImage ? new[] { image } : Array.Empty<ToeicQuestionImageDto>()));
                            imageAttached |= attachImage;
                        }
                    }
                }

                questions = ParseParagraphs(ocrParagraphs, isOcr: true);
                if (questions.Count == 0)
                {
                    throw new DocxImportException(
                        "OCR không nhận diện được câu hỏi TOEIC có đủ lựa chọn A, B, C và D từ các ảnh. " +
                        "Hãy kiểm tra độ rõ của ảnh và bố cục câu hỏi.");
                }
            }
            else if (questions.Count == 0)
            {
                throw new DocxImportException("Không tìm thấy câu hỏi TOEIC có đủ lựa chọn A, B, C và D trong tài liệu.");
            }

            var title = Path.GetFileNameWithoutExtension(fileName);
            return new ToeicExamImportPreviewDto(fileName, title, questions);
        }
    }

    private static List<ToeicQuestionPreviewDto> ParseParagraphs(
        IReadOnlyList<ParsedParagraph> paragraphs,
        bool isOcr = false)
    {
        var questions = new List<ToeicQuestionPreviewDto>();
        var passageBuffer = new List<string>();
        var passageImages = new List<ToeicQuestionImageDto>();
        var currentPassage = string.Empty;
        var currentPart = 0;
        var currentQuestion = new ParsedQuestion();
        var hasCurrentQuestion = false;
        var totalCharacters = 0;
        var pendingQuestionNumber = (int?)null;
        var lastQuestionNumber = 0;

        foreach (var paragraph in paragraphs)
        {
            var text = paragraph.Text;
            var paragraphImages = paragraph.Images;
            if (text.Length == 0)
            {
                if (currentPart is 6 or 7)
                {
                    passageImages.AddRange(paragraphImages);
                }
                else if (hasCurrentQuestion)
                {
                    currentQuestion.Images.AddRange(paragraphImages);
                }
                continue;
            }

            totalCharacters += text.Length;
            if (totalCharacters > 2_000_000)
            {
                throw new DocxImportException("Nội dung văn bản vượt quá giới hạn xử lý.");
            }

            var partMatch = PartPattern.Match(text);
            if (partMatch.Success)
            {
                FinishQuestion(currentQuestion, hasCurrentQuestion, questions);
                hasCurrentQuestion = false;
                if (!int.TryParse(partMatch.Groups[1].Value, out currentPart) ||
                    currentPart < 5 || currentPart > 7)
                {
                    throw new DocxImportException("Chỉ hỗ trợ các phần TOEIC Reading Part 5, Part 6 và Part 7.");
                }

                passageBuffer.Clear();
                passageImages.Clear();
                currentPassage = string.Empty;
                pendingQuestionNumber = null;
                lastQuestionNumber = 0;
                continue;
            }

            if (currentPart == 0)
            {
                continue;
            }

            if (PassageReferencePattern.IsMatch(text))
            {
                FinishQuestion(currentQuestion, hasCurrentQuestion, questions);
                hasCurrentQuestion = false;
                passageBuffer.Clear();
                passageImages.Clear();
                currentPassage = string.Empty;
                passageImages.AddRange(paragraphImages);
                continue;
            }

            if (pendingQuestionNumber.HasValue)
            {
                if (GetChoiceMatch(text, isOcr).Success || AnswerPattern.IsMatch(text))
                {
                    throw new DocxImportException(
                        $"Không nhận diện được nội dung câu {pendingQuestionNumber.Value} trong ảnh quét.");
                }

                currentQuestion = new ParsedQuestion
                {
                    Part = currentPart,
                    Number = NormalizeQuestionNumber(pendingQuestionNumber.Value, currentPart, lastQuestionNumber, isOcr),
                    Text = text.Trim(),
                    Passage = currentPart == 5 ? null : currentPassage,
                    Images = currentPart == 5
                        ? new List<ToeicQuestionImageDto>()
                        : passageImages.ToList()
                };
                AddEmbeddedChoiceA(text, currentQuestion, isOcr);
                lastQuestionNumber = currentQuestion.Number;
                passageImages.Clear();
                currentQuestion.Images.AddRange(paragraphImages);
                pendingQuestionNumber = null;
                hasCurrentQuestion = true;
                continue;
            }

            var questionNumberOnlyMatch = QuestionNumberOnlyPattern.Match(text);
            if (questionNumberOnlyMatch.Success)
            {
                FinishQuestion(currentQuestion, hasCurrentQuestion, questions);
                hasCurrentQuestion = false;
                pendingQuestionNumber = int.Parse(questionNumberOnlyMatch.Groups[1].Value);
                continue;
            }

            var questionMatch = QuestionPattern.Match(text);
            if (questionMatch.Success)
            {
                FinishQuestion(currentQuestion, hasCurrentQuestion, questions);
                if (passageBuffer.Count > 0)
                {
                    currentPassage = string.Join(Environment.NewLine, passageBuffer);
                    passageBuffer.Clear();
                }

                currentQuestion = new ParsedQuestion
                {
                    Part = currentPart,
                    Number = NormalizeQuestionNumber(
                        int.Parse(questionMatch.Groups[1].Value),
                        currentPart,
                        lastQuestionNumber,
                        isOcr),
                    Text = questionMatch.Groups[2].Value.Trim(),
                    Passage = currentPart == 5 ? null : currentPassage,
                    Images = currentPart == 5
                        ? new List<ToeicQuestionImageDto>()
                        : passageImages.ToList()
                };
                AddEmbeddedChoiceA(questionMatch.Groups[2].Value, currentQuestion, isOcr);
                lastQuestionNumber = currentQuestion.Number;
                passageImages.Clear();
                currentQuestion.Images.AddRange(paragraphImages);
                hasCurrentQuestion = true;
                continue;
            }

            var choiceMatch = GetChoiceMatch(text, isOcr);
            if (choiceMatch.Success && hasCurrentQuestion)
            {
                var key = choiceMatch.Groups[1].Value.ToUpperInvariant();
                if (!currentQuestion.Choices.TryAdd(key, choiceMatch.Groups[2].Value.Trim()))
                {
                    throw new DocxImportException($"Câu {currentQuestion.Number} có lựa chọn {key} bị lặp.");
                }

                currentQuestion.Images.AddRange(paragraphImages);
                continue;
            }

            var answerMatch = AnswerPattern.Match(text);
            if (answerMatch.Success && hasCurrentQuestion)
            {
                currentQuestion.CorrectAnswer = answerMatch.Groups[1].Value.ToUpperInvariant();
                currentQuestion.Images.AddRange(paragraphImages);
                continue;
            }

            if (hasCurrentQuestion && currentQuestion.Choices.Count == 0)
            {
                currentQuestion.Text = $"{currentQuestion.Text} {text}".Trim();
                currentQuestion.Images.AddRange(paragraphImages);
            }
            else if (currentPart is 6 or 7)
            {
                if (hasCurrentQuestion)
                {
                    FinishQuestion(currentQuestion, true, questions);
                    hasCurrentQuestion = false;
                }

                passageBuffer.Add(text);
                passageImages.AddRange(paragraphImages);
                currentPassage = string.Empty;
            }
            else if (hasCurrentQuestion)
            {
                currentQuestion.Images.AddRange(paragraphImages);
            }
        }

        if (pendingQuestionNumber.HasValue)
        {
            throw new DocxImportException(
                $"Không nhận diện được nội dung câu {pendingQuestionNumber.Value} trong ảnh quét.");
        }

        FinishQuestion(currentQuestion, hasCurrentQuestion, questions);
        return questions;
    }

    private static Match GetChoiceMatch(string text, bool isOcr)
    {
        var match = ChoicePattern.Match(text);
        return match.Success || !isOcr ? match : OcrChoicePattern.Match(text);
    }

    private static void AddEmbeddedChoiceA(string text, ParsedQuestion question, bool isOcr)
    {
        if (!isOcr)
        {
            return;
        }

        var match = EmbeddedChoiceAPattern.Match(text);
        if (match.Success)
        {
            question.Choices["A"] = match.Groups[2].Value.Trim();
            question.Text = match.Groups[1].Value.Trim();
        }
    }

    private static int NormalizeQuestionNumber(int number, int part, int lastQuestionNumber, bool isOcr)
    {
        if (!isOcr || number > 54 || lastQuestionNumber == 0)
        {
            return number;
        }

        var offset = part switch
        {
            5 when lastQuestionNumber >= 101 && lastQuestionNumber <= 130 => 100,
            6 when lastQuestionNumber >= 131 && lastQuestionNumber <= 146 => 130,
            7 when lastQuestionNumber >= 147 && lastQuestionNumber <= 200 => 146,
            _ => 0
        };
        return offset > 0 && number <= (part == 5 ? 30 : part == 6 ? 16 : 54)
            ? number + offset
            : number;
    }

    private static IReadOnlyList<ToeicQuestionImageDto> ExtractImages(
        Paragraph paragraph,
        MainDocumentPart mainPart,
        IDictionary<string, ToeicQuestionImageDto> imageCache,
        ref long totalImageBytes)
    {
        var images = new List<ToeicQuestionImageDto>();
        foreach (var blip in paragraph.Descendants<Blip>())
        {
            var relationshipId = blip.Embed?.Value;
            if (string.IsNullOrWhiteSpace(relationshipId) || images.Any(image =>
                    imageCache.TryGetValue(relationshipId, out var cached) && ReferenceEquals(cached, image)))
            {
                continue;
            }

            if (!imageCache.TryGetValue(relationshipId, out var image))
            {
                if (mainPart.GetPartById(relationshipId) is not ImagePart imagePart)
                {
                    throw new DocxImportException("Tài liệu chứa ảnh Word bị thiếu hoặc không hợp lệ.");
                }

                var supportedContentTypes = new[]
                {
                    "image/png", "image/jpeg", "image/gif", "image/bmp", "image/webp", "image/tiff"
                };
                if (!supportedContentTypes.Contains(imagePart.ContentType, StringComparer.OrdinalIgnoreCase))
                {
                    throw new DocxImportException(
                        $"Không hỗ trợ định dạng ảnh {imagePart.ContentType} trong tài liệu Word.");
                }

                using var imageStream = imagePart.GetStream(FileMode.Open, FileAccess.Read);
                using var buffer = new MemoryStream();
                imageStream.CopyTo(buffer);
                var imageData = buffer.ToArray();
                if (!ToeicQuestionImageValidator.IsValid(imagePart.ContentType, imageData))
                {
                    throw new DocxImportException("Tài liệu Word chứa ảnh bị hỏng hoặc không đúng định dạng.");
                }

                totalImageBytes = checked(totalImageBytes + imageData.LongLength);
                if (totalImageBytes > DocxUploadValidator.MaximumExpandedBytes)
                {
                    throw new DocxImportException("Tổng dung lượng hình ảnh vượt giới hạn giải nén 50 MB.");
                }

                image = new ToeicQuestionImageDto(imagePart.ContentType, imageData);
                imageCache.Add(relationshipId, image);
            }

            images.Add(image);
        }

        return images;
    }

    private static string NormalizeParagraph(Paragraph paragraph)
    {
        var parts = paragraph.Descendants().Select(element =>
            element switch
            {
                Text text => text.Text,
                TabChar => " ",
                Break => " ",
                _ => string.Empty
            });
        return Regex.Replace(string.Concat(parts), @"\s+", " ").Trim();
    }

    private static void FinishQuestion(
        ParsedQuestion question,
        bool hasQuestion,
        ICollection<ToeicQuestionPreviewDto> questions)
    {
        if (!hasQuestion)
        {
            return;
        }

        var missing = new[] { "A", "B", "C", "D" }
            .Where(key => !question.Choices.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value))
            .ToArray();
        if (missing.Length > 0)
        {
            throw new DocxImportException(
                $"Câu {question.Number} chưa có đủ lựa chọn A, B, C và D (thiếu: {string.Join(", ", missing)}).");
        }

        questions.Add(new ToeicQuestionPreviewDto(
            question.Part,
            question.Number,
            question.Text,
            question.Passage,
            question.Choices["A"],
            question.Choices["B"],
            question.Choices["C"],
            question.Choices["D"],
            question.CorrectAnswer,
            question.Images));
        if (questions.Count > 300)
        {
            throw new DocxImportException("Tài liệu vượt quá giới hạn 300 câu hỏi.");
        }
    }

    private sealed class ParsedQuestion
    {
        public int Part { get; init; }
        public int Number { get; init; }
        public string Text { get; set; }
        public string Passage { get; init; }
        public string CorrectAnswer { get; set; }
        public Dictionary<string, string> Choices { get; } = new(StringComparer.Ordinal);
        public List<ToeicQuestionImageDto> Images { get; init; } = new();
    }

    private sealed record ParsedParagraph(
        string Text,
        IReadOnlyList<ToeicQuestionImageDto> Images);
}
