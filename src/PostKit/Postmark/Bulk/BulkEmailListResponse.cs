using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Bulk;

internal sealed class BulkEmailListResponse
{
    [JsonPropertyName("Requests")]
    public IReadOnlyList<SendBulkEmailResponse?>? Requests { get; [UsedImplicitly] init; }

    [JsonPropertyName("PaginationKey")]
    public string? PaginationKey { get; [UsedImplicitly] init; }
}
