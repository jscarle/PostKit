using JetBrains.Annotations;

namespace PostKit.Templates;

/// <summary>Represents a successful template deletion.</summary>
public sealed record TemplateDeletion
{
    /// <summary>Gets the success message returned by Postmark.</summary>
    public required string Message { [UsedImplicitly] get; init; }
}