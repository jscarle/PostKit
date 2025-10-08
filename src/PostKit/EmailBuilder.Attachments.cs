using System;
using System.Collections.Generic;
using System.Linq;

namespace PostKit;

partial class EmailBuilder
{
    private const int AttachmentSizeLimitInBytes = 10 * 1024 * 1024;
    private List<Attachment>? _attachments;
    private int _attachmentBytes;

    public IEmailBuilder WithAttachment(Attachment attachment)
    {
        ArgumentNullException.ThrowIfNull(attachment);

        EnsureAttachmentsWithinLimit(attachment.ContentLength);

        (_attachments ??= new List<Attachment>()).Add(attachment);
        _attachmentBytes += attachment.ContentLength;

        return this;
    }

    public IEmailBuilder WithAttachments(IEnumerable<Attachment> attachments)
    {
        ArgumentNullException.ThrowIfNull(attachments);

        var buffer = attachments as ICollection<Attachment> ?? attachments.ToList();
        if (buffer.Count == 0)
            return this;

        long additionalBytes = 0;
        foreach (var attachment in buffer)
        {
            ArgumentNullException.ThrowIfNull(attachment);
            additionalBytes += attachment.ContentLength;
        }

        EnsureAttachmentsWithinLimit(additionalBytes);

        (_attachments ??= new List<Attachment>()).AddRange(buffer);
        _attachmentBytes += (int)additionalBytes;

        return this;
    }

    internal void EnsureAttachmentConstraints()
    {
        EnsureAttachmentsWithinLimit(0);
    }

    private void EnsureAttachmentsWithinLimit(long additionalBytes)
    {
        if (additionalBytes < 0)
            throw new ArgumentOutOfRangeException(nameof(additionalBytes));

        var projectedTotal = (long)_attachmentBytes + additionalBytes;
        if (projectedTotal > AttachmentSizeLimitInBytes)
            throw new InvalidOperationException("Attachments exceed Postmark's 10 MB limit.");
    }
}
