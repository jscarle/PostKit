using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.SenderSignatures;

internal sealed class SenderSignatureEditRequest
{
    [JsonPropertyName("Name")]
    public string? Name { [UsedImplicitly] get; init; }

    [JsonPropertyName("ReplyToEmail")]
    public string? ReplyToEmailAddress { [UsedImplicitly] get; init; }

    [JsonPropertyName("ReturnPathDomain")]
    public string? ReturnPathDomain { [UsedImplicitly] get; init; }

    [JsonPropertyName("ConfirmationPersonalNote")]
    public string? ConfirmationPersonalNote { [UsedImplicitly] get; init; }
}
