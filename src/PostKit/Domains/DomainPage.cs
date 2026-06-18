using JetBrains.Annotations;

namespace PostKit.Domains;

/// <summary>Represents a page of Postmark sender domains.</summary>
public sealed record DomainPage
{
    /// <summary>Gets the total number of matching domains.</summary>
    public required int TotalCount { [UsedImplicitly] get; init; }

    /// <summary>Gets the domains returned for this page.</summary>
    public required IReadOnlyList<PostmarkDomain> Domains { [UsedImplicitly] get; init; }
}