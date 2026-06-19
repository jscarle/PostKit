using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Templates;

internal sealed class TemplateListResponse
{
    [JsonPropertyName("TotalCount")]
    public int? TotalCount { get; [UsedImplicitly] init; }

    [JsonPropertyName("Templates")]
    public List<TemplateSummaryResponse?>? Templates { get; [UsedImplicitly] init; }
}