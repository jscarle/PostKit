using System.Text;
using PostKit.Common;

namespace PostKit.Postmark.Common;

internal static class PostmarkSizeEstimator
{
    internal const long BodySizeLimitInBytes = 5L * 1024 * 1024;
    internal const long MessageSizeLimitInBytes = 10L * 1024 * 1024;
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

    internal static long EstimateAttachmentsSizeLowerBound(IReadOnlyCollection<Attachment>? attachments)
    {
        if (attachments is null || attachments.Count == 0)
            return 0;

        long total = 0;
        foreach (var attachment in attachments)
            total += EstimateBase64SizeLowerBound(attachment.Content);

        return total;
    }

    internal static long EstimateMessageSizeLowerBound(Emails.Email email)
    {
        return EstimateAttachmentsSizeLowerBound(email.Attachments)
            + EstimateBodySizeLowerBound(email.TextBody)
            + EstimateBodySizeLowerBound(email.HtmlBody);
    }
}
