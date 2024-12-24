using System.Text.Json.Serialization;

namespace PostKit.Postmark.Email;

internal sealed class EmailResponse : PostmarkResponse
{
    [JsonPropertyName("MessageID")]
    public string? MessageId { get; init; }

    [JsonPropertyName("SubmittedAt")]
    public DateTimeOffset? SubmittedAt { get; init; }

    [JsonPropertyName("To")]
    public string? To { get; init; }
}
