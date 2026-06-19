using JetBrains.Annotations;

namespace PostKit.SenderSignatures;

/// <summary>Represents a page of Postmark sender signatures.</summary>
public sealed record SenderSignaturePage
{
    /// <summary>Gets the total number of matching sender signatures.</summary>
    public required int TotalCount { [UsedImplicitly] get; init; }

    /// <summary>Gets the sender signatures returned for this page.</summary>
    public required IReadOnlyList<SenderSignatureSummary> SenderSignatures { [UsedImplicitly] get; init; }
}