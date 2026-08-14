using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace PostKit.Webhooks;

/// <summary>Represents an attachment in a Postmark inbound webhook.</summary>
public sealed record InboundWebhookAttachment
{
    /// <summary>Gets the attachment file name.</summary>
    [JsonPropertyName("Name")]
    public required string Name { [UsedImplicitly] get; init; }

    /// <summary>Gets the Base64-encoded attachment content.</summary>
    [JsonPropertyName("Content")]
    public required string Content { [UsedImplicitly] get; init; }

    /// <summary>Gets the MIME content type.</summary>
    [JsonPropertyName("ContentType")]
    public required string ContentType { [UsedImplicitly] get; init; }

    /// <summary>Gets the decoded content length in bytes reported by Postmark.</summary>
    [JsonPropertyName("ContentLength")]
    public required long ContentLength { [UsedImplicitly] get; init; }

    /// <summary>Gets the content identifier when Postmark includes one.</summary>
    [JsonPropertyName("ContentID")]
    public string? ContentId { [UsedImplicitly] get; [UsedImplicitly] init; }
}
