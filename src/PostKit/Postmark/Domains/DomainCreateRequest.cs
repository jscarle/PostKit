using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Domains;

internal sealed class DomainCreateRequest
{
    [JsonPropertyName("Name")]
    public required string Name { [UsedImplicitly] get; init; }

    [JsonPropertyName("ReturnPathDomain")]
    public string? ReturnPathDomain { [UsedImplicitly] get; init; }
}
