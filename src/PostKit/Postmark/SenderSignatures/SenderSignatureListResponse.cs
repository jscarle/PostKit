using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.SenderSignatures;

internal sealed class SenderSignatureListResponse
{
    [JsonPropertyName("TotalCount")]
    public int? TotalCount { get; [UsedImplicitly] init; }

    [JsonPropertyName("SenderSignatures")]
    public List<SenderSignatureSummaryResponse?>? SenderSignatures { get; [UsedImplicitly] init; }
}