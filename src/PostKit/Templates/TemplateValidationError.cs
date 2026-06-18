using JetBrains.Annotations;

namespace PostKit.Templates;

/// <summary>Represents one template validation error.</summary>
public sealed record TemplateValidationError
{
    /// <summary>Gets the validation error message.</summary>
    public required string Message { [UsedImplicitly] get; init; }

    /// <summary>Gets the 1-based line number, when available.</summary>
    public required int? Line { [UsedImplicitly] get; init; }

    /// <summary>Gets the 1-based character position, when available.</summary>
    public required int? CharacterPosition { [UsedImplicitly] get; init; }
}