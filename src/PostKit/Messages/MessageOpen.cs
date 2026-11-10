using JetBrains.Annotations;
using PostKit.Common;

namespace PostKit.Messages;

/// <summary>Represents a message open event.</summary>
public sealed record MessageOpen : MessageTrackingEvent
{
    internal MessageOpen(string? recordType, MessageTrackingClient? client, MessageTrackingOperatingSystem? os, MessagePlatform? platform, string userAgent, MessageTrackingGeo? geo, Guid messageId, string messageStream, DateTimeOffset receivedAt,
        string? tag, string? recipient, TimeSpan? readTime = null) : base(recordType, client, os, platform, userAgent, geo, messageId, messageStream, receivedAt, tag, recipient)
    {
        ReadTime = readTime;
    }

    /// <summary>Gets the recorded reading time, when Postmark supplies it.</summary>
    public TimeSpan? ReadTime { [UsedImplicitly] get; }
}
