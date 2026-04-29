using System.Text;
using PostKit.BulkEmails;
using PostKit.Common;

namespace PostKit.Postmark.Common;

internal static class PostmarkSizeEstimator
{
    internal const long BodySizeLimitInBytes = 5L * 1024 * 1024;
    internal const long MessageSizeLimitInBytes = 10L * 1024 * 1024;
    internal const long BulkPayloadSizeLimitInBytes = 50L * 1024 * 1024;
    internal const long BatchPayloadSizeLimitInBytes = 50L * 1024 * 1024;

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

    private static long EstimateAttachmentPayloadSizeLowerBound(IReadOnlyCollection<Attachment>? attachments)
    {
        if (attachments is null || attachments.Count == 0)
            return 0;

        long total = 0;
        foreach (var attachment in attachments)
            total += EstimateBase64SizeLowerBound(attachment.Content);

        return total;
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

    internal static long EstimateMessageContentSizeLowerBound(
        string? textBody,
        string? htmlBody,
        int templateModelSizeInBytes,
        IReadOnlyCollection<Attachment>? attachments,
        IReadOnlyDictionary<string, string>? headers = null
    )
    {
        return EstimateBodySizeLowerBound(textBody)
            + EstimateBodySizeLowerBound(htmlBody)
            + templateModelSizeInBytes
            + EstimateAttachmentPayloadSizeLowerBound(attachments)
            + EstimateHeaderSizeLowerBound(headers);
    }

    internal static long EstimateMessagePayloadSizeLowerBound(Emails.Email email)
    {
        ArgumentNullException.ThrowIfNull(email);

        return EstimateMessageContentSizeLowerBound(email.TextBody, email.HtmlBody, email.TemplateModelSizeInBytes, email.Attachments, email.Headers)
            + EstimateMetadataSizeLowerBound(email.Metadata);
    }

    internal static long EstimateBatchPayloadSizeLowerBound(IReadOnlyCollection<Emails.Email> emails)
    {
        ArgumentNullException.ThrowIfNull(emails);

        long total = 0;
        foreach (var email in emails)
        {
            ArgumentNullException.ThrowIfNull(email);
            total += EstimateMessagePayloadSizeLowerBound(email);
        }

        return total;
    }

    internal static long EstimateBulkEmailPayloadSizeLowerBound(BulkEmail bulkEmail)
    {
        ArgumentNullException.ThrowIfNull(bulkEmail);

        long total = EstimateMessageContentSizeLowerBound(
                         bulkEmail.TextBody,
                         bulkEmail.HtmlBody,
                         templateModelSizeInBytes: 0,
                         bulkEmail.Attachments,
                         bulkEmail.Headers
                     )
                     + EstimateMetadataSizeLowerBound(bulkEmail.Metadata);

        foreach (var message in bulkEmail.Messages)
        {
            total += message.TemplateModelSizeInBytes;
            total += EstimateHeaderSizeLowerBound(message.Headers);
            total += EstimateMetadataSizeLowerBound(message.Metadata);
        }

        return total;
    }

    private static long EstimateStringSizeLowerBound(string? value)
    {
        return value is null ? 0 : Encoding.UTF8.GetByteCount(value);
    }

}
