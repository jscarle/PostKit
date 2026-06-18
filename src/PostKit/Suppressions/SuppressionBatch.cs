using System.Collections.ObjectModel;
using JetBrains.Annotations;

namespace PostKit.Suppressions;

/// <summary>Represents the per-address results returned after creating or deleting suppressions.</summary>
public sealed record SuppressionBatch
{
    internal SuppressionBatch(IReadOnlyCollection<SuppressionResult> suppressions)
    {
        Suppressions = suppressions switch
        {
            ReadOnlyCollection<SuppressionResult> collection => collection,
            _ => new ReadOnlyCollection<SuppressionResult>(suppressions.ToList())
        };
    }

    /// <summary>Gets the per-address suppression create or delete results.</summary>
    public IReadOnlyCollection<SuppressionResult> Suppressions { [UsedImplicitly] get; }
}
