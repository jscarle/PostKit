using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.InboundRules;

internal sealed class InboundRuleTriggerResponse
{
    [JsonPropertyName("ID")]
    public long? Id { get; [UsedImplicitly] init; }

    [JsonPropertyName("Rule")]
    public string? Rule { get; [UsedImplicitly] init; }
}