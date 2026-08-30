using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.InboundRules;

internal sealed class InboundRuleTriggerListResponse
{
    [JsonPropertyName("TotalCount")]
    public int? TotalCount { get; [UsedImplicitly] init; }

    [JsonPropertyName("InboundRules")]
    // ReSharper disable once CollectionNeverUpdated.Global
    public List<InboundRuleTriggerResponse?>? InboundRules { get; [UsedImplicitly] init; }
}
