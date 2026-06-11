using JetBrains.Annotations;

namespace PostKit.Messages;

/// <summary>Represents the raw source returned from the Postmark outbound message dump endpoint.</summary>
public sealed record OutboundMessageDump
{
    /// <summary>Gets the raw source of the outbound message.</summary>
    public string Body { [UsedImplicitly] get; }

    internal OutboundMessageDump(string body)
    {
        Body = body;
    }
}
