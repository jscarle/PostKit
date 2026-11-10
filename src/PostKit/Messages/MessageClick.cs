using JetBrains.Annotations;
using PostKit.Common;

namespace PostKit.Messages;

/// <summary>Represents a message click event.</summary>
public sealed record MessageClick : MessageTrackingEvent
{
    internal MessageClick(string? recordType, LinkClickLocation clickLocation, MessageTrackingClient? client, MessageTrackingOperatingSystem? os, string? originalLink, MessagePlatform? platform, string userAgent, MessageTrackingGeo? geo, Guid messageId,
        string messageStream, DateTimeOffset receivedAt, string? tag, string? recipient) : base(recordType, client, os, platform, userAgent, geo, messageId, messageStream, receivedAt, tag, recipient)
    {
        ClickLocation = clickLocation;
        OriginalLink = originalLink;
    }

    /// <summary>Gets which body part contained the clicked link.</summary>
    public LinkClickLocation ClickLocation { [UsedImplicitly] get; }

    /// <summary>Gets the original clicked link.</summary>
    public string? OriginalLink { [UsedImplicitly] get; }
}
