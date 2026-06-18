using JetBrains.Annotations;

namespace PostKit.Templates;

/// <summary>Represents a page of Postmark templates.</summary>
public sealed record TemplatePage
{
    /// <summary>Gets the total number of matching templates.</summary>
    public required int TotalCount { [UsedImplicitly] get; init; }

    /// <summary>Gets the templates returned for this page.</summary>
    public required IReadOnlyList<TemplateSummary> Templates { [UsedImplicitly] get; init; }
}