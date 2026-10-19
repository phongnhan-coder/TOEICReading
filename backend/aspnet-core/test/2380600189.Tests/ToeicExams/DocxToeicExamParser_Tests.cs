using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using TOEICReading.ToeicExams;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using SkiaSharp;

namespace TOEICReading.Tests.ToeicExams;

public class DocxToeicExamParser_Tests
{
    private readonly DocxToeicExamParser _parser;

    public DocxToeicExamParser_Tests()
    {
        _parser = new DocxToeicExamParser(new TestImageOcrService());
    }

    [Fact]
    public async Task Parse_valid_docx_returns_questions_and_passages_for_parts_5_6_and_7()
    {
        using var stream = CreateDocx(
            "Part 5",
            "1. The manager _____ the report.",
            "A. review",
            "B. reviews",
            "C. reviewing",
            "D. reviewed",
            "Answer: B",
            "Part 6",
            "Questions 2-2 refer to the following notice.",
            "Office hours will change next week.",
            "2. When will the new schedule begin?",
            "A. Monday",
            "B. Tuesday",
            "C. Friday",
            "D. Saturday",
            "Part 7",
            "Questions 3-3 refer to the following email.",
            "Dear staff, the seminar begins at nine.",
            "3. What time does the seminar begin?",
            "A. 8:00",
            "B. 8:30",
            "C. 9:00",
            "D. 9:30");

        var validation = await DocxUploadValidator.ValidateAsync(
            stream,
            "reading.docx",
            stream.Length,
            CancellationToken.None);
        Assert.Null(validation);

        var result = _parser.Parse(stream, "reading.docx");

        Assert.Equal("reading", result.SuggestedTitle);
        Assert.Equal(3, result.Questions.Count);
        Assert.Equal("B", result.Questions[0].CorrectAnswer);
        Assert.Equal("Office hours will change next week.", result.Questions[1].Passage);
        Assert.Equal("Dear staff, the seminar begins at nine.", result.Questions[2].Passage);
    }

    [Fact]
    public void Parse_docx_preserves_inline_question_images()
    {
        var imageBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        using var stream = CreateDocxWithImage(imageBytes);

        var result = _parser.Parse(stream, "image-reading.docx");

        Assert.Single(result.Questions);
        var image = Assert.Single(result.Questions[0].Images);
        Assert.Equal("image/png", image.ContentType);
        Assert.Equal(imageBytes, image.Data);
    }

    [Fact]
    public void Parse_image_only_docx_reports_when_ocr_cannot_read_a_question()
    {
        var imageBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        using var stream = CreateDocxWithImage(imageBytes, includeQuestionText: false);

        var error = Assert.Throws<DocxImportException>(() => _parser.Parse(stream, "scanned-reading.docx"));

        Assert.Contains("OCR không nhận diện được câu hỏi", error.Message);
    }

    [Fact]
    public void Parse_image_only_docx_parses_ocr_questions_and_keeps_the_page_image()
    {
        var imageBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        using var stream = CreateDocxWithImage(imageBytes, includeQuestionText: false);
        var parser = new DocxToeicExamParser(new TestImageOcrService(
            "PART 5",
            "1. The manager reviews the report. A. review",
            "B. reviews",
            "C. reviewing",
            "D. reviewed"));

        var result = parser.Parse(stream, "scanned-reading.docx");

        var question = Assert.Single(result.Questions);
        Assert.Equal(5, question.Part);
        Assert.Equal("The manager reviews the report.", question.QuestionText);
        Assert.Equal("review", question.OptionA);
        Assert.Equal("reviews", question.OptionB);
        Assert.Equal(imageBytes, Assert.Single(question.Images).Data);
    }

    [Fact]
    public void Parse_image_only_docx_uses_portable_ocr_for_scanned_question_text()
    {
        using var stream = CreateDocxWithImage(CreateQuestionImage(), includeQuestionText: false);
        using var ocrService = new RapidOcrDocxImageOcrService();
        var parser = new DocxToeicExamParser(ocrService);

        var result = parser.Parse(stream, "scanned-reading.docx");

        var question = Assert.Single(result.Questions);
        Assert.Equal(5, question.Part);
        Assert.Contains("manager", question.QuestionText, System.StringComparison.OrdinalIgnoreCase);
        Assert.Equal("review", question.OptionA, ignoreCase: true);
        Assert.Equal("reviews", question.OptionB, ignoreCase: true);
        Assert.Equal("reviewing", question.OptionC, ignoreCase: true);
        Assert.Equal("reviewed", question.OptionD, ignoreCase: true);
    }

    [Fact]
    public async Task Validate_rejects_wrong_extension()
    {
        using var stream = new MemoryStream(new byte[] { 1, 2, 3, 4 });

        var error = await DocxUploadValidator.ValidateAsync(
            stream,
            "reading.doc",
            stream.Length,
            CancellationToken.None);

        Assert.Equal("Chỉ chấp nhận tệp có định dạng .docx.", error);
    }

    [Fact]
    public async Task Validate_rejects_empty_file()
    {
        using var stream = new MemoryStream();

        var error = await DocxUploadValidator.ValidateAsync(
            stream,
            "empty.docx",
            stream.Length,
            CancellationToken.None);

        Assert.Equal("Tệp đã chọn rỗng.", error);
    }

    [Fact]
    public async Task Validate_rejects_file_over_25_megabytes()
    {
        using var stream = new MemoryStream();

        var error = await DocxUploadValidator.ValidateAsync(
            stream,
            "large.docx",
            DocxUploadValidator.MaximumUploadBytes + 1,
            CancellationToken.None);

        Assert.Equal("Dung lượng tệp tối đa là 25 MB.", error);
    }

    [Fact]
    public async Task Validate_accepts_docx_at_25_megabyte_limit()
    {
        using var stream = CreateDocx();

        var error = await DocxUploadValidator.ValidateAsync(
            stream,
            "reading.docx",
            DocxUploadValidator.MaximumUploadBytes,
            CancellationToken.None);

        Assert.Null(error);
    }

    [Fact]
    public async Task Validate_rejects_non_word_zip()
    {
        using var stream = new MemoryStream(new byte[] { 0x50, 0x4B, 0x03, 0x04 });

        var error = await DocxUploadValidator.ValidateAsync(
            stream,
            "not-word.docx",
            stream.Length,
            CancellationToken.None);

        Assert.Equal("Tệp .docx bị hỏng hoặc không phải gói Word hợp lệ.", error);
    }

    [Fact]
    public void Parse_empty_docx_fails_instead_of_returning_success_shaped_preview()
    {
        using var stream = CreateDocx();

        var error = Assert.Throws<DocxImportException>(() => _parser.Parse(stream, "empty.docx"));

        Assert.Contains("Không tìm thấy câu hỏi", error.Message);
    }

    private static MemoryStream CreateDocx(params string[] paragraphs)
    {
        var stream = new MemoryStream();
        using (var document = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document, true))
        {
            var mainPart = document.AddMainDocumentPart();
            mainPart.Document = new Document(new Body());
            foreach (var text in paragraphs)
            {
                mainPart.Document.Body.AppendChild(
                    new Paragraph(new Run(new Text(text) { Space = SpaceProcessingModeValues.Preserve })));
            }

            mainPart.Document.Save();
        }

        stream.Position = 0;
        return stream;
    }

    private static MemoryStream CreateDocxWithImage(byte[] imageBytes) =>
        CreateDocxWithImage(imageBytes, includeQuestionText: true);

    private static MemoryStream CreateDocxWithImage(byte[] imageBytes, bool includeQuestionText)
    {
        var stream = new MemoryStream();
        using (var document = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document, true))
        {
            var mainPart = document.AddMainDocumentPart();
            mainPart.Document = new Document(new Body());
            var paragraphs = includeQuestionText
                ? new[]
                     {
                         "Part 5",
                         "1. Read the diagram.",
                         "A. First",
                         "B. Second",
                         "C. Third",
                         "D. Fourth"
                     }
                : System.Array.Empty<string>();

            foreach (var text in paragraphs)
            {
                mainPart.Document.Body.AppendChild(
                    new Paragraph(new Run(new Text(text) { Space = SpaceProcessingModeValues.Preserve })));
                if (text.StartsWith("1.", System.StringComparison.Ordinal))
                {
                    mainPart.Document.Body.AppendChild(CreateImageParagraph(mainPart, imageBytes));
                }
            }

            if (!includeQuestionText)
            {
                mainPart.Document.Body.AppendChild(CreateImageParagraph(mainPart, imageBytes));
            }

            mainPart.Document.Save();
        }

        stream.Position = 0;
        return stream;
    }

    private static Paragraph CreateImageParagraph(MainDocumentPart mainPart, byte[] imageBytes)
    {
        var imagePart = mainPart.AddImagePart(ImagePartType.Png);
        using (var imageStream = new MemoryStream(imageBytes))
        {
            imagePart.FeedData(imageStream);
        }

        var relationshipId = mainPart.GetIdOfPart(imagePart);
        var drawing = new Drawing(
            new DocumentFormat.OpenXml.Drawing.Wordprocessing.Inline(
                new DocumentFormat.OpenXml.Drawing.Wordprocessing.Extent { Cx = 990000L, Cy = 792000L },
                new DocumentFormat.OpenXml.Drawing.Wordprocessing.DocProperties
                {
                    Id = 1U,
                    Name = "question-image"
                },
                new DocumentFormat.OpenXml.Drawing.Wordprocessing.NonVisualGraphicFrameDrawingProperties(),
                new DocumentFormat.OpenXml.Drawing.Graphic(
                    new DocumentFormat.OpenXml.Drawing.GraphicData(
                        new DocumentFormat.OpenXml.Drawing.Pictures.Picture(
                            new DocumentFormat.OpenXml.Drawing.Pictures.NonVisualPictureProperties(
                                new DocumentFormat.OpenXml.Drawing.Pictures.NonVisualDrawingProperties
                                {
                                    Id = 0U,
                                    Name = "question-image.png"
                                },
                                new DocumentFormat.OpenXml.Drawing.Pictures.NonVisualPictureDrawingProperties()),
                            new DocumentFormat.OpenXml.Drawing.Pictures.BlipFill(
                                new DocumentFormat.OpenXml.Drawing.Blip { Embed = relationshipId },
                                new DocumentFormat.OpenXml.Drawing.Stretch(
                                    new DocumentFormat.OpenXml.Drawing.FillRectangle())),
                            new DocumentFormat.OpenXml.Drawing.Pictures.ShapeProperties(
                                new DocumentFormat.OpenXml.Drawing.Transform2D(
                                    new DocumentFormat.OpenXml.Drawing.Offset { X = 0L, Y = 0L },
                                    new DocumentFormat.OpenXml.Drawing.Extents { Cx = 990000L, Cy = 792000L }),
                                new DocumentFormat.OpenXml.Drawing.PresetGeometry(
                                    new DocumentFormat.OpenXml.Drawing.AdjustValueList())
                                {
                                    Preset = DocumentFormat.OpenXml.Drawing.ShapeTypeValues.Rectangle
                                }))
                    )
                    {
                        Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture"
                    })
            ));

        return new Paragraph(new Run(drawing));
    }

    private static byte[] CreateQuestionImage()
    {
        using var bitmap = new SKBitmap(1800, 1300);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.White);
        using var font = new SKFont(SKTypeface.FromFamilyName("Arial"), 70);
        using var paint = new SKPaint
        {
            Color = SKColors.Black,
            IsAntialias = true
        };

        canvas.DrawText("PART 5", 100, 150, SKTextAlign.Left, font, paint);
        canvas.DrawText("1. The manager reviews the report.", 100, 320, SKTextAlign.Left, font, paint);
        canvas.DrawText("A. review", 100, 490, SKTextAlign.Left, font, paint);
        canvas.DrawText("B. reviews", 100, 660, SKTextAlign.Left, font, paint);
        canvas.DrawText("C. reviewing", 100, 830, SKTextAlign.Left, font, paint);
        canvas.DrawText("D. reviewed", 100, 1000, SKTextAlign.Left, font, paint);

        using var image = SKImage.FromBitmap(bitmap);
        using var encoded = image.Encode(SKEncodedImageFormat.Png, 100);
        return encoded.ToArray();
    }

    private sealed class TestImageOcrService : IDocxImageOcrService
    {
        private readonly string[] _lines;

        public TestImageOcrService(params string[] lines)
        {
            _lines = lines;
        }

        public System.Collections.Generic.IReadOnlyList<string> Recognize(byte[] imageData) => _lines;
    }
}
