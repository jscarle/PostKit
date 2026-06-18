using JetBrains.Annotations;

namespace PostKit.Templates;

/// <summary>Represents one template change in a push result.</summary>
public sealed record TemplatePushChange
{
    /// <summary>Gets the change action.</summary>
    public required TemplatePushAction Action { [UsedImplicitly] get; init; }

    /// <summary>Gets the template ID.</summary>
    public required long TemplateId { [UsedImplicitly] get; init; }

    /// <summary>Gets the template alias, when one is set.</summary>
    public required string? Alias { [UsedImplicitly] get; init; }

    /// <summary>Gets the template name.</summary>
    public required string Name { [UsedImplicitly] get; init; }

    /// <summary>Gets the template type.</summary>
    public required TemplateType TemplateType { [UsedImplicitly] get; init; }
}
