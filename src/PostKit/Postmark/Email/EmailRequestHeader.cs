using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Email;

internal sealed class EmailRequestHeader
{
    [JsonPropertyName("Name")]
    public required string Name { [UsedImplicitly] get; init; }

    [JsonPropertyName("Value")]
    public required string Value { [UsedImplicitly] get; init; }
}