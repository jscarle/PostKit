using System.Text.Json;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Stats;

internal sealed class OutboundStatsSeriesResponse
{
    [JsonPropertyName("Days")]
    public List<OutboundStatsDayResponse?>? Days { get; [UsedImplicitly] init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Totals { get; [UsedImplicitly] init; }
}
