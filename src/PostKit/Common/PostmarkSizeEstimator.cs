using System.Globalization;
using System.Text;
using PostKit.BulkEmails;
using PostKit.Emails;

namespace PostKit.Common;

internal static class PostmarkSizeEstimator
{
    internal const long BodySizeLimitInBytes = 5L * 1024 * 1024;
    internal const long MessageSizeLimitInBytes = 10L * 1024 * 1024;
    internal const long BulkPayloadSizeLimitInBytes = 50L * 1024 * 1024;
    internal const long BatchPayloadSizeLimitInBytes = 50L * 1024 * 1024;

    internal static string FormatActualSizeLimitMessage(string message, long actualSizeInBytes, long limitInBytes)
    {
        return $"{message} Actual size: {FormatByteCount(actualSizeInBytes)}. Limit: {FormatByteCount(limitInBytes)}.";
    }

    internal static string FormatEstimatedSizeLimitMessage(string message, long estimatedSizeInBytes, long limitInBytes)
    {
        return $"{message} Estimated size: {FormatByteCount(estimatedSizeInBytes)}. Limit: {FormatByteCount(limitInBytes)}.";
    }

    internal static long EstimateBodySizeLowerBound(string? body)
    {
        return body is null ? 0 : Encoding.UTF8.GetByteCount(body);
    }

    internal static long EstimateBase64SizeLowerBound(string? base64Content)
    {
        if (string.IsNullOrEmpty(base64Content))
            return 0;

        return base64Content.Length;
    }

    internal static long EstimateHeaderSizeLowerBound(IReadOnlyDictionary<string, string>? headers)
    {
        if (headers is null || headers.Count == 0)
            return 0;

        long total = 0;
        foreach (var header in headers)
        {
            total += EstimateStringSizeLowerBound(header.Key);
            total += EstimateStringSizeLowerBound(header.Value);
            total += 4; // ": " + CRLF
        }

        return total;
    }

    internal static long EstimateMessageContentSizeLowerBound(string? textBody, string? htmlBody, int templateModelSizeInBytes, IReadOnlyCollection<Attachment>? attachments, IReadOnlyDictionary<string, string>? headers = null)
    {
        return EstimateBodySizeLowerBound(textBody) + EstimateBodySizeLowerBound(htmlBody) + templateModelSizeInBytes + EstimateAttachmentPayloadSizeLowerBound(attachments) + EstimateHeaderSizeLowerBound(headers);
    }

    internal static long EstimateMessagePayloadSizeLowerBound(Email email)
    {
        ArgumentNullException.ThrowIfNull(email);

        return EstimateMessageContentSizeLowerBound(email.TextBody, email.HtmlBody, email.TemplateModelSizeInBytes, email.Attachments, email.Headers) + EstimateMetadataSizeLowerBound(email.Metadata);
    }

    internal static long EstimateBatchPayloadSizeLowerBound(IReadOnlyCollection<Email> emails)
    {
        ArgumentNullException.ThrowIfNull(emails);

        long total = 0;
        var index = 0;
        foreach (var email in emails)
        {
            if (email is null)
                throw new ArgumentException($"The email at index {index} cannot be null.", nameof(emails));

            total += EstimateMessagePayloadSizeLowerBound(email);
            index++;
        }

        return total;
    }

    internal static long EstimateBulkEmailPayloadSizeLowerBound(BulkEmail bulkEmail)
    {
        ArgumentNullException.ThrowIfNull(bulkEmail);

        var total = EstimateMessageContentSizeLowerBound(bulkEmail.TextBody, bulkEmail.HtmlBody, 0, bulkEmail.Attachments, bulkEmail.Headers) + EstimateMetadataSizeLowerBound(bulkEmail.Metadata);

        foreach (var message in bulkEmail.Messages)
        {
            total += message.TemplateModelSizeInBytes;
            total += EstimateHeaderSizeLowerBound(message.Headers);
            total += EstimateMetadataSizeLowerBound(message.Metadata);
        }

        return total;
    }

    private static long EstimateAttachmentPayloadSizeLowerBound(IReadOnlyCollection<Attachment>? attachments)
    {
        if (attachments is null || attachments.Count == 0)
            return 0;

        long total = 0;
        foreach (var attachment in attachments)
            total += EstimateBase64SizeLowerBound(attachment.Content);

        return total;
    }

    private static long EstimateMetadataSizeLowerBound(IReadOnlyDictionary<string, string>? metadata)
    {
        if (metadata is null || metadata.Count == 0)
            return 0;

        long total = 0;
        foreach (var entry in metadata)
        {
            total += EstimateStringSizeLowerBound(entry.Key);
            total += EstimateStringSizeLowerBound(entry.Value);
        }

        return total;
    }

    private static long EstimateStringSizeLowerBound(string? value)
    {
        return value is null ? 0 : Encoding.UTF8.GetByteCount(value);
    }

    private static string FormatByteCount(long value)
    {
        return $"{value.ToString("N0", CultureInfo.InvariantCulture)} bytes";
    }
}
