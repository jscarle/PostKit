using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.SenderSignatures;

internal sealed class SenderSignatureEditRequest
{
    [JsonPropertyName("Name")]
    public string? Name { [UsedImplicitly] get; init; }

    [JsonPropertyName("ReplyToEmailAddress")]
    public string? ReplyToEmailAddress { [UsedImplicitly] get; init; }
}