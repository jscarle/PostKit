using JetBrains.Annotations;

namespace PostKit.Templates;

/// <summary>Represents a template push result.</summary>
public sealed record TemplatePush
{
    /// <summary>Gets the number of template changes.</summary>
    public required int TotalCount { [UsedImplicitly] get; init; }

    /// <summary>Gets the template changes returned by Postmark.</summary>
    public required IReadOnlyList<TemplatePushChange> Templates { [UsedImplicitly] get; init; }
}
