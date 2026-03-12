using System.Collections.ObjectModel;
using JetBrains.Annotations;

namespace PostKit.Bounces;

/// <summary>Represents delivery statistics returned by Postmark.</summary>
public sealed record GetDeliveryStatsResponse
{
    /// <summary>Gets the number of currently inactive email addresses.</summary>
    public int InactiveMails { [UsedImplicitly] get; }

    /// <summary>Gets the bounce counts grouped by type.</summary>
    public IReadOnlyList<BounceTypeCount> Bounces { [UsedImplicitly] get; }

    internal GetDeliveryStatsResponse(int inactiveMails, IReadOnlyList<BounceTypeCount> bounces)
    {
        InactiveMails = inactiveMails;
        Bounces = bounces switch
        {
            ReadOnlyCollection<BounceTypeCount> collection => collection,
            _ => new ReadOnlyCollection<BounceTypeCount>(bounces.ToList()),
        };
    }
}
