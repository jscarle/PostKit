using System.Globalization;

namespace PostKit.Common;

internal static class PostmarkSizeLimits
{
    // Verified against Postmark's API validators on November 11, 2026.
    // Bodies use UTF-16 character counts; attachments use their combined decoded byte count.
    internal const int BodyCharacterLimit = 5 * 1024 * 1024;
    internal const long AttachmentSizeLimitInBytes = 10L * 1024 * 1024;

    // Bulk rejects exactly 10 MiB, whereas Email and Batch accept it.
    internal const long BulkAttachmentSizeLimitInBytes = AttachmentSizeLimitInBytes - 1;
    internal const long PayloadSizeLimitInBytes = 50L * 1024 * 1024;

    internal static string FormatSizeLimitMessage(string message, long actualSizeInBytes, long limitInBytes)
    {
        return $"{message} Actual size: {actualSizeInBytes.ToString("N0", CultureInfo.InvariantCulture)} bytes. Limit: {limitInBytes.ToString("N0", CultureInfo.InvariantCulture)} bytes.";
    }

    internal static string FormatBodyLengthLimitMessage(string propertyName, int actualLength)
    {
        var formattedLimit = BodyCharacterLimit.ToString("N0", CultureInfo.InvariantCulture);
        var formattedLength = actualLength.ToString("N0", CultureInfo.InvariantCulture);
        return $"{propertyName} exceeds Postmark's limit of {formattedLimit} characters. Actual length: {formattedLength}. Shorten {propertyName}(...).";
    }

    internal static long GetAttachmentContentSize(IReadOnlyCollection<Attachment>? attachments)
    {
        long total = 0;
        if (attachments is null)
            return total;

        foreach (var attachment in attachments)
            total += attachment.ContentSizeInBytes;

        return total;
    }
}
