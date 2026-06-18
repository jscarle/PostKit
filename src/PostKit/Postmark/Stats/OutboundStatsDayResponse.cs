using System.Text.Json;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Stats;

internal sealed class OutboundStatsDayResponse
{
    [JsonPropertyName("Date")]
    public DateOnly? Date { get; [UsedImplicitly] init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Counts { get; [UsedImplicitly] init; }
}