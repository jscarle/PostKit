using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Templates;

internal sealed class TemplateValidationRequest
{
    [JsonPropertyName("Subject")]
    public string? Subject { [UsedImplicitly] get; init; }

    [JsonPropertyName("HtmlBody")]
    public string? HtmlBody { [UsedImplicitly] get; init; }

    [JsonPropertyName("TextBody")]
    public string? TextBody { [UsedImplicitly] get; init; }

    [JsonPropertyName("TestRenderModel")]
    public JsonNode? TestRenderModel { [UsedImplicitly] get; init; }

    [JsonPropertyName("InlineCssForHtmlTestRender")]
    public bool? InlineCssForHtmlTestRender { [UsedImplicitly] get; init; }

    [JsonPropertyName("TemplateType")]
    public string? TemplateType { [UsedImplicitly] get; init; }

    [JsonPropertyName("LayoutTemplate")]
    public string? LayoutTemplate { [UsedImplicitly] get; init; }
}