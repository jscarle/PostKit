using System.Text.Json.Serialization;

namespace PostKit.Postmark.Email;

internal sealed class EmailAttachmentRequest
{
    [JsonPropertyName("Name")]
    public string? Name { get; set; }

    [JsonPropertyName("ContentID")]
    public string? ContentId { get; set; }

    [JsonPropertyName("ContentType")]
    public string? ContentType { get; set; }

    [JsonPropertyName("Content")]
    public string? Content { get; set; }
}
