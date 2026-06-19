using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Email;

internal sealed class EmailRequestAttachment
{
    [JsonPropertyName("Name")]
    public string? Name { [UsedImplicitly] get; set; }

    [JsonPropertyName("ContentID")]
    public string? ContentId { [UsedImplicitly] get; set; }

    [JsonPropertyName("ContentType")]
    public string? ContentType { [UsedImplicitly] get; set; }

    [JsonPropertyName("Content")]
    public string? Content { [UsedImplicitly] get; set; }
}