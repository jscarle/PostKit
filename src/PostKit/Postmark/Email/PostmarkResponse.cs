using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Email;

internal class PostmarkResponse
{
    [JsonPropertyName("ErrorCode")]
    public required int ErrorCode { get; [UsedImplicitly] init; }

    [JsonPropertyName("Message")]
    public required string Message { get; [UsedImplicitly] init; }
}
