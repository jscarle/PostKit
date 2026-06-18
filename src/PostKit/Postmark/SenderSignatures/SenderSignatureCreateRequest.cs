using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.SenderSignatures;

internal sealed class SenderSignatureCreateRequest
{
    [JsonPropertyName("FromEmail")]
    public required string FromEmail { [UsedImplicitly] get; init; }

    [JsonPropertyName("Name")]
    public required string Name { [UsedImplicitly] get; init; }

    [JsonPropertyName("ReplyToEmailAddress")]
    public string? ReplyToEmailAddress { [UsedImplicitly] get; init; }
}