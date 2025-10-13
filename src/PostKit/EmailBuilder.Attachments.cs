namespace PostKit;

partial class EmailBuilder
{
    private List<Attachment>? _attachments;
    private int _attachmentBytes;

    /// <inheritdoc/>
    public IEmailBuilder WithAttachment(Attachment attachment)
    {
        ArgumentNullException.ThrowIfNull(attachment);

        EnsureAttachmentsWithinLimit(attachment.Content.Length);

        (_attachments ??= []).Add(attachment);
        _attachmentBytes += attachment.Content.Length;

        return this;
    }

    /// <inheritdoc/>
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
            additionalBytes += attachment.Content.Length;
        }

        EnsureAttachmentsWithinLimit(additionalBytes);

        (_attachments ??= []).AddRange(buffer);
        _attachmentBytes += (int)additionalBytes;

        return this;
    }

    private void EnsureAttachmentsWithinLimit(long additionalBytes)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(additionalBytes);

        var projectedTotal = _attachmentBytes + additionalBytes;
        if (projectedTotal > MessageSizeLimitInBytes)
            throw new InvalidOperationException("Attachments exceed Postmark's 10 MB limit.");
    }
}
