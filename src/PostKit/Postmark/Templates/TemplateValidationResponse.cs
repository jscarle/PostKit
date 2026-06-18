using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Templates;

internal sealed class TemplateValidationResponse
{
    [JsonPropertyName("AllContentIsValid")]
    public bool? AllContentIsValid { get; [UsedImplicitly] init; }

    [JsonPropertyName("HtmlBody")]
    public TemplateValidationContentResponse? HtmlBody { get; [UsedImplicitly] init; }

    [JsonPropertyName("TextBody")]
    public TemplateValidationContentResponse? TextBody { get; [UsedImplicitly] init; }

    [JsonPropertyName("Subject")]
    public TemplateValidationContentResponse? Subject { get; [UsedImplicitly] init; }

    [JsonPropertyName("SuggestedTemplateModel")]
    public JsonNode? SuggestedTemplateModel { get; [UsedImplicitly] init; }
}
