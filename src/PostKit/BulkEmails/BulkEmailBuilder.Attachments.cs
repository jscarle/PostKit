using PostKit.Common;
using PostKit.Postmark.Common;

namespace PostKit.BulkEmails;

partial class BulkEmailBuilder
{
    private List<Attachment>? _attachments;
    private long _attachmentBytes;

    /// <inheritdoc/>
    public IBulkEmailBuilder WithAttachment(Attachment attachment)
    {
        ArgumentNullException.ThrowIfNull(attachment);

        var estimatedSize = PostmarkSizeEstimator.EstimateBase64SizeLowerBound(attachment.Content);
        EnsureAttachmentsWithinLimit(estimatedSize);

        (_attachments ??= []).Add(attachment);
        _attachmentBytes += estimatedSize;

        return this;
    }

    /// <inheritdoc/>
    public IBulkEmailBuilder WithAttachments(IEnumerable<Attachment> attachments)
    {
        ArgumentNullException.ThrowIfNull(attachments);

        var buffer = attachments as ICollection<Attachment> ?? attachments.ToList();
        if (buffer.Count == 0)
            return this;

        long additionalBytes = 0;
        foreach (var attachment in buffer)
        {
            ArgumentNullException.ThrowIfNull(attachment);
            additionalBytes += PostmarkSizeEstimator.EstimateBase64SizeLowerBound(attachment.Content);
        }

        EnsureAttachmentsWithinLimit(additionalBytes);

        (_attachments ??= []).AddRange(buffer);
        _attachmentBytes += additionalBytes;

        return this;
    }

    private void EnsureAttachmentsWithinLimit(long additionalBytes)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(additionalBytes);

        var projectedTotal = _attachmentBytes + additionalBytes;
        if (projectedTotal > PostmarkSizeEstimator.BulkPayloadSizeLimitInBytes)
            throw new InvalidOperationException("Estimated attachment content exceeds Postmark's 50 MB bulk payload limit.");
    }
}
