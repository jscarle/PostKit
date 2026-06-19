using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Suppressions;

internal sealed class SuppressionDumpResponse
{
    [JsonPropertyName("Suppressions")]
    public IReadOnlyList<SuppressionDumpItemResponse>? Suppressions { get; [UsedImplicitly] init; }
}