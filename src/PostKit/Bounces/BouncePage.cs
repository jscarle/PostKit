using System.Collections.ObjectModel;
using JetBrains.Annotations;

namespace PostKit.Bounces;

/// <summary>Represents the results returned when searching Postmark bounces.</summary>
public sealed record BouncePage
{
    internal BouncePage(int totalCount, IReadOnlyCollection<Bounce> bounces)
    {
        TotalCount = totalCount;
        Bounces = bounces switch
        {
            ReadOnlyCollection<Bounce> collection => collection,
            _ => new ReadOnlyCollection<Bounce>(bounces.ToList())
        };
    }

    /// <summary>Gets the total number of bounces matching the query.</summary>
    public int TotalCount { [UsedImplicitly] get; }

    /// <summary>Gets the current page of bounces returned by the query.</summary>
    public IReadOnlyCollection<Bounce> Bounces { [UsedImplicitly] get; }
}