using RapidOcrNet;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TOEICReading.ToeicExams;

/// <summary>Runs the bundled Latin PP-OCR model on embedded DOCX images.</summary>
public sealed class RapidOcrDocxImageOcrService : IDocxImageOcrService, IDisposable
{
    private const int MaximumImageDimension = 8000;
    private const long MaximumImagePixels = 20_000_000;
    private readonly object _sync = new();
    private readonly RapidOcr _ocr = new();
    private bool _initialized;
    private bool _disposed;

    /// <inheritdoc />
    public IReadOnlyList<string> Recognize(byte[] imageData)
    {
        ArgumentNullException.ThrowIfNull(imageData);
        using var imageStream = new MemoryStream(imageData, writable: false);
        using var codec = SKCodec.Create(imageStream);
        if (codec is null)
        {
            throw new DocxImportException("Không thể giải mã ảnh trong tài liệu Word để nhận dạng.");
        }

        var imageInfo = codec.Info;
        if (imageInfo.Width <= 0 ||
            imageInfo.Height <= 0 ||
            imageInfo.Width > MaximumImageDimension ||
            imageInfo.Height > MaximumImageDimension ||
            (long)imageInfo.Width * imageInfo.Height > MaximumImagePixels)
        {
            throw new DocxImportException("Kích thước ảnh vượt quá giới hạn OCR an toàn.");
        }

        using var bitmap = SKBitmap.Decode(codec);
        if (bitmap is null)
        {
            throw new DocxImportException("Không thể giải mã ảnh trong tài liệu Word để nhận dạng.");
        }

        lock (_sync)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            InitializeEngine();
            var detectedBlocks = _ocr.Detect(bitmap, RapidOcrOptions.Default)
                .TextBlocks
                .Select(block =>
                {
                    var points = block.BoxPoints;
                    var minX = points.Min(point => point.X);
                    var maxX = points.Max(point => point.X);
                    return new DetectedTextBlock(
                        block.Text.Trim(),
                        minX,
                        maxX,
                        points.Min(point => point.Y));
                })
                .Where(block => block.Text.Length > 0)
                .ToArray();
            var middleX = bitmap.Width / 2f;
            var leftColumnCount = detectedBlocks.Count(block =>
                block.MaxX - block.MinX <= bitmap.Width * 0.55f && block.MaxX < middleX);
            var rightColumnCount = detectedBlocks.Count(block =>
                block.MaxX - block.MinX <= bitmap.Width * 0.55f && block.MinX > middleX);
            var hasTwoColumns = leftColumnCount >= 3 && rightColumnCount >= 3;
            var orderedBlocks = hasTwoColumns
                ? detectedBlocks
                    .OrderBy(block => block.MaxX - block.MinX > bitmap.Width * 0.55f
                        ? -1
                        : block.MinX > middleX ? 1 : 0)
                    .ThenBy(block => block.MinY)
                    .ThenBy(block => block.MinX)
                : detectedBlocks
                    .OrderBy(block => block.MinY)
                    .ThenBy(block => block.MinX);

            return orderedBlocks
                .Select(block => block.Text)
                .Where(text => text.Length > 0)
                .ToArray();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        lock (_sync)
        {
            if (_disposed)
            {
                return;
            }

            _ocr.Dispose();
            _disposed = true;
        }
    }

    private void InitializeEngine()
    {
        if (_initialized)
        {
            return;
        }

        var modelDirectory = Path.Combine(AppContext.BaseDirectory, "models", "v5");
        var detectorPath = Path.Combine(modelDirectory, "ch_PP-OCRv5_mobile_det.onnx");
        var classifierPath = Path.Combine(modelDirectory, "ch_PP-LCNet_x0_25_textline_ori_cls_mobile.onnx");
        var recognizerPath = Path.Combine(modelDirectory, "latin_PP-OCRv5_rec_mobile_infer.onnx");
        var dictionaryPath = Path.Combine(modelDirectory, "ppocrv5_latin_dict.txt");
        var missingModel = new[] { detectorPath, classifierPath, recognizerPath, dictionaryPath }
            .FirstOrDefault(path => !File.Exists(path));
        if (missingModel is not null)
        {
            throw new InvalidOperationException(
                "The bundled English OCR model files are missing from the application output.");
        }

        _ocr.InitModels(detectorPath, classifierPath, recognizerPath, dictionaryPath);
        _initialized = true;
    }

    private sealed record DetectedTextBlock(string Text, float MinX, float MaxX, float MinY);
}
