using JetBrains.Annotations;

namespace PostKit.Templates;

/// <summary>Represents validation details for one template content field.</summary>
public sealed record TemplateValidationContent
{
    /// <summary>Gets whether the content field was valid.</summary>
    public required bool ContentIsValid { [UsedImplicitly] get; init; }

    /// <summary>Gets the validation errors returned by Postmark.</summary>
    public required IReadOnlyList<TemplateValidationError> ValidationErrors { [UsedImplicitly] get; init; }

    /// <summary>Gets the rendered content returned by Postmark, when available.</summary>
    public required string? RenderedContent { [UsedImplicitly] get; init; }
}