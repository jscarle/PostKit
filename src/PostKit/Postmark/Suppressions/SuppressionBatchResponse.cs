using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Suppressions;

internal sealed class SuppressionBatchResponse
{
    [JsonPropertyName("Suppressions")]
    public IReadOnlyList<SuppressionResultResponse>? Suppressions { get; [UsedImplicitly] init; }
}
