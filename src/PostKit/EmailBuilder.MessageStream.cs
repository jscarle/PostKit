using System.Diagnostics;
using PostKit.Common;

namespace PostKit;

partial class EmailBuilder
{
    private string? _messageStream;

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

    public IEmailBuilder UsingMessageStream(string messageStreamId)
    {
        _messageStream.EnsureNotSet(nameof(Email.MessageStream));

        if (!IsValidMessageStreamId(messageStreamId))
            throw new ArgumentException("The message stream ID is invalid.", nameof(messageStreamId));

        return this;
    }

    private static bool IsValidMessageStreamId(ReadOnlySpan<char> streamId)
    {
        // Check length constraints
        if (streamId.Length is 0 or > 30)
            return false;

        // Check if the streamId starts with a number
        if (char.IsDigit(streamId[0]))
            return false;

        // Check if the streamId starts or ends with a dash
        if (streamId[0] == '-' || streamId[^1] == '-')
            return false;

        // Check character constraints and for repeating dashes
        var previousWasDash = false;
        foreach (var ch in streamId)
        {
            if (ch is '-' or >= 'a' and <= 'z' or >= '0' and <= '9')
            {
                if (ch == '-')
                {
                    if (previousWasDash)
                        return false; // Repeating dashes
                    previousWasDash = true;
                }
                else
                {
                    previousWasDash = false;
                }
            }
            else
            {
                return false; // Invalid character
            }
        }

        return true;
    }
}
