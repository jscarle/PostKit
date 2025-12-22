using System.Text.Json.Serialization;

namespace PostKit.Postmark.Email;

internal sealed class EmailTemplateBatchRequest
{
    [JsonPropertyName("Messages")]
    public required IReadOnlyList<EmailRequest> Messages { get; init; }
}
