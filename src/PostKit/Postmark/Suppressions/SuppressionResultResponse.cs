using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Suppressions;

internal sealed class SuppressionResultResponse
{
    [JsonPropertyName("EmailAddress")]
    public string? EmailAddress { get; [UsedImplicitly] init; }

    [JsonPropertyName("Status")]
    public string? Status { get; [UsedImplicitly] init; }

    [JsonPropertyName("Message")]
    public string? Message { get; [UsedImplicitly] init; }
}
