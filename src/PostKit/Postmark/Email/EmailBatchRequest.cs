using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Email;

internal sealed class EmailBatchRequest
{
    [JsonPropertyName("Messages")]
    public required IReadOnlyList<EmailRequest> Messages { [UsedImplicitly] get; init; }
}
