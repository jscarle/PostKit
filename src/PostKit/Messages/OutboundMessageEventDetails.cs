using System.Collections.ObjectModel;
using JetBrains.Annotations;

namespace PostKit.Messages;

/// <summary>Represents event-specific details returned for an outbound message event.</summary>
public sealed record OutboundMessageEventDetails
{
    internal OutboundMessageEventDetails(IReadOnlyDictionary<string, string> values)
    {
        Values = values switch
        {
            ReadOnlyDictionary<string, string> dictionary => dictionary,
            _ => new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(values, StringComparer.Ordinal))
        };

        Summary = GetValue("Summary");
        BounceId = GetValue("BounceID");
        DeliveryMessage = GetValue("DeliveryMessage");
        DestinationServer = GetValue("DestinationServer");
        DestinationIp = GetValue("DestinationIP");
        Origin = GetValue("Origin");
        SuppressSending = GetValue("SuppressSending");
        Link = GetValue("Link");
        ClickLocation = GetValue("ClickLocation");
    }

    /// <summary>Gets the raw event detail values returned by Postmark.</summary>
    public IReadOnlyDictionary<string, string> Values { [UsedImplicitly] get; }

    /// <summary>Gets Postmark's summary for events that include one.</summary>
    public string? Summary { [UsedImplicitly] get; }

    /// <summary>Gets the bounce identifier for bounce events that include one.</summary>
    public string? BounceId { [UsedImplicitly] get; }

    /// <summary>Gets the delivery message for delivery-related events that include one.</summary>
    public string? DeliveryMessage { [UsedImplicitly] get; }

    /// <summary>Gets the destination server for delivery-related events that include one.</summary>
    public string? DestinationServer { [UsedImplicitly] get; }

    /// <summary>Gets the destination IP address for delivery-related events that include one.</summary>
    public string? DestinationIp { [UsedImplicitly] get; }

    /// <summary>Gets the origin for subscription-change events that include one.</summary>
    public string? Origin { [UsedImplicitly] get; }

    /// <summary>Gets the suppression flag for subscription-change events that include one.</summary>
    public string? SuppressSending { [UsedImplicitly] get; }

    /// <summary>Gets the clicked link for click events that include one.</summary>
    public string? Link { [UsedImplicitly] get; }

    /// <summary>Gets the click location for click events that include one.</summary>
    public string? ClickLocation { [UsedImplicitly] get; }

    private string? GetValue(string name)
    {
        return Values.TryGetValue(name, out var value) ? value : null;
    }
}
