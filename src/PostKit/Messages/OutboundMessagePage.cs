using System.Collections.ObjectModel;
using JetBrains.Annotations;

namespace PostKit.Messages;

/// <summary>Represents the results returned when searching outbound Postmark messages.</summary>
public sealed record OutboundMessagePage
{
    internal OutboundMessagePage(int totalCount, IReadOnlyCollection<OutboundMessage> messages)
    {
        TotalCount = totalCount;
        Messages = messages switch
        {
            ReadOnlyCollection<OutboundMessage> collection => collection,
            _ => new ReadOnlyCollection<OutboundMessage>(messages.ToList())
        };
    }

    /// <summary>Gets the total number of outbound messages matching the query.</summary>
    public int TotalCount { [UsedImplicitly] get; }

    /// <summary>Gets the current page of outbound messages returned by the query.</summary>
    public IReadOnlyCollection<OutboundMessage> Messages { [UsedImplicitly] get; }
}
