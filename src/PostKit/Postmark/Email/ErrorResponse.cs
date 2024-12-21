using System.Text.Json.Serialization;

namespace PostKit.Postmark.Email;

internal sealed class ErrorResponse
{
    [JsonPropertyName("ErrorCode")]
    public required int ErrorCode { get; init; }

    [JsonPropertyName("Message")]
    public required string Message { get; init; }
}