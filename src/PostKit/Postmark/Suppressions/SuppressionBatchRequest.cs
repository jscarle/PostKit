using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Suppressions;

internal sealed class SuppressionBatchRequest
{
    [JsonPropertyName("Suppressions")]
    public required IReadOnlyList<SuppressionRequestItem> Suppressions { [UsedImplicitly] get; init; }
}
