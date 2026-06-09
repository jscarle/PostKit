using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Suppressions;

internal sealed class SuppressionRequestItem
{
    [JsonPropertyName("EmailAddress")]
    public required string EmailAddress { [UsedImplicitly] get; init; }
}
