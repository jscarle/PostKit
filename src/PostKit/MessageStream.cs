using System.Diagnostics.CodeAnalysis;

namespace PostKit;

[SuppressMessage("Naming", "CA1711: Identifiers should not have incorrect suffix", Justification = "Postmark calls it a MessageStream, CA1711. You’re the one making it awkward.")]
public enum MessageStream
{
    Transactional = 0,
    Broadcast = 1,
}
