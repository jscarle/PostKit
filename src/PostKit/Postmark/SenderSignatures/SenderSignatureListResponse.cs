using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.SenderSignatures;

internal sealed class SenderSignatureListResponse
{
    [JsonPropertyName("TotalCount")]
    public int? TotalCount { get; [UsedImplicitly] init; }

    [JsonPropertyName("SenderSignatures")]
    // ReSharper disable once CollectionNeverUpdated.Global
    public List<SenderSignatureSummaryResponse?>? SenderSignatures { get; [UsedImplicitly] init; }
}
