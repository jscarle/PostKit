using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Email;

internal sealed class EmailResponse : PostmarkResponse
{
    [JsonPropertyName("MessageID")]
    public string? MessageId { get; [UsedImplicitly] init; }

    [JsonPropertyName("SubmittedAt")]
    public DateTimeOffset? SubmittedAt { get; [UsedImplicitly] init; }

    [JsonPropertyName("To")]
    public string? To { get; [UsedImplicitly] init; }
}
