using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Templates;

internal sealed class TemplatePushRequest
{
    [JsonPropertyName("SourceServerID")]
    public required long SourceServerId { [UsedImplicitly] get; init; }

    [JsonPropertyName("DestinationServerID")]
    public required long DestinationServerId { [UsedImplicitly] get; init; }

    [JsonPropertyName("PerformChanges")]
    public required bool PerformChanges { [UsedImplicitly] get; init; }
}