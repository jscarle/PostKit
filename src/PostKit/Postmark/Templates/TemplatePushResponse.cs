using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Templates;

internal sealed class TemplatePushResponse
{
    [JsonPropertyName("TotalCount")]
    public int? TotalCount { get; [UsedImplicitly] init; }

    [JsonPropertyName("Templates")]
    public List<TemplatePushChangeResponse?>? Templates { get; [UsedImplicitly] init; }
}