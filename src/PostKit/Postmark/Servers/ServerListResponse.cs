using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Servers;

internal sealed class ServerListResponse
{
    [JsonPropertyName("TotalCount")]
    public int? TotalCount { get; [UsedImplicitly] init; }

    [JsonPropertyName("Servers")]
    public List<ServerResponse?>? Servers { get; [UsedImplicitly] init; }
}