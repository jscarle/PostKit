namespace PostKit.Messages;

/// <summary>Represents a message open event.</summary>
public sealed record MessageOpen : MessageTrackingEvent
{
    internal MessageOpen(string? recordType, MessageTrackingClient? client, MessageTrackingOperatingSystem? os, string? platform, string userAgent, MessageTrackingGeo? geo, Guid messageId, string messageStream, DateTimeOffset receivedAt,
        string? tag, string? recipient) : base(recordType, client, os, platform, userAgent, geo, messageId, messageStream, receivedAt, tag, recipient)
    {
    }
}