using System.Collections.ObjectModel;
using JetBrains.Annotations;

namespace PostKit.Bounces;

/// <summary>Represents the results returned when searching Postmark bounces.</summary>
public sealed record GetBouncesResponse
{
    /// <summary>Gets the total number of bounces matching the query.</summary>
    public int TotalCount { [UsedImplicitly] get; }

    /// <summary>Gets the current page of bounces returned by the query.</summary>
    public IReadOnlyList<Bounce> Bounces { [UsedImplicitly] get; }

    internal GetBouncesResponse(int totalCount, IReadOnlyList<Bounce> bounces)
    {
        TotalCount = totalCount;
        Bounces = bounces switch
        {
            ReadOnlyCollection<Bounce> collection => collection,
            _ => new ReadOnlyCollection<Bounce>(bounces.ToList()),
        };
    }
}
