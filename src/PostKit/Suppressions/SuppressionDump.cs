using System.Collections.ObjectModel;
using JetBrains.Annotations;

namespace PostKit.Suppressions;

/// <summary>Represents suppressions returned for a Postmark message stream.</summary>
public sealed record SuppressionDump
{
    internal SuppressionDump(IReadOnlyCollection<Suppression> suppressions)
    {
        Suppressions = suppressions switch
        {
            ReadOnlyCollection<Suppression> collection => collection,
            _ => new ReadOnlyCollection<Suppression>(suppressions.ToList())
        };
    }

    /// <summary>Gets the suppressions returned by the query.</summary>
    public IReadOnlyCollection<Suppression> Suppressions { [UsedImplicitly] get; }
}