using System.Text;
using MimeKit;
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

    internal static long EstimateAttachmentSizeLowerBound(string? base64Content)
    {
        if (string.IsNullOrEmpty(base64Content))
            return 0;

        var padding = 0;
        if (base64Content[^1] == '=')
        {
            padding = 1;
            if (base64Content.Length > 1 && base64Content[^2] == '=')
                padding = 2;
        }

        return (base64Content.Length / 4L * 3L) - padding;
    }

    internal static long EstimateAttachmentPayloadSizeLowerBound(IReadOnlyCollection<Attachment>? attachments)
    {
        if (attachments is null || attachments.Count == 0)
            return 0;

        long total = 0;
        foreach (var attachment in attachments)
            total += EstimateAttachmentSizeLowerBound(attachment.Content);

        return total;
    }

    internal static long EstimateMessageContentSizeLowerBound(string? textBody, string? htmlBody, int templateModelSizeInBytes, IReadOnlyCollection<Attachment>? attachments)
    {
        return EstimateBodySizeLowerBound(textBody)
            + EstimateBodySizeLowerBound(htmlBody)
            + templateModelSizeInBytes
            + EstimateAttachmentPayloadSizeLowerBound(attachments);
    }

    internal static long EstimateMessagePayloadSizeLowerBound(Emails.Email email)
    {
        ArgumentNullException.ThrowIfNull(email);

        return EstimateMessageContentSizeLowerBound(email.TextBody, email.HtmlBody, email.TemplateModelSizeInBytes, email.Attachments);
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

        long total = EstimateMessageContentSizeLowerBound(bulkEmail.TextBody, bulkEmail.HtmlBody, templateModelSizeInBytes: 0, bulkEmail.Attachments);

        foreach (var message in bulkEmail.Messages)
            total += message.TemplateModelSizeInBytes;

        return total;
    }

    internal static long EstimateStringSizeLowerBound(string? value)
    {
        return value is null ? 0 : Encoding.UTF8.GetByteCount(value);
    }

}
