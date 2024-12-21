using System.Text.Json.Serialization;

namespace PostKit.Postmark.Email;

internal sealed class EmailHeaderRequest
{
    [JsonPropertyName("Name")]
    public required string Name { get; init; }

    [JsonPropertyName("Value")]
    public required string Value { get; init; }
}
