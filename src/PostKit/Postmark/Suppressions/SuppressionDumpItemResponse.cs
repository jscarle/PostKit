using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Suppressions;

internal sealed class SuppressionDumpItemResponse
{
    [JsonPropertyName("EmailAddress")]
    public string? EmailAddress { get; [UsedImplicitly] init; }

    [JsonPropertyName("SuppressionReason")]
    public string? SuppressionReason { get; [UsedImplicitly] init; }

    [JsonPropertyName("Origin")]
    public string? Origin { get; [UsedImplicitly] init; }

    [JsonPropertyName("CreatedAt")]
    public DateTimeOffset? CreatedAt { get; [UsedImplicitly] init; }
}