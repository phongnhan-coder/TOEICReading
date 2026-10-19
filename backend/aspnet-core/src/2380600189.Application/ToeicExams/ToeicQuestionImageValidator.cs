using System;

namespace TOEICReading.ToeicExams;

public static class ToeicQuestionImageValidator
{
    public static bool IsValid(string contentType, byte[] data)
    {
        if (data is null || data.Length < 4)
        {
            return false;
        }

        return contentType?.ToLowerInvariant() switch
        {
            "image/png" => data.Length >= 8 &&
                data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47 &&
                data[4] == 0x0D && data[5] == 0x0A && data[6] == 0x1A && data[7] == 0x0A,
            "image/jpeg" => data[0] == 0xFF && data[1] == 0xD8 && data[2] == 0xFF,
            "image/gif" => data[0] == 0x47 && data[1] == 0x49 && data[2] == 0x46 &&
                data[3] == 0x38 && (data[4] == 0x37 || data[4] == 0x39) && data[5] == 0x61,
            "image/bmp" => data[0] == 0x42 && data[1] == 0x4D,
            "image/webp" => data.Length >= 12 &&
                data[0] == 0x52 && data[1] == 0x49 && data[2] == 0x46 && data[3] == 0x46 &&
                data[8] == 0x57 && data[9] == 0x45 && data[10] == 0x42 && data[11] == 0x50,
            "image/tiff" =>
                (data[0] == 0x49 && data[1] == 0x49 && data[2] == 0x2A && data[3] == 0x00) ||
                (data[0] == 0x4D && data[1] == 0x4D && data[2] == 0x00 && data[3] == 0x2A),
            _ => false
        };
    }
}
