using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Templates;

internal sealed class TemplateValidationContentResponse
{
    [JsonPropertyName("ContentIsValid")]
    public bool? ContentIsValid { get; [UsedImplicitly] init; }

    [JsonPropertyName("ValidationErrors")]
    public List<TemplateValidationErrorResponse?>? ValidationErrors { get; [UsedImplicitly] init; }

    [JsonPropertyName("RenderedContent")]
    public string? RenderedContent { get; [UsedImplicitly] init; }
}