using JetBrains.Annotations;

namespace PostKit.Servers;

/// <summary>Represents a page of Postmark servers.</summary>
public sealed record ServerPage
{
    /// <summary>Gets the total number of matching servers.</summary>
    public required int TotalCount { [UsedImplicitly] get; init; }

    /// <summary>Gets the servers returned for this page.</summary>
    public required IReadOnlyList<PostmarkServer> Servers { [UsedImplicitly] get; init; }
}