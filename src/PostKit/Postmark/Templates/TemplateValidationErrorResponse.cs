using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Templates;

internal sealed class TemplateValidationErrorResponse
{
    [JsonPropertyName("Message")]
    public string? Message { get; [UsedImplicitly] init; }

    [JsonPropertyName("Line")]
    public int? Line { get; [UsedImplicitly] init; }

    [JsonPropertyName("CharacterPosition")]
    public int? CharacterPosition { get; [UsedImplicitly] init; }
}