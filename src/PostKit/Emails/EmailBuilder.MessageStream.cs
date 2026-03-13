using System.Diagnostics;
using PostKit.Common;
using PostKit.Postmark.Common;

namespace PostKit.Emails;

partial class EmailBuilder
{
    private string? _messageStream;

    /// <inheritdoc/>
    public IEmailBuilder UsingMessageStream(MessageStream messageStream)
    {
        _messageStream.EnsureNotSet(nameof(Email.MessageStream));

        _messageStream = messageStream switch
        {
            MessageStream.Transactional => "outbound",
            MessageStream.Broadcast => "broadcast",
            _ => throw new UnreachableException($"Enum value of '{nameof(MessageStream)}.{messageStream}' has not been handled."),
        };

        return this;
    }

    /// <inheritdoc/>
    public IEmailBuilder UsingMessageStream(string messageStreamId)
    {
        _messageStream.EnsureNotSet(nameof(Email.MessageStream));

        if (!messageStreamId.AsSpan().IsValidMessageStreamId())
            throw new ArgumentException("The message stream ID is invalid.", nameof(messageStreamId));

        _messageStream = messageStreamId;

        return this;
    }
}
