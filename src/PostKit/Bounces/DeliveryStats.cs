using System.Collections.ObjectModel;
using JetBrains.Annotations;

namespace PostKit.Bounces;

/// <summary>Represents delivery statistics returned by Postmark.</summary>
public sealed record DeliveryStats
{
    /// <summary>Gets the number of currently inactive email addresses.</summary>
    public int InactiveMails { [UsedImplicitly] get; }

    /// <summary>Gets the bounce counts grouped by type.</summary>
    public IReadOnlyCollection<BounceSummary> Bounces { [UsedImplicitly] get; }

    internal DeliveryStats(int inactiveMails, IReadOnlyCollection<BounceSummary> bounces)
    {
        InactiveMails = inactiveMails;
        Bounces = bounces switch
        {
            ReadOnlyCollection<BounceSummary> collection => collection,
            _ => new ReadOnlyCollection<BounceSummary>(bounces.ToList()),
        };
    }
}
