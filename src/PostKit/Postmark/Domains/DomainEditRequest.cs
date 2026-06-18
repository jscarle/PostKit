using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Domains;

internal sealed class DomainEditRequest
{
    [JsonPropertyName("ReturnPathDomain")]
    public required string ReturnPathDomain { [UsedImplicitly] get; init; }
}