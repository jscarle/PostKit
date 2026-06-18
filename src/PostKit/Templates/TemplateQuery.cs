using JetBrains.Annotations;

namespace PostKit.Templates;

/// <summary>Represents filters for listing Postmark templates.</summary>
public sealed record TemplateQuery
{
    /// <summary>Gets the optional template type filter.</summary>
    public TemplateListType? TemplateType { [UsedImplicitly] get; init; }

    /// <summary>Gets the optional layout template alias filter.</summary>
    public string? LayoutTemplate { [UsedImplicitly] get; init; }
}