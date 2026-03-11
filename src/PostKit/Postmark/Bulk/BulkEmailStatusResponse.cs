using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Bulk;

internal sealed class BulkEmailStatusResponse
{
    [JsonPropertyName("Id")]
    public string? Id { get; [UsedImplicitly] init; }

    [JsonPropertyName("SubmittedAt")]
    public DateTimeOffset? SubmittedAt { get; [UsedImplicitly] init; }

    [JsonPropertyName("TotalMessages")]
    public int? TotalMessages { get; [UsedImplicitly] init; }

    [JsonPropertyName("PercentageCompleted")]
    public double? PercentageCompleted { get; [UsedImplicitly] init; }

    [JsonPropertyName("Status")]
    public string? Status { get; [UsedImplicitly] init; }

    [JsonPropertyName("Subject")]
    public string? Subject { get; [UsedImplicitly] init; }
}
