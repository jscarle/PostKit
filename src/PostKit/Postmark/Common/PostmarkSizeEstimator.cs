using PostKit.Common;

namespace PostKit.Postmark.Common;

internal static class PostmarkSizeEstimator
{
    internal const long BodySizeLimitInBytes = 5L * 1024 * 1024;
    internal const long MessageSizeLimitInBytes = 10L * 1024 * 1024;
    internal const long BatchPayloadSizeLimitInBytes = 50L * 1024 * 1024;
    private const int Base64LowEndSlackDivisor = 10;

    internal static long EstimateBodySizeLowerBound(string? body)
    {
        return body?.Length ?? 0;
    }

    internal static long EstimateBase64SizeLowerBound(string? base64Content)
    {
        if (string.IsNullOrEmpty(base64Content))
            return 0;

        var encodedSize = base64Content.Length;
        encodedSize -= encodedSize / Base64LowEndSlackDivisor;

        return encodedSize;
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
