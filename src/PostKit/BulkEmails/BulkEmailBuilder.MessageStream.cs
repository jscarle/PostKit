using System.Diagnostics;
using PostKit.Common;
using PostKit.Postmark.Common;

namespace PostKit.BulkEmails;

partial class BulkEmailBuilder
{
    private string? _messageStream;

    /// <inheritdoc/>
    public IBulkEmailBuilder UsingMessageStream(MessageStream messageStream)
    {
        _messageStream.EnsureNotSet(nameof(BulkEmail.MessageStream));

        _messageStream = messageStream switch
        {
            MessageStream.Broadcast => "broadcast",
            MessageStream.Transactional => throw new ArgumentException("The Bulk API only supports broadcast message streams.", nameof(messageStream)),
            _ => throw new UnreachableException($"Enum value of '{nameof(MessageStream)}.{messageStream}' has not been handled."),
        };

        return this;
    }

    /// <inheritdoc/>
    public IBulkEmailBuilder UsingMessageStream(string messageStreamId)
    {
        _messageStream.EnsureNotSet(nameof(BulkEmail.MessageStream));

        if (!messageStreamId.AsSpan().IsValidMessageStreamId())
            throw new ArgumentException("The message stream ID is invalid.", nameof(messageStreamId));

        if (string.Equals(messageStreamId, "outbound", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("The Bulk API only supports broadcast message streams.", nameof(messageStreamId));

        _messageStream = messageStreamId;

        return this;
    }
}
