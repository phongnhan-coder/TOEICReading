using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TOEICReading.ToeicExams;

public static class DocxUploadValidator
{
    public const long MaximumUploadBytes = 25 * 1024 * 1024;
    public const long MaximumExpandedBytes = 50 * 1024 * 1024;
    private const int MaximumZipEntries = 1000;

    public static async Task<string> ValidateAsync(
        Stream stream,
        string fileName,
        long fileLength,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(Path.GetExtension(fileName), ".docx", StringComparison.OrdinalIgnoreCase))
        {
            return "Chỉ chấp nhận tệp có định dạng .docx.";
        }

        if (fileLength == 0)
        {
            return "Tệp đã chọn rỗng.";
        }

        if (fileLength > MaximumUploadBytes)
        {
            return "Dung lượng tệp tối đa là 25 MB.";
        }

        if (fileLength < 4)
        {
            return "Tệp không phải tài liệu Word .docx hợp lệ.";
        }

        var signature = new byte[4];
        var signatureLength = await stream.ReadAsync(signature.AsMemory(0, signature.Length), cancellationToken);
        if (signatureLength != signature.Length ||
            signature[0] != 0x50 ||
            signature[1] != 0x4B ||
            (signature[2] != 0x03 && signature[2] != 0x05 && signature[2] != 0x07))
        {
            return "Tệp không phải tài liệu Word .docx hợp lệ.";
        }

        stream.Position = 0;
        try
        {
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: true);
            if (archive.Entries.Count > MaximumZipEntries)
            {
                return "Tài liệu chứa quá nhiều thành phần để xử lý an toàn.";
            }

            long expandedBytes = 0;
            foreach (var entry in archive.Entries)
            {
                cancellationToken.ThrowIfCancellationRequested();
                expandedBytes = checked(expandedBytes + entry.Length);
                if (expandedBytes > MaximumExpandedBytes)
                {
                    return "Nội dung giải nén của tài liệu vượt quá giới hạn 50 MB.";
                }
            }

            var hasContentTypes = archive.Entries.Any(entry =>
                string.Equals(entry.FullName, "[Content_Types].xml", StringComparison.Ordinal));
            var hasDocument = archive.Entries.Any(entry =>
                string.Equals(entry.FullName, "word/document.xml", StringComparison.Ordinal));
            if (!hasContentTypes || !hasDocument)
            {
                return "Tệp ZIP không chứa cấu trúc tài liệu Word .docx.";
            }

            stream.Position = 0;
            return null;
        }
        catch (InvalidDataException)
        {
            return "Tệp .docx bị hỏng hoặc không phải gói Word hợp lệ.";
        }
        catch (OverflowException)
        {
            return "Kích thước nội dung trong tệp không hợp lệ.";
        }
    }
}
