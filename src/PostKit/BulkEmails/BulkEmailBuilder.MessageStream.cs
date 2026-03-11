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

        if (!IsValidMessageStreamId(messageStreamId))
            throw new ArgumentException("The message stream ID is invalid.", nameof(messageStreamId));

        if (string.Equals(messageStreamId, "outbound", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("The Bulk API only supports broadcast message streams.", nameof(messageStreamId));

        _messageStream = messageStreamId;

        return this;
    }

    private static bool IsValidMessageStreamId(ReadOnlySpan<char> streamId)
    {
        if (streamId.Length is 0 or > 30)
            return false;

        if (char.IsDigit(streamId[0]))
            return false;

        if (streamId[0] == '-' || streamId[^1] == '-')
            return false;

        var previousWasDash = false;
        foreach (var ch in streamId)
        {
            if (ch is '-' or >= 'a' and <= 'z' or >= '0' and <= '9')
            {
                if (ch == '-')
                {
                    if (previousWasDash)
                        return false;

                    previousWasDash = true;
                }
                else
                {
                    previousWasDash = false;
                }
            }
            else
            {
                return false;
            }
        }

        return true;
    }
}
