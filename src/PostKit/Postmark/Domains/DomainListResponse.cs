using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Domains;

internal sealed class DomainListResponse
{
    [JsonPropertyName("TotalCount")]
    public int? TotalCount { get; [UsedImplicitly] init; }

    [JsonPropertyName("Domains")]
    public List<DomainResponse?>? Domains { get; [UsedImplicitly] init; }
}