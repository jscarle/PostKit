using System.Text.Json.Serialization;

namespace PostKit.Postmark.Email;

internal sealed class EmailResponse
{
    [JsonPropertyName("To")]
    public required string To { get; init; }

    [JsonPropertyName("SubmittedAt")]
    public required DateTimeOffset SubmittedAt { get; init; }

    [JsonPropertyName("MessageID")]
    public required string MessageId { get; init; }

    [JsonPropertyName("ErrorCode")]
    public required int ErrorCode { get; init; }

    [JsonPropertyName("Message")]
    public required string Message { get; init; }
}