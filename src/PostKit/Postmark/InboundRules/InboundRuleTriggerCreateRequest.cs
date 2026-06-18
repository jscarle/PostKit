using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.InboundRules;

internal sealed class InboundRuleTriggerCreateRequest
{
    [JsonPropertyName("Rule")]
    public required string Rule { [UsedImplicitly] get; init; }
}
