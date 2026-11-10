using System.Collections.ObjectModel;
using JetBrains.Annotations;
using PostKit.Common;
using PostKit.Suppressions;

namespace PostKit.Messages;

/// <summary>Represents event-specific details returned for an outbound message event.</summary>
public sealed record OutboundMessageEventDetails
{
    internal OutboundMessageEventDetails(IReadOnlyDictionary<string, string> values, long? bounceId, bool? suppressSending, SuppressionOrigin? origin, LinkClickLocation? clickLocation)
    {
        Values = values switch
        {
            ReadOnlyDictionary<string, string> dictionary => dictionary,
            _ => new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(values, StringComparer.Ordinal))
        };

        Summary = GetValue("Summary");
        BounceId = bounceId;
        DeliveryMessage = GetValue("DeliveryMessage");
        DestinationServer = GetValue("DestinationServer");
        DestinationIp = GetValue("DestinationIP");
        Origin = origin;
        SuppressSending = suppressSending;
        Link = GetValue("Link");
        ClickLocation = clickLocation;
    }

    /// <summary>Gets the raw event detail values returned by Postmark.</summary>
    public IReadOnlyDictionary<string, string> Values { [UsedImplicitly] get; }

    /// <summary>Gets Postmark's summary for events that include one.</summary>
    public string? Summary { [UsedImplicitly] get; }

    /// <summary>Gets the bounce identifier for bounce events that include one.</summary>
    public long? BounceId { [UsedImplicitly] get; }

    /// <summary>Gets the delivery message for delivery-related events that include one.</summary>
    public string? DeliveryMessage { [UsedImplicitly] get; }

    /// <summary>Gets the destination server for delivery-related events that include one.</summary>
    public string? DestinationServer { [UsedImplicitly] get; }

    /// <summary>Gets the destination IP address for delivery-related events that include one.</summary>
    public string? DestinationIp { [UsedImplicitly] get; }

    /// <summary>Gets the origin for subscription-change events that include one.</summary>
    public SuppressionOrigin? Origin { [UsedImplicitly] get; }

    /// <summary>Gets the suppression flag for subscription-change events that include one.</summary>
    public bool? SuppressSending { [UsedImplicitly] get; }

    /// <summary>Gets the clicked link for click events that include one.</summary>
    public string? Link { [UsedImplicitly] get; }

    /// <summary>Gets the click location for click events that include one.</summary>
    public LinkClickLocation? ClickLocation { [UsedImplicitly] get; }

    private string? GetValue(string name)
    {
        return Values.TryGetValue(name, out var value) ? value : null;
    }
}