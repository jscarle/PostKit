using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Templates;

internal sealed class TemplatePushResponse
{
    [JsonPropertyName("TotalCount")]
    public int? TotalCount { get; [UsedImplicitly] init; }

    [JsonPropertyName("Templates")]
    // ReSharper disable once CollectionNeverUpdated.Global
    public List<TemplatePushChangeResponse?>? Templates { get; [UsedImplicitly] init; }
}
