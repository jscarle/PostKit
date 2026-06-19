using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Messages;

internal sealed class OutboundMessageAttachmentResponse
{
    [JsonPropertyName("Name")]
    public string? Name { get; [UsedImplicitly] init; }

    [JsonPropertyName("ContentID")]
    public string? ContentId { get; [UsedImplicitly] init; }

    [JsonPropertyName("ContentType")]
    public string? ContentType { get; [UsedImplicitly] init; }

    [JsonPropertyName("Content")]
    public string? Content { get; [UsedImplicitly] init; }
}