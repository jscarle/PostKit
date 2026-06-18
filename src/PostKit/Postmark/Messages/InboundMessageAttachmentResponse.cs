using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Postmark.Messages;

internal sealed class InboundMessageAttachmentResponse
{
    [JsonPropertyName("Name")]
    public string? Name { get; [UsedImplicitly] init; }

    [JsonPropertyName("ContentID")]
    public string? ContentId { get; [UsedImplicitly] init; }

    [JsonPropertyName("ContentType")]
    public string? ContentType { get; [UsedImplicitly] init; }

    [JsonPropertyName("ContentLength")]
    public long? ContentLength { get; [UsedImplicitly] init; }
}
