using System.Diagnostics.CodeAnalysis;

namespace PostKit;

/// <summary>Identifies the Postmark message stream that an email should be sent through.</summary>
[SuppressMessage("Naming", "CA1711: Identifiers should not have incorrect suffix", Justification = "Postmark calls it a MessageStream, CA1711. You're the one making it awkward.")]
public enum MessageStream
{
    /// <summary>Represents the transactional message stream.</summary>
    Transactional = 0,

    /// <summary>Represents the broadcast message stream.</summary>
    Broadcast = 1,
}
