using System.Text.Json.Nodes;
using JetBrains.Annotations;

namespace PostKit.Templates;

/// <summary>Represents the result of validating template content.</summary>
public sealed record TemplateValidation
{
    /// <summary>Gets whether all submitted content was valid.</summary>
    public required bool AllContentIsValid { [UsedImplicitly] get; init; }

    /// <summary>Gets the HTML body validation result, when returned.</summary>
    public required TemplateValidationContent? HtmlBody { [UsedImplicitly] get; init; }

    /// <summary>Gets the text body validation result, when returned.</summary>
    public required TemplateValidationContent? TextBody { [UsedImplicitly] get; init; }

    /// <summary>Gets the subject validation result, when returned.</summary>
    public required TemplateValidationContent? Subject { [UsedImplicitly] get; init; }

    /// <summary>Gets the suggested template model returned by Postmark.</summary>
    public required JsonNode? SuggestedTemplateModel { [UsedImplicitly] get; init; }
}