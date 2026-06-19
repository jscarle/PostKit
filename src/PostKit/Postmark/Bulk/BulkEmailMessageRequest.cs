using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using PostKit.Postmark.Email;

namespace PostKit.Postmark.Bulk;

internal sealed class BulkEmailMessageRequest
{
    [JsonPropertyName("To")]
    public string? To { [UsedImplicitly] get; init; }

    [JsonPropertyName("Cc")]
    public string? Cc { [UsedImplicitly] get; init; }

    [JsonPropertyName("Bcc")]
    public string? Bcc { [UsedImplicitly] get; init; }

    [JsonPropertyName("TemplateModel")]
    public JsonNode? TemplateModel { [UsedImplicitly] get; init; }

    [JsonPropertyName("Metadata")]
    public IReadOnlyDictionary<string, string>? Metadata { [UsedImplicitly] get; init; }

    [JsonPropertyName("Headers")]
    public IReadOnlyList<EmailRequestHeader>? Headers { [UsedImplicitly] get; init; }
}